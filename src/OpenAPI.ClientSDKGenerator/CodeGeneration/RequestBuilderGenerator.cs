namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class RequestBuilderGenerator(string @namespace)
{
    internal SourceCode Generate() =>
        new("RequestBuilder.g.cs", 
$$"""
using Corvus.Json;
using OpenAPI.ParameterStyleParsers;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json.Nodes;

namespace {{@namespace}};

internal sealed class RequestBuilder(HttpClient httpClient)
{
    private static readonly ConcurrentDictionary<string, IParameterValueParser> ParserCache = new();
    private const string ParameterValueParserVersion = "2.0";
    
    private readonly Dictionary<string, string> _pathParameters = new();
    internal void AddPathParameter<T>(string name, T value, string parameterSpecificationAsJson)
        where T : struct, IJsonValue
    {
        _pathParameters[name] = Serialize(value, parameterSpecificationAsJson);
    }

    internal Task SendAsync(string pathTemplate, 
        string httpMethod, 
        CancellationToken cancellation = default)
    {
        var path = _pathParameters.Aggregate(pathTemplate, (uri, parameter) => 
            uri.Replace("{" + parameter.Key + "}", parameter.Value));
        return httpClient.SendAsync(new HttpRequestMessage
        {
            Method = new HttpMethod(httpMethod),
            RequestUri = new Uri(path) 
        }, cancellation);
    }
    
    private string Serialize<TValue>(TValue value, string parameterSpecificationAsJson)
        where TValue : struct, IJsonValue
    {
        var parser = ParserCache.GetOrAdd(parameterSpecificationAsJson, 
            _ => ParameterValueParserFactory.OpenApi(ParameterValueParserVersion, parameterSpecificationAsJson));        
        var jsonValue = value.Serialize();
        return parser.Serialize(JsonNode.Parse(jsonValue));
    }
}
""");

}
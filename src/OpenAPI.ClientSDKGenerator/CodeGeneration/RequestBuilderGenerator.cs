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

namespace {{@namespace}};

internal sealed class RequestBuilder(HttpClient httpClient)
{
    private static readonly ConcurrentDictionary<string, IParameterValueParser> ParserCache = new();
    private const string ParameterValueParserVersion = "2.0";
    
    private readonly Dictionary<string, (IJsonValue Value, string ParameterSpecificationAsJson)> _pathParameters = new();
    internal void AddPathParameter(string name, IJsonValue value, string parameterSpecificationAsJson)
    {
        _pathParameters[name] = (value, parameterSpecificationAsJson);
    }

    internal Task SendAsync(string pathTemplate, 
        string httpMethod, 
        CancellationToken cancellation = default)
    {
        var path = _pathParameters.Aggregate(pathTemplate, (uri, parameter) => 
            uri.Replace("{" + parameter.Key + "}", Serialize(parameter.Value.Value, parameter.Value.ParameterSpecificationAsJson)));
        return httpClient.SendAsync(new HttpRequestMessage
        {
            Method = new Method(httpMethod),
            RequestUri = new Uri(path) 
        }, cancellation);
    }
    
    private string Serialize(IJsonValue value, string parameterSpecificationAsJson)
    {
        var parser = ParserCache.GetOrAdd(parameterSpecificationAsJson, 
            _ => ParameterValueParserFactory.OpenApi(ParameterValueParserVersion, parameterSpecificationAsJson));        
        var jsonValue = value.Serialize();
        return parser.Serialize(JsonNode.Parse(jsonValue));
    }
}
""");

}
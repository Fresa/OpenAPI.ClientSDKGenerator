using System;
using Microsoft.OpenApi;
using OpenAPI.ClientSDKGenerator.OpenApi;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class RequestBuilderGenerator(
    OpenApiSpecVersion openApiSpecVersion,
    ClientSdkGeneratorConfig generatorConfig,
    JsonValidationExceptionGenerator jsonValidationExceptionGenerator)
{
    internal SourceCode Generate(string @namespace) =>
        new("RequestBuilder.g.cs", 
$$"""
using Corvus.Json;
using OpenAPI.ParameterStyleParsers;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json.Nodes;

namespace {{@namespace}};

internal sealed class RequestBuilder(HttpClient httpClient, ClientSdkConfiguration configuration)
{
    private static readonly ConcurrentDictionary<string, IParameterValueParser> ParserCache = new();
    private const string ParameterValueParserVersion = "{{openApiSpecVersion.GetParameterVersion()}}";
    
    private readonly Dictionary<string, Func<string>> _pathParameters = new();
    internal void AddPathParameter<T>(
        string name, 
        T value,
        string schemaLocation, 
        string parameterSpecificationAsJson)
        where T : struct, IJsonValue
    {
        _validationContext = value.Validate(schemaLocation, true, _validationContext, _validationLevel);
        _pathParameters[name] = () => Serialize(value, parameterSpecificationAsJson);
    }

    internal Task SendAsync(string pathTemplate, 
        string httpMethod, 
        CancellationToken cancellation = default)
    {
        Validate();
        var path = _pathParameters.Aggregate(pathTemplate, (uri, parameter) => 
            uri.Replace("{" + parameter.Key + "}", parameter.Value()));
        return httpClient.SendAsync(new HttpRequestMessage
        {
            Method = new HttpMethod(httpMethod),
            RequestUri = new Uri(path, UriKind.Relative) 
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
    
    private ValidationLevel _validationLevel = ValidationLevel.{{generatorConfig.ValidationLevel.ToString()}};
    private ValidationContext _validationContext = ValidationContext.ValidContext.UsingStack().UsingResults();

    private void Validate()
    {
        if (_validationContext.IsValid)
            return;

        var validationResult = _validationContext.Results.WithLocation(configuration.OpenApiSpecificationUri);
        {{jsonValidationExceptionGenerator.CreateThrowJsonValidationExceptionInvocation("Response is not valid", "validationResult")}};
    }
}
""");

}
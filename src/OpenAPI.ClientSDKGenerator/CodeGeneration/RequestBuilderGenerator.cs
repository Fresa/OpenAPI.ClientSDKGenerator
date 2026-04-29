namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class RequestBuilderGenerator(string @namespace)
{
    internal SourceCode Generate() =>
        new("RequestBuilder.g.cs", 
$$"""
using System.Net.Http;

namespace {{@namespace}};

internal sealed class RequestBuilder(HttpClient httpClient)
{
    private readonly Dictionary<string, IJsonValue> _pathParameters = new();
    internal void AddPathParameter(string name, IJsonValue value)
    {
        _pathParameters[name] = value;
    }

    internal Task SendAsync(string pathTemplate, 
        HttpMethod httpMethod, 
        CancellationToken cancellation = default)
    {
        return httpClient.SendAsync(new HttpRequestMessage
        {
            Method = httpMethod,
            RequestUri = new Uri(pathTemplate) 
        }, cancellation);
    }
}
""");

}
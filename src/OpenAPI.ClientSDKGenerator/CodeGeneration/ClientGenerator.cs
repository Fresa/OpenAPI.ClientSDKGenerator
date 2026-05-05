using OpenAPI.ClientSDKGenerator.Extensions;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class ClientGenerator(string clientName, string @namespace)
{
    public string ClassName { get; } = clientName.ToPascalCase();
    public string Namespace { get; } = @namespace;

    internal SourceCode Generate()
    {
        return new SourceCode($"{ClassName}.g.cs", $$"""
using System.Net.Http;

namespace {{Namespace}};

internal sealed partial class {{ClassName}}(HttpClient httpClient)
{
}
""");
    }

    public PathsGenerator GetPathsGenerator() => new(this);
}
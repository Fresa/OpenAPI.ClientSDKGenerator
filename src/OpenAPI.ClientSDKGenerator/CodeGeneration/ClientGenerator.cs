using OpenAPI.ClientSDKGenerator.Extensions;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal static class ClientGenerator
{
    internal static SourceCode Generate(string clientName, string rootNamespace)
    {
        var name = clientName.ToPascalCase();
        return new SourceCode($"{name}.g.cs", $$"""
using System.Net.Http;

namespace {{rootNamespace}};

internal sealed class {{name}}(HttpClient httpClient)
{

}
""");
    }
}
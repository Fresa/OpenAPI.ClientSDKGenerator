using System.IO;
using Corvus.Json;
using Microsoft.CodeAnalysis;
using OpenAPI.ClientSDKGenerator.Extensions;
using OpenAPI.ClientSDKGenerator.OpenApi;

namespace OpenAPI.ClientSDKGenerator;

internal sealed class ClientSdkGeneratorConfig(AdditionalText openApiSpecification, string ns)
{
    public AdditionalText OpenApiSpecification { get; } = openApiSpecification;
    public string Namespace { get; } = ns;
    
    internal OpenApiSpecification LoadOpenApiSpecification()
    {
        var format = Path.GetExtension(OpenApiSpecification.Path)
            .TrimStart('.')
            .ToLowerInvariant();
        var stream = OpenApiSpecification.AsStream();

        var (document, version) = stream.LoadOpenApiDocument(format);
        var openApiUri = new JsonReference(document.BaseUri.ToString());
        var jsonDocument = stream.ParseJsonDocument(format);

        return new OpenApiSpecification(document, version, openApiUri, jsonDocument);
    }
}
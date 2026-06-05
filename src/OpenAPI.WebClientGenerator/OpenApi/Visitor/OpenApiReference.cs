using System.Text.Json;
using Corvus.Json;

namespace OpenAPI.WebClientGenerator.OpenApi.Visitor;

internal sealed class OpenApiReference<T>(T document, JsonDocument openApiDocument, JsonReference documentReference)
{
    internal T Document { get; } = document;
    internal JsonDocument OpenApiDocument { get; } = openApiDocument;
    internal JsonReference DocumentReference { get; } = documentReference;
}
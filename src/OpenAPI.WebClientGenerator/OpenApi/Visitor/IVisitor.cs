using OpenAPI.WebClientGenerator.Json;

namespace OpenAPI.WebClientGenerator.OpenApi.Visitor;

internal interface IVisitor
{
    internal JsonPointer Pointer { get; }
}
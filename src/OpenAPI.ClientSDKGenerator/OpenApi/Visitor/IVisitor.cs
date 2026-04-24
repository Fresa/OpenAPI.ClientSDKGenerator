using OpenAPI.ClientSDKGenerator.Json;

namespace OpenAPI.ClientSDKGenerator.OpenApi.Visitor;

internal interface IVisitor
{
    internal JsonPointer Pointer { get; }
}
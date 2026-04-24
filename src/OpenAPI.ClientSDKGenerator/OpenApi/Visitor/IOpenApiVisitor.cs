using Microsoft.OpenApi;

namespace OpenAPI.ClientSDKGenerator.OpenApi.Visitor;

internal interface IOpenApiVisitor
{
    public IOpenApiPathItemVisitor Visit(IOpenApiPathItem path);
}
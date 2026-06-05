using Microsoft.OpenApi;

namespace OpenAPI.WebClientGenerator.OpenApi.Visitor;

internal interface IOpenApiVisitor
{
    public IOpenApiPathItemVisitor Visit(IOpenApiPathItem path);
}
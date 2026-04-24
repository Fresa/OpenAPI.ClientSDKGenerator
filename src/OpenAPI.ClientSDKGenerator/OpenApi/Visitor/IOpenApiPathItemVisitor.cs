using System.Net.Http;
using Corvus.Json;
using Microsoft.OpenApi;

namespace OpenAPI.ClientSDKGenerator.OpenApi.Visitor;

internal interface IOpenApiPathItemVisitor : IVisitor
{
    JsonReference GetSchemaReference(IOpenApiParameter parameter);
    IOpenApiOperationVisitor Visit(HttpMethod parameter);
    
}
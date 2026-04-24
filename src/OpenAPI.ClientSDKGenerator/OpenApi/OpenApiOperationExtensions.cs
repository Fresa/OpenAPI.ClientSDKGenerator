using System.Collections.Generic;
using Microsoft.OpenApi;

namespace OpenAPI.ClientSDKGenerator.OpenApi;

internal static class OpenApiOperationExtensions
{
    internal static IList<IOpenApiParameter> GetParameters(this OpenApiOperation operation) =>
        operation.Parameters ?? [];
}
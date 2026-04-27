using System.Collections.Generic;
using System.Net.Http;
using Microsoft.OpenApi;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal interface IEntityGenerator
{
    void AddOperation(string pathExpression, KeyValuePair<HttpMethod, OpenApiOperation> operation,
        IEnumerable<ParameterGenerator> parameterGenerators);
}
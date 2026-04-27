using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class OperationGenerator(string pathExpression, OpenApiOperation operation, IEnumerable<ParameterGenerator> parameterGenerators)
{
    public string PathExpression { get; } = pathExpression;
    public OpenApiOperation Operation { get; } = operation;
    public ParameterGenerator[] Parameters { get; } = parameterGenerators.ToArray();
}
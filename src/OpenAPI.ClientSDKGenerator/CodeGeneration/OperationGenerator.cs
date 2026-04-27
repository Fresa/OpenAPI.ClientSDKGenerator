using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class OperationGenerator(
    OpenApiOperation operation, 
    IEnumerable<ParameterGenerator> parameterGenerators)
{
    public OpenApiOperation Operation { get; } = operation;
    public ParameterGenerator[] Parameters { get; } = parameterGenerators.ToArray();
}
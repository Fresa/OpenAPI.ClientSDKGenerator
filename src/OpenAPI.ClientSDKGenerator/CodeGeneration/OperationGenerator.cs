using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class OperationGenerator(
    OpenApiOperation operation,
    IEnumerable<ParameterGenerator> parameterGenerators,
    RequestBodyGenerator requestBodyGenerator)
{
    public OpenApiOperation Operation { get; } = operation;
    public RequestBodyGenerator RequestBodyGenerator { get; } = requestBodyGenerator;
    public ParameterGenerator[] Parameters { get; } = parameterGenerators.ToArray();
}
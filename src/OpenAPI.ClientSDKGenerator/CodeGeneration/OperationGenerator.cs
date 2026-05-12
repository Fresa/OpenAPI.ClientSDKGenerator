using Microsoft.OpenApi;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class OperationGenerator(
    OpenApiOperation operation,
    ParameterGenerator[] parameterGenerators,
    RequestBodyGenerator requestBodyGenerator)
{
    public OpenApiOperation Operation { get; } = operation;
    public RequestBodyGenerator RequestBodyGenerator { get; } = requestBodyGenerator;
    public QueryGenerator QueryGenerator { get; } = new(parameterGenerators);
}
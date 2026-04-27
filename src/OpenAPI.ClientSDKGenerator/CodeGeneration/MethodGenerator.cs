using System.Collections.Generic;
using System.Net.Http;
using Microsoft.OpenApi;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class MethodGenerator(string pathExpression, ParameterGenerator[] parameters)
{
    public string PathExpression { get; } = pathExpression;
    public ParameterGenerator[] Parameters { get; } = parameters;
    private readonly Dictionary<HttpMethod, OperationGenerator> _operationGenerators = new();

    public void AddOperation( 
        HttpMethod method, 
        OpenApiOperation operation,
        IEnumerable<ParameterGenerator> parameterGenerators)
    {
        _operationGenerators.Add(method, 
            new OperationGenerator(operation, parameterGenerators));
    }
}
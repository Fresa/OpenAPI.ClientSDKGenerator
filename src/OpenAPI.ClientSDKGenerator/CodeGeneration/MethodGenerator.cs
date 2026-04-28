using System.Collections.Generic;
using System.Net.Http;
using Microsoft.OpenApi;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class MethodGenerator(string pathExpression, ParameterGenerator[] parameters)
{
    public string PathExpression { get; } = pathExpression;
    public ParameterGenerator[] Parameters { get; } = parameters;
    internal Dictionary<HttpMethod, OperationGenerator> Operations { get; } = new();

    public void AddOperation( 
        HttpMethod method, 
        OperationGenerator operation)
    {
        Operations.Add(method, operation);
    }
}
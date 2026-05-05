using System.Collections.Generic;
using System.Net.Http;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class MethodGenerator(string pathExpression, ParameterGenerator[] parameters)
{
    public string PathExpression { get; } = pathExpression;
    public ParameterGenerator[] Parameters { get; } = parameters;
    internal Dictionary<HttpMethod, OperationGenerator> Operations { get; } = new();
    internal Dictionary<string, EntityGenerator> Children { get; } = new();

    public void AddOperation(
        HttpMethod method,
        OperationGenerator operation)
    {
        Operations.Add(method, operation);
    }

    internal EntityGenerator AddEntity(string name)
    {
        if (Children.TryGetValue(name, out var entity))
            return entity;
        entity = new EntityGenerator(name);
        Children.Add(name, entity);
        return entity;
    }
}
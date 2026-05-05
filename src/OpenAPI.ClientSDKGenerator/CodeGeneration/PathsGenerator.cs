using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OpenAPI.ClientSDKGenerator.Extensions;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class PathsGenerator(ClientGenerator clientGenerator)
{
    // todo: make this configurable
    // The root entity name, i.e. the name used for path templates that start with a parameter, i.e. /{id}
    private const string RootEntityName = "_";
    private readonly ConcurrentDictionary<string, EntityGenerator> _entityGenerators = new();
    internal MethodGenerator GetMethodGenerator(string pathTemplate, ParameterGenerator[] parameters)
    {
        var parameterLookup =
            new ConcurrentDictionary<string, ParameterGenerator>(
                parameters.ToDictionary(generator => generator.ParameterName, generator => generator));
        
        var segments = pathTemplate
            .Split(['/'], StringSplitOptions.RemoveEmptyEntries)
            .ToArray();

        EntityGenerator? current = null;
        List<ParameterGenerator> currentParameters = [];
        foreach (var segment in segments)
        {
            if (segment.StartsWith("{"))
            {
                var parameterName = segment.TrimStart('{').TrimEnd('}');
                if (!parameterLookup.TryRemove(parameterName, out var parameter))
                {
                    throw new InvalidOperationException(
                        $"path contain parameter {parameterName} which is not defined in the parameter collection");
                }
                                
                currentParameters.Add(parameter);
                continue;
            }

            var methodGenerator = AddMethodToCurrentEntity();

            var entityName = segment.ToPascalCase();
            if (methodGenerator != null)
            {
                current = methodGenerator.AddEntity(entityName);
                continue;
            }

            // Client SDK name overlaps with one of the root entities
            if (entityName == clientGenerator.ClassName)
            {
                entityName = $"{entityName}_";
            }
            current = _entityGenerators.GetOrAdd(entityName, _ => new EntityGenerator(entityName));
        }

        return AddMethodToCurrentEntity() ?? throw new InvalidOperationException("path template is empty");

        MethodGenerator? AddMethodToCurrentEntity()
        {
            if (currentParameters.Any() && current == null)
            {
                // if the template starts with parameters we need to create a root entity
                current = _entityGenerators.GetOrAdd(RootEntityName, _ => new EntityGenerator(RootEntityName));
            }

            var methodGenerator = current?.AddMethod(pathTemplate, currentParameters.ToArray());
            currentParameters.Clear();
            return methodGenerator;
        }
    }

    internal IEnumerable<SourceCode> Generate() =>
        _entityGenerators.Values.SelectMany(entityGeneratorsValue => 
            entityGeneratorsValue.Generate(clientGenerator.Namespace, clientGenerator.ClassName));
}

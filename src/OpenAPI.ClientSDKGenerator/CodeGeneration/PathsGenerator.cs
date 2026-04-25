using System;
using System.Collections.Generic;
using System.Linq;
using OpenAPI.ClientSDKGenerator.Extensions;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class PathsGenerator(ClientGenerator clientGenerator)
{
    // todo: make this configurable
    // The root entity name, i.e. the name used for path templates that start with a parameter, i.e. /{id}
    private const string RootEntityName = "_";
    private readonly Dictionary<string, EntityGenerator> _entityGenerators = new();
    internal EntityGenerator GetEntityGenerator(string pathTemplate, ParameterGenerator[] parameters)
    {
        var segments = pathTemplate
            .Split('/')
            .ToArray();

        EntityGenerator? current = null;
        List<ParameterGenerator> currentParameters = [];
        foreach (var segment in segments)
        {
            if (segment.StartsWith("{"))
            {
                var parameterName = segment.TrimStart('{').TrimEnd('}');
                var parameter = parameters.FirstOrDefault(generator => generator.ParameterName == parameterName) ??
                                throw new InvalidOperationException(
                                    $"path contain parameter {parameterName} which is not defined in the parameter collection");
                currentParameters.Add(parameter);
                continue;
            }

            var entityName = segment.ToPascalCase();
            if (string.IsNullOrEmpty(entityName))
            {
                entityName = RootEntityName;
            }
            AddCurrentParameters();

            if (current != null)
            {
                current = current.AddEntity(entityName);
                continue;
            }
            
            if (_entityGenerators.TryGetValue(entityName, out current))
            {
                continue;
            };

            current = new EntityGenerator(entityName);
            _entityGenerators.Add(entityName, current);
        }

        AddCurrentParameters();
        return current ?? throw new InvalidOperationException("path template is empty");

        void AddCurrentParameters()
        {
            if (!currentParameters.Any())
            {
                return;
            }
            current?.AddParameters(currentParameters.ToArray());
            currentParameters.Clear();
        }
    }

    internal IEnumerable<SourceCode> Generate() =>
        _entityGenerators.Values.SelectMany(entityGeneratorsValue => 
            entityGeneratorsValue.Generate(clientGenerator.ClassName));
}

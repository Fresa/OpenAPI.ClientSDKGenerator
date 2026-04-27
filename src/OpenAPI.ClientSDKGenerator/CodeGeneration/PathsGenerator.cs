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
    internal EntityGenerator GetEntityGenerator(string pathTemplate)
    {
        var segments = pathTemplate
            .Split(['/'], StringSplitOptions.RemoveEmptyEntries)
            .ToArray();

        EntityGenerator? current = null;
        foreach (var segment in segments)
        {
            if (segment.StartsWith("{") && current == null)
            {
                // if the template starts with parameters we need to create a root entity
                current = _entityGenerators.GetOrAdd(RootEntityName, _ => new EntityGenerator(RootEntityName));
            }

            var entityName = segment.ToPascalCase();
            if (current != null)
            {
                current = current.AddEntity(entityName);
                continue;
            }

            // Client SDK name overlaps with one of the root entities
            if (entityName == clientGenerator.ClassName)
            {
                entityName = $"{entityName}_";
            }
            current = _entityGenerators.GetOrAdd(entityName, _ => new EntityGenerator(entityName));
        }

        return current ?? throw new InvalidOperationException("path template is empty");
    }

    internal IEnumerable<SourceCode> Generate() =>
        _entityGenerators.Values.SelectMany(entityGeneratorsValue => 
            entityGeneratorsValue.Generate(clientGenerator.Namespace, clientGenerator.ClassName));
}

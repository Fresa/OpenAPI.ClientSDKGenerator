using System.Collections.Generic;
using System.Linq;
using OpenAPI.ClientSDKGenerator.Extensions;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class EntityGenerator(string name)
{
    public string Name { get; } = name;
    private readonly Dictionary<string, EntityGenerator> _entityGenerators = new();
    private readonly Dictionary<string, ParameterGenerator[]> _methodSignatures = new();

    internal EntityGenerator AddEntity(string name)
    {
        if (_entityGenerators.TryGetValue(name, out var entity))
            return entity;
        entity = new EntityGenerator(name);
        _entityGenerators.Add(name, entity);
        return entity;
    }

    internal void AddParameters(params ParameterGenerator[] parameterGenerators)
    {
        var id = parameterGenerators.Aggregate("", (id, generator) => id + generator.FullyQualifiedTypeName);
        if (_methodSignatures.TryGetValue(id, out _))
            return;
        _methodSignatures.Add(id, parameterGenerators);
    }

    internal IEnumerable<SourceCode> Generate(params string[] outerClassNames)
    {
        var allNames = outerClassNames.Concat([Name]).ToArray();
        var fileName = string.Join(".", allNames);

        yield return new SourceCode($"{fileName}.g.cs", GenerateClass(allNames));

        foreach (var inner in _entityGenerators.Values
                     .SelectMany(entity => entity.Generate(allNames)))
        {
            yield return inner;
        }
    }

    private static string GenerateClass(IReadOnlyList<string> names)
    {
        if (names.Count == 0)
        {
            return string.Empty;
        }

        var name = names[0];
        var inner = GenerateClass(names.Skip(1).ToArray());

        return $$"""
            internal sealed partial class {{name}}
            {
            {{inner.Indent(4)}}
            }
            """;
    }
}
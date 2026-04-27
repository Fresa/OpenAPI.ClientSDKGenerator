using System;
using System.Collections.Generic;
using System.Linq;
using OpenAPI.ClientSDKGenerator.Extensions;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class EntityGenerator(string name)
{
    private readonly Dictionary<string, EntityGenerator> _entityGenerators = new();
    private readonly Dictionary<string, ParameterGenerator[]> _methodSignatures = new();
    private readonly string _className = $"{name.ToPascalCase()}Entity";
    
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

    internal IEnumerable<SourceCode> Generate(string @namespace, params string[] outerClassNames)
    {
        var fileName = string.Join(".", outerClassNames);

        yield return new SourceCode($"{fileName}.{_className}.g.cs", GenerateClass(@namespace, outerClassNames));

        foreach (var inner in _entityGenerators.Values
                     .SelectMany(entity => entity.Generate(@namespace, outerClassNames)))
        {
            yield return inner;
        }
    }

    private static string GenerateNestedClassStructure(IReadOnlyList<string> nestedClassNames, Func<string> content)
    {
        if (nestedClassNames.Count == 0)
        {
            return content();
        }

        var className = nestedClassNames[0];
        var inner = GenerateNestedClassStructure(nestedClassNames.Skip(1).ToArray(), content);
        return 
$$"""
internal sealed partial class {{className}}
{{{inner.Indent(4)}}
}
""";
    }
    
    private string GenerateClass(string @namespace, IReadOnlyList<string> nestedClassNames)
    {
        return 
$$"""
namespace {{@namespace}};
{{GenerateNestedClassStructure(nestedClassNames, () =>
$$"""
{{_methodSignatures.Values.AggregateToString(parameters =>
$$"""
internal {{_className}} {{name}}({{parameters.AggregateToString(parameter =>
$$"""
    {{parameter.FullyQualifiedTypeName}} {{parameter.ParameterName.ToCamelCase()}},
""").TrimEnd(',')}}) => 
    new({{parameters.AggregateToString(parameter =>
$"""
        {parameter.ParameterName.ToCamelCase()},
""").TrimEnd(',')}});
"""
)}}

internal sealed partial class {{_className}}
{{{_methodSignatures.Values.AggregateToString(parameters =>
$$"""
    internal {{_className}}({{parameters.AggregateToString(parameter =>
$$"""
        {{parameter.FullyQualifiedTypeName}} {{parameter.ParameterName.ToCamelCase()}},
""").TrimEnd(',')}})
    {
    }
"""
)}}
}
"""  
)}}

""";
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using OpenAPI.ClientSDKGenerator.Extensions;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class EntityGenerator(string name)
{
    private readonly Dictionary<string, EntityGenerator> _entityGenerators = new();
    private readonly Dictionary<string, MethodGenerator> _methodSignatures = new();
    private readonly string _className = name.ToPascalCase();
    
    internal EntityGenerator AddEntity(string name)
    {
        if (_entityGenerators.TryGetValue(name, out var entity))
            return entity;
        entity = new EntityGenerator(name);
        _entityGenerators.Add(name, entity);
        return entity;
    }

    internal MethodGenerator AddMethod(string pathExpression, params ParameterGenerator[] parameterGenerators)
    {
        var id = parameterGenerators.Aggregate("", (id, generator) => id + generator.FullyQualifiedTypeName);
        if (_methodSignatures.TryGetValue(id, out var methodGenerator))
            return methodGenerator;
        methodGenerator = new MethodGenerator(pathExpression, parameterGenerators);
        _methodSignatures.Add(id, methodGenerator);
        return methodGenerator;
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
{{_methodSignatures.Values.AggregateToString(methodGenerator =>
    {
        var className = _className + methodGenerator.Parameters.Length;
        return 
$$"""
internal {{className}} {{name}}({{GetMethodParameterList(methodGenerator)}}) => 
    new({{GetMethodArgumentList(methodGenerator).Indent(4).TrimStart()}});
         
internal sealed partial class {{className}}({{GetConstructorParameterList(methodGenerator)}})
{{{methodGenerator.Operations.AggregateToString(operation => 
$$"""
    internal Task {{operation.Key.Method.ToLower().ToPascalCase()}}Async(CancellationToken cancellation = default) =>
        httpClient.SendAsync(new HttpRequestMessage
        {
            Method = new Method("{{operation.Key.Method}}")
        }, cancellation);
    
""")}}
}

""";
    }
)}}
""")}}
""";
    }

    private static string GetMethodParameterList(MethodGenerator methodGenerator) =>
        methodGenerator.Parameters.AggregateToString(parameter =>
            $$"""
                  {{parameter.FullyQualifiedTypeName}} {{parameter.ParameterName.ToCamelCase()}},
              """).TrimEnd(',');
    
    private static string GetConstructorParameterList(MethodGenerator methodGenerator) =>
        methodGenerator.Parameters.AggregateToString("HttpClient httpClient", parameter =>
            $$"""
                  , {{parameter.FullyQualifiedTypeName}} {{parameter.ParameterName.ToCamelCase()}}
              """);
    
    private static string GetMethodArgumentList(MethodGenerator methodGenerator) =>
        methodGenerator.Parameters.AggregateToString("httpClient", parameter =>
            $$"""
                  , {{parameter.ParameterName.ToCamelCase()}}
              """);
    
}

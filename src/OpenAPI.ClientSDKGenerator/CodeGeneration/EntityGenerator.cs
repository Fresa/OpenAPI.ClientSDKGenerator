using System;
using System.Collections.Generic;
using System.Linq;
using OpenAPI.ClientSDKGenerator.Extensions;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class EntityGenerator(string name)
{
    private readonly Dictionary<string, MethodGenerator> _methodSignatures = new();
    private readonly string _className = name.ToPascalCase();

    internal MethodGenerator AddMethod(string pathExpression, params ParameterGenerator[] parameterGenerators)
    {
        var id = parameterGenerators.Aggregate("", (id, generator) => id + generator.FullyQualifiedTypeName);
        if (_methodSignatures.TryGetValue(id, out var methodGenerator))
            return methodGenerator;
        methodGenerator = new MethodGenerator(pathExpression, parameterGenerators);
        _methodSignatures.Add(id, methodGenerator);
        return methodGenerator;
    }

    internal IEnumerable<SourceCode> Generate(string @namespace, params string[] outerClassNames) =>
        Generate(@namespace, outerEntityNames: outerClassNames, outerClassNames: outerClassNames, rootEntity: true);

    private IEnumerable<SourceCode> Generate(
        string @namespace,
        string[] outerEntityNames,
        string[] outerClassNames,
        bool rootEntity)
    {
        var fileName = string.Join(".", outerEntityNames);

        yield return new SourceCode($"{fileName}.{_className}.g.cs", GenerateClass(@namespace, outerClassNames, rootEntity));

        var childEntityChain = outerEntityNames.Append(_className).ToArray();
        foreach (var methodGenerator in _methodSignatures.Values)
        {
            var parentClassName = _className + methodGenerator.Parameters.Length;
            var childClassChain = outerClassNames.Append(parentClassName).ToArray();
            foreach (var source in methodGenerator.Children.Values
                         .SelectMany(child => 
                             child.Generate(@namespace, childEntityChain, childClassChain, rootEntity: false)))
            {
                yield return source;
            }
        }
    }

    private static string GenerateNestedClassStructure(IReadOnlyList<string> nestedClassNames, Func<string> content, bool isRoot = false)
    {
        if (nestedClassNames.Count == 0)
        {
            return content().Trim();
        }

        var className = nestedClassNames[0];
        var inner = GenerateNestedClassStructure(nestedClassNames.Skip(1).ToArray(), content, isRoot);
        return 
$$"""
internal sealed partial class {{className}}
{
{{inner.Indent(4)}}
}
""";
    }
    
    private string GenerateClass(string @namespace, IReadOnlyList<string> nestedClassNames, bool rootEntity = false)
    {
        return 
$$"""
#nullable enable
using Corvus.Json;
using System.Net.Http.Headers;
using System.Text;

namespace {{@namespace}};
{{GenerateNestedClassStructure(nestedClassNames, () =>
$$"""
{{_methodSignatures.Values.AggregateToString(methodGenerator =>
    {
        var className = _className + methodGenerator.Parameters.Length;
        return 
$$"""
internal {{className}} {{name}}({{GetMethodParameterList(methodGenerator)}})
{{{(rootEntity ? 
"""

    var requestBuilder = new RequestBuilder(httpClient, _configuration);
""" : "")}}{{methodGenerator.Parameters.AggregateToString(parameter =>
$$""""
    requestBuilder.AddPathParameter("{{parameter.ParameterName}}",
        {{parameter.ParameterName.ToCamelCase()}},
        "{{parameter.SchemaLocation}}",
        """
        {{parameter.ParameterSpecificationAsJson.Indent(8).Trim()}}
        """);
"""").TrimEnd(',')}}
    return new(requestBuilder);
}

internal sealed partial class {{className}}(RequestBuilder requestBuilder)
{{{methodGenerator.Operations.AggregateToString(operation => 
$$"""
    internal Task {{operation.Key.Method.ToLower().ToPascalCase()}}Async({{
        (operation.Value.RequestBodyGenerator.HasBody ? "Content content, " : "")}}
        CancellationToken cancellation = default) =>
        requestBuilder.SendAsync(
            "{{methodGenerator.PathExpression}}",
            "{{operation.Key.Method}}", 
            {{(operation.Value.RequestBodyGenerator.HasBody ? "content.Get()" : "null")}},
            cancellation);
            
{{operation.Value.RequestBodyGenerator.GenerateClass().Indent(4)}}
""".TrimEnd())}}
}

""";
    }
)}}
""", rootEntity)}}
#nullable restore
""";
    }

    private static string GetMethodParameterList(MethodGenerator methodGenerator) =>
        methodGenerator.Parameters.AggregateToString(parameter =>
            $$"""
                  {{parameter.FullyQualifiedTypeName}} {{parameter.ParameterName.ToCamelCase()}},
              """).TrimEnd(',');
}

using System.Collections.Generic;
using System.Linq;
using OpenAPI.WebClientGenerator.Extensions;

namespace OpenAPI.WebClientGenerator.CodeGeneration;

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
            var entityClassName = _className + methodGenerator.Parameters.Length;
            var entityClassChain = outerClassNames.Append(entityClassName).ToArray();

            foreach (var operation in methodGenerator.Operations)
            {
                var verb = operation.Key.Method.ToLower().ToPascalCase();
                foreach (var source in operation.Value.ResponseGenerator.Generate(
                    @namespace,
                    nestingClassNames: entityClassChain,
                    className: $"{verb}Response"))
                {
                    yield return source;
                }
            }

            foreach (var source in methodGenerator.Children.Values
                         .SelectMany(child =>
                             child.Generate(@namespace, childEntityChain, entityClassChain, rootEntity: false)))
            {
                yield return source;
            }
        }
    }

    private string GenerateClass(string @namespace, IReadOnlyList<string> nestedClassNames, bool rootEntity = false)
    {
        return
$$"""
#nullable enable
using Corvus.Json;
using System.IO.Pipelines;
using System.Net.Http.Headers;
using System.Text;

namespace {{@namespace}};
{{NestedClassGenerator.Wrap(nestedClassNames, () =>
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

internal partial class {{className}}(RequestBuilder requestBuilder)
{{{methodGenerator.Operations.AggregateToString(operation => 
$$"""
    internal async Task<{{operation.Key.Method.ToLower().ToPascalCase()}}Response> {{operation.Key.Method.ToLower().ToPascalCase()}}Async({{
        (operation.Value.RequestBodyGenerator.HasBody ? "Content content," : "")}}{{
            new ParametersGenerator []
            {
                operation.Value.QueryGenerator,
                operation.Value.HeadersGenerator
            }.OrderBy(generator => generator.IsOptional)
            .Select(GetParameterArgumentExpression)
            .AggregateToString()
            .Indent(8)
            .TrimStart()
        }}{{(operation.Value.ResponseGenerator.GeneratesContent ? 
$"""

        Accept? accepts = null,
""" : "")}}
        CancellationToken cancellation = default)
    {{{
            new ParametersGenerator []
            {
                operation.Value.QueryGenerator,
                operation.Value.HeadersGenerator
            }
            .Select(GetParameterBuilderMethod)
            .AggregateToString()
            .Indent(8)
        }}{{(operation.Value.ResponseGenerator.GeneratesContent ? 
"""

        requestBuilder.AcceptMediaTypes(accepts?.MediaTypes ?? []);
""" : "")}}
        var response = await requestBuilder
            .SendAsync(
                "{{methodGenerator.PathExpression}}",
                "{{operation.Key.Method}}",
                {{(operation.Value.RequestBodyGenerator.HasBody ? "content.Get()" : "null")}},
                cancellation)
            .ConfigureAwait(false);
        return await {{operation.Key.Method.ToLower().ToPascalCase()}}Response.BindAsync(response, cancellation)
            .ConfigureAwait(false);
    }{{(operation.Value.ResponseGenerator.GeneratesContent ? 
$$"""
    
    internal sealed class Accept
    {
        private Accept() {}
        internal static Accept Content<T>()
            where T : {{operation.Key.Method.ToLower().ToPascalCase()}}Response.IAcceptContent =>
            new Accept().And<T>();

        internal Accept And<T>()
            where T : {{operation.Key.Method.ToLower().ToPascalCase()}}Response.IAcceptContent
        {
            _mediaTypes.Add(T.MediaType);
            return this;
        }
        
        private readonly List<MediaTypeWithQualityHeaderValue> _mediaTypes = [];
        internal MediaTypeWithQualityHeaderValue[] MediaTypes => _mediaTypes.ToArray();
    }
""" : "")}}
{{ new[] 
    { 
        operation.Value.RequestBodyGenerator.GenerateClass(),
        operation.Value.QueryGenerator.GenerateClass(), 
        operation.Value.HeadersGenerator.GenerateClass()
    }
    .AggregateToString()
    .Trim()
    .PrependNewline()
    .Indent(4)
    .TrimEnd()
}}
""")}}
}

""";
    }
)}}
""")}}
#nullable restore
""";
    }

    private static string GetMethodParameterList(MethodGenerator methodGenerator) =>
        methodGenerator.Parameters.AggregateToString(parameter =>
            $$"""
                  {{parameter.FullyQualifiedTypeName}} {{parameter.ParameterName.ToCamelCase()}},
              """).TrimEnd(',');

    private static string GetParameterBuilderMethod(ParametersGenerator parametersGenerator) =>
        parametersGenerator.IsEmpty
            ? string.Empty
            : $"{(parametersGenerator.IsOptional ?
                $"({parametersGenerator.ClassName.ToCamelCase()} ?? new())" : parametersGenerator.ClassName.ToCamelCase())}.AddTo(requestBuilder);";

    private static string GetParameterArgumentExpression(ParametersGenerator parametersGenerator)
    {
        if (parametersGenerator.IsEmpty)
        {
            return string.Empty;
        }

        var terny = parametersGenerator.IsOptional ? "?" : string.Empty;
        var defaultExpression = parametersGenerator.IsOptional ? " = null" : string.Empty;
        return $"{parametersGenerator.ClassName}{terny} {parametersGenerator.ClassName.ToCamelCase()}{defaultExpression},"; 
    }
}

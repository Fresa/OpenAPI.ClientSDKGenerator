using System.Linq;
using Microsoft.OpenApi;
using OpenAPI.ClientSDKGenerator.Extensions;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal abstract class ParametersGenerator(ParameterGenerator[] parameters)
{
    internal bool IsEmpty { get; } = parameters.Length == 0;
    internal string ClassName => IsEmpty ? string.Empty : parameters[0].Location.GetDisplayName().ToPascalCase();
    internal bool IsOptional { get; } = parameters.All(generator => !generator.IsParameterRequired);

    internal string GenerateClass()
    {
        if (parameters.Length == 0)
        {
            return string.Empty;
        }

        var className = ClassName;

        return
$$""""
internal sealed class {{className}}
{{{parameters.AggregateToString(parameter =>
$$"""
    internal {{(parameter.IsParameterRequired ? "required " : "")}}{{parameter.FullyQualifiedTypeName}} {{parameter.ParameterName.ToPascalCase()}} { get; init; }
""")}}

    internal RequestBuilder AddTo(RequestBuilder requestBuilder)
    {{{parameters.AggregateToString(parameter =>
$$""""
        requestBuilder.Add{{className}}("{{parameter.ParameterName}}",
            {{parameter.ParameterName.ToPascalCase()}},
            {{parameter.IsParameterRequired.ToString().ToLowerInvariant()}},
            "{{parameter.SchemaLocation}}",
            """
            {{parameter.ParameterSpecificationAsJson.Indent(12).Trim()}}
            """);
"""")}}
        return requestBuilder;
    }
}
"""";
    }
}
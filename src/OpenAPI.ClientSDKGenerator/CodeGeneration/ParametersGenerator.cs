using Microsoft.OpenApi;
using OpenAPI.ClientSDKGenerator.Extensions;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal abstract class ParametersGenerator(ParameterGenerator[] parameters)
{
    internal string GenerateClass()
    {
        if (parameters.Length == 0)
        {
            return string.Empty;
        }

        var className = parameters[0].Location.GetDisplayName().ToPascalCase();

        return
$$""""
internal sealed class {{className}}
{{{parameters.AggregateToString(parameter =>
$$"""
    internal {{(parameter.IsParameterRequired ? "required " : "")}}{{parameter.FullyQualifiedTypeName}} {{parameter.ParameterName.ToPascalCase()}} { get; init; }
""")}}

    internal void AddTo(RequestBuilder requestBuilder)
    {{{parameters.AggregateToString(parameter =>
$$""""
        requestBuilder.Add{{className}}("{{parameter.ParameterName}}",
            {{parameter.ParameterName.ToPascalCase()}},
            "{{parameter.SchemaLocation}}",
            """
            {{parameter.ParameterSpecificationAsJson.Indent(12).Trim()}}
            """);
"""")}}
    }
}
"""";
    }
}
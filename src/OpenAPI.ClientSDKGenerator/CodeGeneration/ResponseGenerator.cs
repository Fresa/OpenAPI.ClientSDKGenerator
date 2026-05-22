using System.Collections.Generic;
using System.Linq;
using OpenAPI.ClientSDKGenerator.Extensions;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class ResponseGenerator(
    List<ResponseContentGenerator> responseBodyGenerators
    )
{
    public SourceCode GenerateResponseClass(string @namespace, string path)
    {
        return new SourceCode($"{path}/Response.g.cs",
$$"""
#nullable enable
using Corvus.Json;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace {{@namespace}};

/// <summary>
/// Contains the operation's response objects
/// </summary>
internal abstract partial class Response
{{{Enumerable.Range(1, 5).AggregateToString(i => 
$$"""
    /// <summary>
    /// Check if status code is {{i}}xx
    /// </summary>
    /// <param name="code">Status code to match</param>
    /// <returns>true if code matches</returns>
    protected static bool Matches{{i}}xxStatusCode(int code) => 
        code >= {{i}}00 && code <= {{i}}99;
""")}}

    /// <summary>
    /// Validate the response
    /// </summary>
    /// <param name="validationLevel">Validation level</param>
    /// <returns>The validation result</returns>
    internal abstract ValidationContext Validate(ValidationLevel validationLevel);
    {{
    responseBodyGenerators.AggregateToString(generator => 
        generator.GenerateResponseContentClass()).Indent(4)
    }}
}
#nullable restore
""");
    }
}
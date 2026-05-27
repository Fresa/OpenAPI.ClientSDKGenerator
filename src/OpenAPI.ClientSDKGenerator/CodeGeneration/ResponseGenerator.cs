using System.Collections.Generic;
using System.Linq;
using OpenAPI.ClientSDKGenerator.Extensions;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class ResponseGenerator(
    List<ResponseContentGenerator> responseBodyGenerators
    )
{
    public string GenerateClass(string operation)
    {
        var className = $"{operation}Response";
        return 
$$"""
/// <summary>
/// Contains the operation's response objects
/// </summary>
internal abstract partial class {{className}}
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
        
    /// <summary>
    /// Read response content as json
    /// </summary>
    /// <param name="response">Response message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    protected static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);
        var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return document.RootElement.Clone();
    }
    {{
    responseBodyGenerators.AggregateToString(generator => 
        generator.GenerateResponseContentClass(className)).Indent(4)
    }}
}
""";
    }
}
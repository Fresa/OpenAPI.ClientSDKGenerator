using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.OpenApi;
using OpenAPI.ClientSDKGenerator.Extensions;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class ResponseContentGenerator
{
    private readonly List<ResponseBodyContentGenerator> _contentGenerators = [];
    // private readonly List<ResponseHeaderGenerator> _headerGenerators = [];
    private readonly string _responseClassName;
    private readonly string _responseStatusCodePattern;
    private readonly IOpenApiResponse _response;
    private readonly bool _hasExplicitStatusCode;
    private readonly bool _hasDefaultStatusCode;

    private ResponseContentGenerator(
        KeyValuePair<string, IOpenApiResponse> response)
    {
        var responseStatusCodePattern = response.Key.ToPascalCase();
        var classNamePrefix = Enum.TryParse<HttpStatusCode>(responseStatusCodePattern, out var statusCode)
            ? statusCode.ToString()
            : responseStatusCodePattern.First() switch
            {
                '1' => "Informational",
                '2' => "Successful",
                '3' => "Redirection",
                '4' => "ClientError",
                '5' => "ServerError",
                var chr when char.IsDigit(chr) => "X",
                _ => string.Empty
            };
        var responseClassName = $"{classNamePrefix}{responseStatusCodePattern}";
        
        _responseStatusCodePattern = responseStatusCodePattern;
        _responseClassName = responseClassName;
        _response = response.Value;
        _hasExplicitStatusCode = int.TryParse(_responseStatusCodePattern, out _);
        _hasDefaultStatusCode = _responseStatusCodePattern == "default";
        Precedence = _hasExplicitStatusCode ? 0 : _hasDefaultStatusCode ? 10 : 5;
    }
    
    internal int Precedence { get; }
    
    public ResponseContentGenerator(
        KeyValuePair<string, IOpenApiResponse> response,
        List<ResponseBodyContentGenerator> contentGenerators
        // List<ResponseHeaderGenerator> headerGenerators
        ) : this(response)
    {
        _contentGenerators = contentGenerators;
        // _headerGenerators = headerGenerators;
    }
    
    public string GenerateResponseContentClass()
    {
        // var anyHeaders = _headerGenerators.Any();
        // var anyRequiredHeader = _headerGenerators.Any(generator => generator.IsRequired);
        // var headerRequiredDirective = anyRequiredHeader ? "required " : "";
        // var defaultHeadersValueAssignment = anyRequiredHeader ? "" : " = new();";
        return 
$$$"""
{{{_response.Description.AsComment("summary", "para")}}}
internal abstract class {{{_responseClassName}}} : Response
{{{{
    _contentGenerators.AggregateToString(generator =>
        generator.GenerateResponseClass(_responseClassName)).Indent(4)
    }}}
    
    /// <summary>
    /// Response for unknown content
    /// </summary>
    internal sealed class Unknown : {{{_responseClassName}}}
    {
        internal Corvus.Json.JsonAny Content { get; }
        
        /// <summary>
        /// Construct response for unknown content
        /// </summary>
        /// <param name="content">Content</param>
        /// <param name="response">Response message</param>
        internal Unknown(JsonElement content, HttpResponseMessage response)
        {
            Content = Corvus.Json.JsonAny.FromJson(content);
            StatusCode = response.StatusCode;
        }
        
        internal static MediaTypeHeaderValue MediaType { get; } = MediaTypeHeaderValue.Parse("*/*");
        
        /// <inheritdoc/>
        internal override ValidationContext Validate(ValidationLevel validationLevel)
        {
            return base.Validate(validationLevel);
        }
    }

    internal static bool MatchesStatusCode(HttpStatusCode statusCode) =>
        {{{(_hasDefaultStatusCode ? "true" : _hasExplicitStatusCode ? $"((int)statusCode) == {_responseStatusCodePattern}" : $"Matches{_responseStatusCodePattern.First()}xxStatusCode((int)statusCode)")}}};
    
    /// <summary>
    /// Response status code
    /// </summary> 
    internal HttpStatusCode StatusCode { get; private set; }

    /// <inheritdoc/>
    internal static async Task<{{{_responseClassName}}}> BindAsync(HttpResponseMessage response, CancellationToken cancellation = default)
    {
        var stream = await response.Content.ReadAsStreamAsync(cancellation)
            .ConfigureAwait(false);
        var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellation)
            .ConfigureAwait(false);
        var content = document.RootElement.Clone();
        
        var contentType = response.Content.Headers.ContentType;
        return contentType switch
        {
            null => new Unknown(content, response),{{{_contentGenerators.AggregateToString(generator => 
$"""
            _ when contentType.IsSubset({generator.ClassName}.MediaType) => new {generator.ClassName}(content, response),
""")
}}}
            _ => new Unknown(content, response)
        };
    }
    
    /// <summary>
    /// Create a validation context
    /// </summary>
    /// <returns>Validation context</returns>
    protected ValidationContext CreateValidationContext() => 
        ValidationContext.ValidContext.UsingStack().UsingResults();
    
    /// <inheritdoc/>
    internal override ValidationContext Validate(ValidationLevel validationLevel)
    {
        var validationContext = CreateValidationContext();
        
        return validationContext;
    }
}
""";
    }
}

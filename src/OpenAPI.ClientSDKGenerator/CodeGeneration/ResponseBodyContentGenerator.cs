using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Corvus.Json.CodeGeneration;
using Corvus.Json.CodeGeneration.CSharp;
using Microsoft.OpenApi;
using OpenAPI.ClientSDKGenerator.Extensions;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class ResponseBodyContentGenerator
{
    private readonly string _contentVariableName;
    internal string ClassName { get; }
    private readonly MediaTypeHeaderValue _contentType;
    private readonly TypeDeclaration _typeDeclaration;
    private readonly bool _isContentTypeRange;
    private readonly bool _isSequentialMediaType;
    
    public ResponseBodyContentGenerator(KeyValuePair<string, IOpenApiMediaType> contentMediaType, TypeDeclaration typeDeclaration)
    { 
        _contentType = MediaTypeHeaderValue.Parse(contentMediaType.Key);
        _typeDeclaration = typeDeclaration;
        _isSequentialMediaType = contentMediaType.Value.ItemSchema != null;
        _isContentTypeRange = _contentType.MediaType.EndsWith("*");
        _contentVariableName = _contentType.MediaType switch
        {
            "*/*" => "any",
            not null when _isContentTypeRange =>
                $"any{_contentType.MediaType.TrimEnd('*').TrimEnd('/').ToLower().ToPascalCase()}",
            null => throw new InvalidOperationException("Content type is null"),
            _ => _contentType.MediaType.ToLower().ToCamelCase()
        };

        ClassName = _contentVariableName.ToPascalCase();
    }

    private string SchemaLocation => _typeDeclaration.RelativeSchemaLocation;
    public string GenerateResponseClass(string responseClassName) =>
        _isSequentialMediaType ? 
$$"""
/// <summary>
/// Response for content {{_contentType}}
/// </summary>
internal sealed class {{ClassName}} : {{responseClassName}}
{
    internal {{ClassName}}Enumerable<{{_typeDeclaration.FullyQualifiedDotnetTypeName()}}> Content { get; }

    private {{ClassName}}(Stream stream, HttpResponseMessage response)
    {
        Content = new(stream);
        StatusCode = response.StatusCode;
    }
    
    /// <summary>
    /// Construct response for content {{_contentType}}
    /// </summary>
    /// <param name="response">Response message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    internal static async Task<{{responseClassName}}> BindAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);
        return new {{ClassName}}(stream, response);
    }
    
    internal static MediaTypeHeaderValue MediaType { get; } = MediaTypeHeaderValue.Parse("{{_contentType}}");
    
    private const string ContentSchemaLocation = "{{SchemaLocation}}";
    /// <inheritdoc/>
    internal override ValidationContext Validate(ValidationLevel validationLevel)
    {
        var validationContext = base.Validate(validationLevel);
        return Content.Validate(ContentSchemaLocation, true, validationContext, validationLevel);
    }
}                              
""" :


$$"""
/// <summary>
/// Response for content {{_contentType}}
/// </summary>
internal sealed class {{ClassName}} : {{responseClassName}}
{
    internal {{_typeDeclaration.FullyQualifiedDotnetTypeName()}} Content { get; }
    
    private {{ClassName}}(JsonElement content, HttpResponseMessage response)
    {
        Content = {{_typeDeclaration.FullyQualifiedDotnetTypeName()}}.FromJson(content);
        StatusCode = response.StatusCode;
    }
    
    /// <summary>
    /// Construct response for content {{_contentType}}
    /// </summary>
    /// <param name="response">Response message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    internal static async Task<{{responseClassName}}> BindAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var content = await Response.ReadJsonAsync(response, cancellationToken)
            .ConfigureAwait(false);
        return new {{ClassName}}(content, response);
    }
    
    internal static MediaTypeHeaderValue MediaType { get; } = MediaTypeHeaderValue.Parse("{{_contentType}}");
    
    private const string ContentSchemaLocation = "{{SchemaLocation}}";
    /// <inheritdoc/>
    internal override ValidationContext Validate(ValidationLevel validationLevel)
    {
        var validationContext = base.Validate(validationLevel);
        return Content.Validate(ContentSchemaLocation, true, validationContext, validationLevel);
    }
}
""";
}

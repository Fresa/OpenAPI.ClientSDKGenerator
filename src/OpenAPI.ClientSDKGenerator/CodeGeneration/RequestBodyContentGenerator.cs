using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Corvus.Json.CodeGeneration;
using Corvus.Json.CodeGeneration.CSharp;
using Microsoft.OpenApi;
using OpenAPI.ClientSDKGenerator.Extensions;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class RequestBodyContentGenerator
{
    private readonly string _contentVariableName;
    internal string ClassName { get; }
    private MediaTypeHeaderValue ContentType { get; }
    private readonly TypeDeclaration _typeDeclaration;
    private readonly bool _isContentTypeRange;
    private readonly bool _isSequentialMediaType;
    
    public RequestBodyContentGenerator(KeyValuePair<string, IOpenApiMediaType> contentMediaType, TypeDeclaration typeDeclaration)
    { 
        ContentType = MediaTypeHeaderValue.Parse(contentMediaType.Key);
        _typeDeclaration = typeDeclaration;
        _isSequentialMediaType = contentMediaType.Value.ItemSchema != null;
        _isContentTypeRange = ContentType.MediaType.EndsWith("*");
        _contentVariableName = ContentType.MediaType switch
        {
            "*/*" => "any",
            not null when _isContentTypeRange =>
                $"any{ContentType.MediaType.TrimEnd('*').TrimEnd('/').ToLower().ToPascalCase()}",
            null => throw new InvalidOperationException("Content type is null"),
            _ => ContentType.MediaType.ToLower().ToCamelCase()
        };

        ClassName = _contentVariableName.ToPascalCase();
    }

    private string SchemaLocation => _typeDeclaration.RelativeSchemaLocation;
    public string GenerateContentClass() =>
        _isSequentialMediaType ? 
$$"""
/// <summary>
/// Request body for content {{ContentType}}
/// </summary>
internal sealed class {{ClassName}} : Content, IDisposable
{
    private readonly {{ClassName}}Writer<{{_typeDeclaration.FullyQualifiedDotnetTypeName()}}> _content;
    private {{_typeDeclaration.FullyQualifiedDotnetTypeName()}}? _currentItem;
    private readonly Stream _stream;
    
    /// <summary>
    /// Construct request for content {{ContentType}}
    /// </summary>{{(_isContentTypeRange ? 
$"""

   /// <param name="contentType">Content type must match range {ContentType.MediaType}</param>
""" : "")}}
    public {{ClassName}}(Stream stream{{(_isContentTypeRange ? ", string contentType" : "")}})
    {{{(_isContentTypeRange ? 
"""
        
        EnsureExpectedContentType(MediaTypeHeaderValue.Parse(contentType), ContentMediaType);
""" : "")}}
        _content = new(stream);
        _stream = stream;
        MediaType = {{(_isContentTypeRange ? "contentType" : $"\"{ContentType.MediaType}\"")}};
    }

    internal override string MediaType { get; }
    
    /// <summary>
    /// Write an item to the sequence
    /// </summary>
    /// <param name="item">Item to write</param>
    internal void WriteItem({{_typeDeclaration.FullyQualifiedDotnetTypeName()}} item)
    {
        _currentItem = item;
        // todo: Validate
        //ValidateCurrentItem();
        
        _content.WriteItem(item);
        _currentItem = null;
    }
    
    private static MediaTypeHeaderValue ContentMediaType { get; } = MediaTypeHeaderValue.Parse("{{ContentType}}");
    
    internal override HttpContent Get() =>
        new StreamContent(_stream)
        {
            Headers =
            {
                ContentType = ContentMediaType
            }
        };

    private const string ContentSchemaLocation = "{{SchemaLocation}}";
    /// <inheritdoc/>
    internal override ValidationContext Validate(ValidationContext context, ValidationLevel validationLevel)
    {
        return ValidateCurrentItem(context, validationLevel);
    }
    
    private ValidationContext ValidateCurrentItem(
        ValidationContext validationContext, 
        ValidationLevel validationLevel) =>
        _currentItem is null
            ? validationContext
            : _content.Validate(_currentItem.Value, ContentSchemaLocation, validationContext,
                validationLevel);
                
    /// <inheritdoc/>
    public void Dispose()
    {
        _content.Dispose();
    }
}                              
""" :


$$"""
/// <summary>
/// Response for content {{ContentType}}
/// </summary>
internal sealed class {{ClassName}} : Content
{
    private {{_typeDeclaration.FullyQualifiedDotnetTypeName()}} _content;
    
    /// <summary>
    /// Construct response for content {{ContentType}}
    /// </summary>
    /// <param name="{{_contentVariableName}}">Content</param>{{(_isContentTypeRange ? 
$"""

    /// <param name="contentType">Content type must match range {ContentType.MediaType}</param>
""" : "")}}
    public {{ClassName}}({{_typeDeclaration.FullyQualifiedDotnetTypeName()}} {{_contentVariableName}}{{(_isContentTypeRange ? ", string contentType" : "")}})
    {{{(_isContentTypeRange ? 
"""
        
        EnsureExpectedContentType(MediaTypeHeaderValue.Parse(contentType), ContentMediaType);
""" : "")}}
        _content = {{_contentVariableName}};
        MediaType = {{(_isContentTypeRange ? "contentType" : $"\"{ContentType}\"")}};
    }
    
    internal override string MediaType { get; }
    
    internal override HttpContent Get() =>
       new StringContent(
           _content.Serialize(),
           encoding: Encoding.UTF8,
           mediaType: MediaType
       );{{(_isContentTypeRange ?
"""    

    private static MediaTypeHeaderValue ContentMediaType { get; } = MediaTypeHeaderValue.Parse("{{ContentType}}");
""" : "")}}    
    private const string ContentSchemaLocation = "{{SchemaLocation}}";
    /// <inheritdoc/>
    internal override ValidationContext Validate(ValidationContext validationContext, ValidationLevel validationLevel) =>
        _content.Validate(ContentSchemaLocation, true, validationContext, validationLevel);
}
""";
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi;
using OpenAPI.ClientSDKGenerator.Extensions;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class RequestBodyGenerator
{
    private readonly IOpenApiRequestBody? _body;
    private readonly List<RequestBodyContentGenerator> _contentGenerators = [];

    public static readonly RequestBodyGenerator Empty = new(null, []);

    internal bool HasBody => _body != null;
    
    public RequestBodyGenerator(
        IOpenApiRequestBody? body,
        List<RequestBodyContentGenerator> contentGenerators)
    {
        _body = body;
        _contentGenerators = contentGenerators;
    }
    
    public string GenerateClass()
    {
        if (!_contentGenerators.Any())
        {
            return string.Empty;
        }
        
        return 
$$$"""
internal abstract class Content
{
    internal abstract string? MediaType { get; }

    /// <summary>
    /// Ensures that the specified content type matches the specification
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified content type does not match the specification</exception>
    /// </summary>
    /// <param name="contentType">Content type</param>
    /// <param name="expectedContentType">Expected content type</param>
    protected void EnsureExpectedContentType(MediaTypeHeaderValue contentType, MediaTypeHeaderValue expectedContentType)
    {
        if (!contentType.IsSubsetOf(expectedContentType))
        {
            throw new ArgumentOutOfRangeException($"Expected content type {contentType.MediaType} to be a subset of {expectedContentType.MediaType}");
        }
    }

    internal abstract HttpContent Get();
    
    internal abstract ValidationContext Validate(ValidationContext validationContext, ValidationLevel validationLevel);
        
{{{
    _contentGenerators.AggregateToString(generator =>
        generator.GenerateContentClass())}}}
}
""";
    }
}
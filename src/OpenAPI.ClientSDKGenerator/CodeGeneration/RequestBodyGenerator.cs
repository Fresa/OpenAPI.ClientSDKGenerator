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
        if (!IsSubset(contentType, expectedContentType))
        {
            throw new ArgumentOutOfRangeException($"Expected content type {contentType.MediaType} to be a subset of {expectedContentType.MediaType}");
        }
    }

    private const string Wildcard = "*";
    private const string Quality = "q";
    private const char ForwardSlash = '/';
    private const char Plus = '+';

    private static bool IsSubset(MediaTypeHeaderValue self, MediaTypeHeaderValue? otherMediaType)
    {
        if (otherMediaType is null)
            return false;
        return MatchesType(self, otherMediaType)
            && MatchesSubtype(self, otherMediaType)
            && MatchesParameters(self, otherMediaType);
    }

    private static ReadOnlySpan<char> Type(MediaTypeHeaderValue mt)
    {
        var s = (mt.MediaType ?? string.Empty).AsSpan();
        var i = s.IndexOf(ForwardSlash);
        return i < 0 ? s : s[..i];
    }

    private static ReadOnlySpan<char> SubType(MediaTypeHeaderValue mt)
    {
        var s = (mt.MediaType ?? string.Empty).AsSpan();
        var i = s.IndexOf(ForwardSlash);
        return i < 0 ? default : s[(i + 1)..];
    }

    private static ReadOnlySpan<char> Suffix(MediaTypeHeaderValue mt)
    {
        var sub = SubType(mt);
        var i = sub.LastIndexOf(Plus);
        return i < 0 ? default : sub[(i + 1)..];
    }

    private static ReadOnlySpan<char> SubTypeWithoutSuffix(MediaTypeHeaderValue mt)
    {
        var sub = SubType(mt);
        var i = sub.LastIndexOf(Plus);
        return i < 0 ? sub : sub[..i];
    }

    private static bool MatchesAllTypes(MediaTypeHeaderValue mt) =>
        string.Equals(mt.MediaType, "*/*", StringComparison.Ordinal);

    private static bool MatchesAllSubTypes(MediaTypeHeaderValue mt) =>
        SubType(mt).Equals(Wildcard.AsSpan(), StringComparison.Ordinal);

    private static bool MatchesAllSubTypesWithoutSuffix(MediaTypeHeaderValue mt) =>
        SubTypeWithoutSuffix(mt).Equals(Wildcard.AsSpan(), StringComparison.OrdinalIgnoreCase);

    private static bool MatchesType(MediaTypeHeaderValue self, MediaTypeHeaderValue set) =>
        MatchesAllTypes(set) ||
        Type(set).Equals(Type(self), StringComparison.OrdinalIgnoreCase);

    private static bool MatchesSubtype(MediaTypeHeaderValue self, MediaTypeHeaderValue set)
    {
        if (MatchesAllSubTypes(set))
            return true;

        if (!Suffix(set).IsEmpty)
        {
            return !Suffix(self).IsEmpty
                && MatchesSubtypeWithoutSuffix(self, set)
                && MatchesSubtypeSuffix(self, set);
        }

        return MatchesEitherSubtypeOrSuffix(self, set);
    }

    private static bool MatchesSubtypeWithoutSuffix(MediaTypeHeaderValue self, MediaTypeHeaderValue set) =>
        MatchesAllSubTypesWithoutSuffix(set) ||
        SubTypeWithoutSuffix(set).Equals(SubTypeWithoutSuffix(self), StringComparison.OrdinalIgnoreCase);

    private static bool MatchesEitherSubtypeOrSuffix(MediaTypeHeaderValue self, MediaTypeHeaderValue set) =>
        SubType(set).Equals(SubType(self), StringComparison.OrdinalIgnoreCase) ||
        SubType(set).Equals(Suffix(self), StringComparison.OrdinalIgnoreCase);

    private static bool MatchesSubtypeSuffix(MediaTypeHeaderValue self, MediaTypeHeaderValue set) =>
        Suffix(set).Equals(Suffix(self), StringComparison.OrdinalIgnoreCase);

    private static bool MatchesParameters(MediaTypeHeaderValue self, MediaTypeHeaderValue set)
    {
        if (set.Parameters.Count == 0)
            return true;

        foreach (var parameter in set.Parameters)
        {
            if (string.Equals(parameter.Name, Wildcard, StringComparison.OrdinalIgnoreCase))
                continue;

            if (string.Equals(parameter.Name, Quality, StringComparison.OrdinalIgnoreCase))
                break;

            var local = self.Parameters.FirstOrDefault(p =>
                string.Equals(p.Name, parameter.Name, StringComparison.OrdinalIgnoreCase));
            if (local is null)
                return false;

            if (!string.Equals(parameter.Value, local.Value, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
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
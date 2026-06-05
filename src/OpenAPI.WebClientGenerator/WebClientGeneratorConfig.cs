using System;
using System.IO;
using Corvus.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using OpenAPI.WebClientGenerator.Extensions;
using OpenAPI.WebClientGenerator.OpenApi;

namespace OpenAPI.WebClientGenerator;

internal sealed class WebClientGeneratorConfig(
    string? clientName,
    AdditionalText openApiSpecification,
    string? ns,
    ValidationLevel? validationLevel)
{
    public string ClientName { get; } = clientName ?? "WebClient";
    public AdditionalText OpenApiSpecification { get; } = openApiSpecification;
    public string? Namespace { get; } = ns;
    public ValidationLevel ValidationLevel { get; } = validationLevel ?? ValidationLevel.Detailed;
    
    internal OpenApiSpecification LoadOpenApiSpecification()
    {
        var format = Path.GetExtension(OpenApiSpecification.Path)
            .TrimStart('.')
            .ToLowerInvariant();
        var stream = OpenApiSpecification.AsStream();

        var (document, version) = stream.LoadOpenApiDocument(format);
        var openApiUri = new JsonReference(document.BaseUri.ToString());
        var jsonDocument = stream.ParseJsonDocument(format);

        return new OpenApiSpecification(document, version, openApiUri, jsonDocument);
    }

    internal static WebClientGeneratorConfig? Parse(AnalyzerConfigOptions options, AdditionalText openApiSpecification)
    {
        if (!options.TryGetValue("build_metadata.AdditionalFiles.SourceItemGroup", out var group) ||
            !string.Equals(group, "WebClientGenerator", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        options.TryGetValue("build_metadata.AdditionalFiles.ClientName", out var clientName);
        options.TryGetValue("build_metadata.AdditionalFiles.Namespace", out var @namespace);

        ValidationLevel? validationLevel = null;
        if (options.TryGetValue("build_metadata.AdditionalFiles.ValidationLevel", out var validationLevelValue) && 
            !string.IsNullOrEmpty(validationLevelValue))
        {
            if (!Enum.TryParse<ValidationLevel>(validationLevelValue, ignoreCase: true, out var parsedValidationLevel))
            {
                throw new ArgumentOutOfRangeException(
                    $"ValidationLevel",
                    $"Could not parse validation level: {validationLevelValue}");
            }
            validationLevel = parsedValidationLevel;
        }

        return new WebClientGeneratorConfig(clientName,
            openApiSpecification: openApiSpecification,
            @namespace,
            validationLevel);
    }
}
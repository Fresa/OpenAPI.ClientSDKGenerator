namespace OpenAPI.WebClientGenerator.CodeGeneration;

internal sealed class WebClientConfigurationGenerator(string @namespace, WebClientGeneratorConfig generatorConfig)
{
    private const string ClassName = "WebClientConfiguration";
    
    internal SourceCode GenerateClass() =>
        new($"{ClassName}.g.cs",
            $$"""
              #nullable enable
              using Corvus.Json;
              using Microsoft.AspNetCore.Authorization;
              using System;

              namespace {{@namespace}};
                    
              internal sealed class {{ClassName}} 
              {
                  /// <summary>
                  /// The uri to the exposed OpenAPI specification used to generate the SDK.
                  /// This is used in the SchemaLocation of the ValidationResult.
                  /// <example>https://localhost/openapi.json</example> 
                  /// </summary>
                  internal Uri? OpenApiSpecificationUri { get; init; }
                  
                  /// <summary>
                  /// Set validation level
                  /// </summary>
                  internal ValidationLevel ValidationLevel { get; init; } = ValidationLevel.{{generatorConfig.ValidationLevel.ToString()}};
  
                  /// <summary>
                  /// Should responses be validated?
                  /// </summary>
                  internal bool ValidateResponses { get; init; } = true;
                  
                  /// <summary>
                  /// Should requests be validated?
                  /// </summary>
                  internal bool ValidateRequests { get; init; } = true;
              }
              #nullable restore
              """);
}
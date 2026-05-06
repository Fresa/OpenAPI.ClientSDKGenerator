namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

public class SdkConfigurationGenerator(string @namespace)
{
    private const string ClassName = "ClientSdkConfiguration";
    
    internal SourceCode GenerateClass() =>
        new($"{ClassName}.g.cs",
            $$"""
              #nullable enable
              using Microsoft.AspNetCore.Authorization;
              using System;

              namespace {{@namespace}};
                    
              public sealed class {{ClassName}} 
              {
                  /// <summary>
                  /// The uri to the exposed OpenAPI specification used to generate the SDK.
                  /// This is used in the SchemaLocation of the ValidationResult.
                  /// <example>https://localhost/openapi.json</example> 
                  /// </summary>
                  public Uri? OpenApiSpecificationUri { get; init; }
              }
              #nullable restore
              """);
}
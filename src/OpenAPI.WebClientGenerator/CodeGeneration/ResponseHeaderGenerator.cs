using Corvus.Json.CodeGeneration;
using Corvus.Json.CodeGeneration.CSharp;
using Microsoft.OpenApi;
using OpenAPI.WebClientGenerator.Extensions;
using OpenAPI.WebClientGenerator.OpenApi;

namespace OpenAPI.WebClientGenerator.CodeGeneration;

internal sealed class ResponseHeaderGenerator(
    string name, 
    IOpenApiHeader header, 
    TypeDeclaration typeDeclaration, 
    OpenApiSpecVersion openApiSpecVersion)
{
    private readonly string _propertyName = name.ToPascalCase();
    private readonly string _requiredDirective = header.Required ? "required " : string.Empty;
    private readonly string _fullyQualifiedTypeName = $"{typeDeclaration.FullyQualifiedDotnetTypeName()}";

    internal string GenerateProperty() =>
        $$"""
          {{header.Description.AsComment("summary", "para")}}
          internal {{_requiredDirective}}{{_fullyQualifiedTypeName}} {{_propertyName}} { get; init; }
          """.TrimStart();
    
    internal string GenerateBindDirective(string responseVariableName)
    {
        // Response header specification is a subset of the parameter specification, so we add the missing properties to be able to use the parameter value parser 
        var headerSpecificationAsJson = 
            $$"""
              {
                "name": "{{name}}",
                "in": "header",
                {{header.Serialize(openApiSpecVersion).ToString().TrimStart('{').TrimStart()}} 
              """;

        return
            $""""
             {_propertyName} = {responseVariableName}.Bind<{_fullyQualifiedTypeName}>(
                 """
                 {headerSpecificationAsJson.Indent(4).TrimStart()}
                 """),
             """";
    }

    internal string GenerateValidateDirective() =>
        $"""
         {_propertyName}.Validate("{typeDeclaration.RelativeSchemaLocation}", {header.Required.ToString().ToLowerInvariant()}, validationContext, validationLevel);
         """;
}

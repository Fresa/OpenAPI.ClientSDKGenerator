using Corvus.Json.CodeGeneration;
using Corvus.Json.CodeGeneration.CSharp;
using Microsoft.OpenApi;
using OpenAPI.ClientSDKGenerator.Extensions;
using OpenAPI.ClientSDKGenerator.OpenApi;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class ParameterGenerator(
    TypeDeclaration typeDeclaration, 
    IOpenApiParameter parameter)
{
    internal string FullyQualifiedTypeName =>
        $"{FullyQualifiedTypeDeclarationIdentifier}{(parameter.Required ? "" : "?")}";

    private string FullyQualifiedTypeDeclarationIdentifier => typeDeclaration.FullyQualifiedDotnetTypeName();
    
    internal string ParameterName { get; } = parameter.GetName();
    internal bool IsParameterRequired { get; } = parameter.Required;
    internal string Location { get; } = parameter.GetLocation();
    internal string SchemaLocation { get; } = typeDeclaration.RelativeSchemaLocation;
}
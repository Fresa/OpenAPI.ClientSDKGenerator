using System;
using Corvus.Json.CodeGeneration;
using Corvus.Json.CodeGeneration.CSharp;
using Microsoft.OpenApi;
using OpenAPI.ClientSDKGenerator.OpenApi;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class ParameterGenerator(
    OpenApiSpecVersion openApiSpecVersion,
    TypeDeclaration typeDeclaration,
    IOpenApiParameter parameter)
{
    internal string FullyQualifiedTypeName =>
        $"{FullyQualifiedTypeDeclarationIdentifier}{(parameter.Required ? "" : "?")}";

    private string FullyQualifiedTypeDeclarationIdentifier => typeDeclaration.FullyQualifiedDotnetTypeName();

    internal string ParameterName { get; } = parameter.GetName();
    internal bool IsParameterRequired { get; } = parameter.Required;
    internal ParameterLocation Location { get; } = parameter.In ?? throw new NullReferenceException("In is null");
    internal string SchemaLocation { get; } = typeDeclaration.RelativeSchemaLocation;

    internal string ParameterSpecificationAsJson { get; } = parameter.Serialize(openApiSpecVersion).ToString();
}
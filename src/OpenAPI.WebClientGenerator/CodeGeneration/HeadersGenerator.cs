using Microsoft.OpenApi;

namespace OpenAPI.WebClientGenerator.CodeGeneration;

internal sealed class HeadersGenerator(ParameterGenerator[] parameters) : 
    ParametersGenerator(parameters)
{
    protected override ParameterLocation Location => ParameterLocation.Header;
}
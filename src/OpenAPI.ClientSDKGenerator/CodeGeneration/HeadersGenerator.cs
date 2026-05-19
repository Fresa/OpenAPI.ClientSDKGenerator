using Microsoft.OpenApi;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class HeadersGenerator(ParameterGenerator[] parameters) : 
    ParametersGenerator(parameters)
{
    protected override ParameterLocation Location => ParameterLocation.Header;
}
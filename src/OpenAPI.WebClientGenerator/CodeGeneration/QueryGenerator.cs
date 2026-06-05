using Microsoft.OpenApi;

namespace OpenAPI.WebClientGenerator.CodeGeneration;

internal sealed class QueryGenerator(ParameterGenerator[] parameters) :
    ParametersGenerator(parameters)
{
    protected override ParameterLocation Location => ParameterLocation.Query;
}
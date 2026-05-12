using System.Linq;
using Microsoft.OpenApi;

namespace OpenAPI.ClientSDKGenerator.CodeGeneration;

internal sealed class QueryGenerator(ParameterGenerator[] parameters) : 
    ParametersGenerator(
        parameters
            .Where(generator => 
                generator.Location == ParameterLocation.Query)
            .ToArray());
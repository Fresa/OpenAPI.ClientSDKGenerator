using System.Collections.Immutable;
using System.Threading;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OpenAPI.ClientSDKGenerator.Tests.Utils;

namespace OpenAPI.ClientSDKGenerator.Tests;

internal static class Generator
{
    internal static Compilation Setup(string openApiSpec, CancellationToken cancellationToken, out ImmutableArray<Diagnostic> diagnostics)
    {
        var generator = new ApiGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.AddAdditionalTexts(
            [
                new TestAdditionalFile($"OpenApiSpecs/{openApiSpec}")
            ]
        );

        const string assemblyName = nameof(ApiGeneratorTests);
        var compilation = CSharpCompilation.Create(assemblyName,
            options: new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary));

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out diagnostics, cancellationToken);
        
        foreach (var tree in newCompilation.SyntaxTrees)                                                  
        {                                
            tree.GetDiagnostics().Should().NotContain(diagnostic =>
                diagnostic.Severity == DiagnosticSeverity.Error ||
                diagnostic.Severity == DiagnosticSeverity.Warning);       
        }     
        
        return newCompilation;
    }
}
using System.IO;
using System.Linq;
using System.Threading;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using OpenAPI.ClientSDKGenerator.Tests.Utils;
using Xunit;

namespace OpenAPI.ClientSDKGenerator.Tests;

public class EntityTests(ITestOutputHelper testOutputHelper)
{
    private CancellationToken Cancellation => TestContext.Current.CancellationToken;

    [Fact]
    public void GivenAClientNameThatOverlapsWithARootEntity_WhenGeneratingAPI_TheOverlappingEntityShouldBeRenamed()
    {
        var compilation = ClientSdkGenerator.Setup("openapi-v3.json",
            clientName: "Pets",
            @namespace: "Example",
            cancellationToken: Cancellation,
            diagnostics: out var diagnostics);

        diagnostics.Should().BeEmpty();

        var generatedFiles = compilation.SyntaxTrees
            .Select(t => Path.GetFileName(t.FilePath))
            .ToArray();

        generatedFiles.Should().Contain("Pets.g.cs");
        compilation.Output("Pets.g.cs", testOutputHelper, Cancellation);
        generatedFiles.Should().Contain("Pets.Pets.g.cs");
        compilation.Output("Pets.Pets.g.cs", testOutputHelper, Cancellation);

        var typeNames = compilation
            .GetSymbolsWithName(_ => true, SymbolFilter.Type, Cancellation)
            .Select(symbol => symbol.Name)
            .ToArray();

        typeNames.Should().Contain("Pets");
        typeNames.Should().Contain("Pets0");
        typeNames.Should().Contain("Pets1");

        var petsType = compilation
            .GetSymbolsWithName("Pets", SymbolFilter.Type, Cancellation)
            .OfType<INamedTypeSymbol>()
            .Single(symbol => symbol.ContainingNamespace.ToDisplayString() == "Example");

        var methodNames = petsType
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Select(method => method.Name)
            .ToArray();

        methodNames.Should().Contain("Pets_", Exactly.Twice());
    }
}
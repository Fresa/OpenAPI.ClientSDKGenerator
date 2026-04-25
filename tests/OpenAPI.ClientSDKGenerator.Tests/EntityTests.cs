using System.IO;
using System.Linq;
using System.Threading;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace OpenAPI.ClientSDKGenerator.Tests;

public class EntityTests
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
        generatedFiles.Should().Contain("Pets.Pets_.g.cs");

        var typeNames = compilation
            .GetSymbolsWithName(_ => true, SymbolFilter.Type, Cancellation)
            .Select(symbol => symbol.Name)
            .ToArray();

        typeNames.Should().Contain("Pets");
        typeNames.Should().Contain("Pets_");
    }
}
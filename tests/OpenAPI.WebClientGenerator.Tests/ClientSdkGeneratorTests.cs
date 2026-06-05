using System.IO;
using System.Linq;
using System.Threading;
using AwesomeAssertions;
using OpenAPI.WebClientGenerator.Tests.Utils;
using Xunit;

namespace OpenAPI.WebClientGenerator.Tests;

public class WebClientGeneratorTests(ITestOutputHelper testOutputHelper)
{
    private CancellationToken Cancellation => TestContext.Current.CancellationToken;
    
    [Theory]
    [InlineData("openapi-v2.json")]
    [InlineData("openapi-v3.json")]
    [InlineData("openapi-v3.1.json")]
    [InlineData("openapi-v3.2.json")]
    [InlineData("openapi-v3.2.yaml")]
    public void GivenAnOpenAPISpec_WhenGeneratingAPI_ExpectedClassesShouldHaveBeenGenerated(string openApiSpec)
    {
        var compilation = WebClientGenerator.Setup(openApiSpec,
            clientName: "TestClient",
            @namespace: "Example",
            cancellationToken: Cancellation,
            diagnostics: out var diagnostics);

        diagnostics.Should().BeEmpty();

        var generatedFiles = compilation.SyntaxTrees
            .Select(t => Path.GetFileName(t.FilePath))
            .ToArray();

        compilation.Output("TestClient.g.cs", testOutputHelper, Cancellation);
        generatedFiles.Should().HaveCountGreaterThan(0);
        generatedFiles.Should().Contain("TestClient.g.cs");
    }
}

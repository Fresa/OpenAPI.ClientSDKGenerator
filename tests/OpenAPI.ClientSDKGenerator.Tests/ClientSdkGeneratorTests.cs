using System.IO;
using System.Linq;
using System.Threading;
using AwesomeAssertions;
using Xunit;

namespace OpenAPI.ClientSDKGenerator.Tests;

public class ClientSdkGeneratorTests
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
        var compilation = ClientSdkGenerator.Setup(openApiSpec,
            clientName: "TestClient",
            @namespace: "Example",
            cancellationToken: Cancellation,
            diagnostics: out var diagnostics);

        diagnostics.Should().BeEmpty();

        var generatedFiles = compilation.SyntaxTrees
            .Select(t => Path.GetFileName(t.FilePath))
            .ToArray();

        generatedFiles.Should().HaveCountGreaterThan(0);
        generatedFiles.Should().Contain("TestClient.g.cs");
    }
}

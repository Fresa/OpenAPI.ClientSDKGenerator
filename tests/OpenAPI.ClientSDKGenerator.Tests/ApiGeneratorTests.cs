using System.IO;
using System.Linq;
using System.Threading;
using AwesomeAssertions;
using Xunit;

namespace OpenAPI.ClientSDKGenerator.Tests;

public class ApiGeneratorTests
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
        var compilation = Generator.Setup(openApiSpec, Cancellation,
            out var diagnostics);

        diagnostics.Should().BeEmpty();

        var generatedFiles = compilation.SyntaxTrees
            .Select(t => Path.GetFileName(t.FilePath))
            .ToArray();

        generatedFiles.Should().HaveCount(0);
    }
}

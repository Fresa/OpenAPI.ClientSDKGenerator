using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Corvus.Json;
using Example.OpenApi32.IntegrationTests.Json;

namespace Example.OpenApi32.IntegrationTests;

public class ExportFooEventsTests(FooApplicationFactory app, ITestOutputHelper testOutput) : FooTestSpecification, IClassFixture<FooApplicationFactory>
{
    [Fact]
    internal async Task ExportingFooEventsAsJsonl_ShouldReturnOk()
    {
        var response = await GetResponse<Foo.Foo.Foo1.Events0.GetResponse.OK200.ApplicationJsonl>();
        await AssertContent(response.Content);
    }

    [Fact]
    internal async Task ExportingFooEventsAsXJsonlines_ShouldReturnOk()
    {
        var response = await GetResponse<Foo.Foo.Foo1.Events0.GetResponse.OK200.ApplicationXJsonlines>();
        await AssertContent(response.Content);
    }

    [Fact]
    internal async Task ExportingFooEventsAsXNdjson_ShouldReturnOk()
    {
        var response = await GetResponse<Foo.Foo.Foo1.Events0.GetResponse.OK200.ApplicationXNdjson>();
        await AssertContent(response.Content);
    }

    [Fact]
    internal async Task ExportingFooEventsAsJsonSeq_ShouldReturnOk()
    {
        var response = await GetResponse<Foo.Foo.Foo1.Events0.GetResponse.OK200.ApplicationJsonSeq>();
        await AssertContent(response.Content);
    }

    [Fact]
    internal async Task ExportingFooEventsAsGeoJsonSeq_ShouldReturnOk()
    {
        var response = await GetResponse<Foo.Foo.Foo1.Events0.GetResponse.OK200.ApplicationGeoJsonSeq>();
        await AssertContent(response.Content);
    }
    
    private async Task<T> GetResponse<T>()
        where T : Foo.Foo.Foo1.Events0.GetResponse.OK200, Foo.Foo.Foo1.Events0.GetResponse.IAcceptContent
    {
        using var httpClient = app.CreateClient();
        var client = new Foo.Foo(httpClient);
        var response = await client.Foo_(1).Events().GetAsync(
            accepts: Foo.Foo.Foo1.Events0.Accept.Content<T>(), 
            cancellation: CancellationToken);

        var typedResponse = response.Should().BeOfType<T>()
            .Subject;
        typedResponse.Validate(ValidationLevel.Detailed).IsValid.Should().BeTrue();
        typedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return typedResponse;
    }

    private static async Task AssertContent(Foo.SequentialJsonEnumerable<Example.Foo.Components.Schemas.FooProperties> enumerable)
    {
        var expectedNames = new Queue<string>(["foo1", "foo2"]);
        await foreach (var (content, contentValidationContext) in enumerable.ConfigureAwait(false))
        {
            contentValidationContext.IsValid.Should().BeTrue();
            content.Name.Should().BeEquivalentTo(new JsonString(expectedNames.Dequeue()));
        }
    }
}
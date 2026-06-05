using System.Net;
using AwesomeAssertions;
using Example.Foo.Components.Schemas;
using OpenAPI.IntegrationTestHelpers.Auth;

namespace Example.OpenApi32.IntegrationTests;

public class ImportFooEventsTests(FooApplicationFactory app) : FooTestSpecification, IClassFixture<FooApplicationFactory>
{
    [Fact]
    internal async Task ImportingFooEventsAsJsonl_ShouldReturnAccepted()
    {
        using var stream = new MemoryStream();
        using var content = new Foo.Foo.Foo1.Events0.Content.ApplicationJsonl(stream);
        content.WriteItem(FooProperties.Create(name: "test"));
        content.WriteItem(FooProperties.Create(name: "another test"));
        stream.Position = 0;
        await SendAsync(content);
    }

    [Fact]
    internal async Task ImportingFooEventsAsXJsonlines_ShouldReturnAccepted()
    {
        using var stream = new MemoryStream();
        using var content = new Foo.Foo.Foo1.Events0.Content.ApplicationXJsonlines(stream);
        content.WriteItem(FooProperties.Create(name: "test"));
        content.WriteItem(FooProperties.Create(name: "another test"));
        stream.Position = 0;
        await SendAsync(content);
    }

    [Fact]
    internal async Task ImportingFooEventsAsXNdjson_ShouldReturnAccepted()
    {
        using var stream = new MemoryStream();
        using var content = new Foo.Foo.Foo1.Events0.Content.ApplicationXNdjson(stream);
        content.WriteItem(FooProperties.Create(name: "test"));
        content.WriteItem(FooProperties.Create(name: "another test"));
        stream.Position = 0;
        await SendAsync(content);
    }

    [Fact]
    internal async Task ImportingFooEventsAsJsonSeq_ShouldReturnAccepted()
    {
        using var stream = new MemoryStream();
        using var content = new Foo.Foo.Foo1.Events0.Content.ApplicationJsonSeq(stream);
        content.WriteItem(FooProperties.Create(name: "test"));
        content.WriteItem(FooProperties.Create(name: "another test"));
        stream.Position = 0;
        await SendAsync(content);
    }

    [Fact]
    internal async Task ImportingFooEventsAsGeoJsonSeq_ShouldReturnAccepted()
    {
        using var stream = new MemoryStream();
        using var content = new Foo.Foo.Foo1.Events0.Content.ApplicationGeoJsonSeq(stream);
        content.WriteItem(FooProperties.Create(name: "test"));
        content.WriteItem(FooProperties.Create(name: "another test"));
        stream.Position = 0;
        await SendAsync(content);
    }

    private async Task SendAsync(Foo.Foo.Foo1.Events0.Content content)
    {
        using var httpClient = app.CreateClient()
            .WithOAuth2ImplicitFlowAuthentication("update");
        var client = new Foo.Foo(httpClient);
        var response = await client.Foo_(1).Events().PostAsync(content, cancellation: CancellationToken);

        var typedResponse = response.Should().BeOfType<Foo.Foo.Foo1.Events0.PostResponse.Accepted202.Empty>()
            .Subject;
        typedResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        // result.Headers.Should().HaveCount(1);
        // result.Headers.GetValues("ImportedEvents")
        //     .Should().HaveCount(1).And.AllBe("2");
    }
}
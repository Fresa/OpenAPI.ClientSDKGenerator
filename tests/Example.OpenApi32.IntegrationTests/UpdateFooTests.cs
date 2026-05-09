using Example.Foo.Components.Schemas;

namespace Example.OpenApi32.IntegrationTests;

public class UpdateFooTests(FooApplicationFactory app) : FooTestSpecification, IClassFixture<FooApplicationFactory>
{
    [Fact]
    public async Task UpdatingFoo_ReturnsUpdatedFoo()
    {
        using var httpClient = app.CreateClient();

        var client = new Foo.Foo(httpClient);
        await client.Foo_(1)
            .PutAsync(
                new Foo.Foo.Foo1.Content.ApplicationJson(
                    FooProperties.Create(name: "test")));
        
        // result.StatusCode.Should().Be(HttpStatusCode.OK);
        // var responseContent = await result.Content.ReadAsJsonNodeAsync(CancellationToken);
        // responseContent.Should().NotBeNull();
        // responseContent.GetValue<string>("#/Name").Should().Be("test");
        // result.Headers.Should().HaveCount(1);
        // result.Headers.Should().ContainKey("Status")
        //     .WhoseValue.Should().HaveCount(1)
        //     .And.Contain("2");
        // result.Content.Headers.ContentType.Should().Be(MediaTypeHeaderValue.Parse("application/json"));
    }

    // [Fact]
    // public async Task Given_invalid_request_When_Updating_Foo_It_Should_Return_400()
    // {
    //     using var httpClient = app.CreateClient()
    //         .WithOAuth2ImplicitFlowAuthentication("update");
    //     var client = CreateClient(httpClient);
    //
    //     var responseHandler = new NativeResponseHandler();
    //     await client.Foo["test"].PutAsync(
    //         new FooProperties { Name = "test" },
    //         config =>
    //         {
    //             config.Headers.Add("Bar", "test");
    //             config.Options.Add(new ResponseHandlerOption { ResponseHandler = responseHandler });
    //         },
    //         CancellationToken);
    //     var result = (HttpResponseMessage)responseHandler.Value;
    //
    //     result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    //     var responseContent = await result.Content.ReadAsJsonNodeAsync(CancellationToken);
    //     responseContent.Should().NotBeNull();
    //     responseContent.AsArray().Should().HaveCount(1);
    //     responseContent.GetValue<string>("#/0/error").Should().NotBeNullOrEmpty();
    //     responseContent.GetValue<string>("#/0/name").Should().Be("https://localhost/api.json#/components/parameters/FooId/schema/type");
    // }
    //
    // [Fact]
    // public async Task Given_unauthenticated_request_When_Updating_Foo_It_Should_Return_401()
    // {
    //     using var httpClient = app.CreateClient();
    //     var client = CreateClient(httpClient);
    //
    //     var responseHandler = new NativeResponseHandler();
    //     await client.Foo[1].PutAsync(
    //         new FooProperties { Name = "test" },
    //         config =>
    //         {
    //             config.Headers.Add("Bar", "test");
    //             config.Options.Add(new ResponseHandlerOption { ResponseHandler = responseHandler });
    //         },
    //         CancellationToken);
    //     var result = (HttpResponseMessage)responseHandler.Value;
    //
    //     result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    // }
    //
    // [Fact]
    // public async Task Given_unauthorized_request_When_Updating_Foo_It_Should_Return_403()
    // {
    //     using var httpClient = app.CreateClient().WithOAuth2ImplicitFlowAuthentication();
    //     var client = CreateClient(httpClient);
    //
    //     var responseHandler = new NativeResponseHandler();
    //     await client.Foo[1].PutAsync(
    //         new FooProperties { Name = "test" },
    //         config =>
    //         {
    //             config.Headers.Add("Bar", "test");
    //             config.Options.Add(new ResponseHandlerOption { ResponseHandler = responseHandler });
    //         },
    //         CancellationToken);
    //     var result = (HttpResponseMessage)responseHandler.Value;
    //
    //     result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    // }
}
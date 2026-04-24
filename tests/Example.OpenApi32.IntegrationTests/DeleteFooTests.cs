using System.Net;
using AwesomeAssertions;

namespace Example.OpenApi32.IntegrationTests;

public class DeleteFooTests(FooApplicationFactory app) : FooTestSpecification, IClassFixture<FooApplicationFactory>
{
    [Fact]
    public async Task When_Deleting_Foo_It_Should_Return_Ok()
    {
        // using var httpClient = app.CreateClient();
        // var adapter = new HttpClientRequestAdapter(
        //     new AnonymousAuthenticationProvider(),
        //     httpClient: httpClient);
        // var client = new FooApiClient(adapter);
        //
        // var responseHandler = new NativeResponseHandler();
        // await client.Foo[1]
        //     .DeleteAsync(config => config.Options.Add(new ResponseHandlerOption { ResponseHandler = responseHandler }),
        //         CancellationToken);
        // var result = (HttpResponseMessage)responseHandler.Value;
        //
        // result.StatusCode.Should().Be(HttpStatusCode.OK);
        // var responseContent = await result.Content.ReadAsByteArrayAsync(CancellationToken);
        // responseContent.Should().BeEmpty();
        // result.Content.Headers.ContentType.Should().BeNull();
        //
        // result.Headers.Should().BeEmpty();
    }
}

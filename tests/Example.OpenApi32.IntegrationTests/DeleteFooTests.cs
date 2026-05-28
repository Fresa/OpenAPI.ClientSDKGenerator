using AwesomeAssertions;

namespace Example.OpenApi32.IntegrationTests;

public class DeleteFooTests(FooApplicationFactory app) : FooTestSpecification, IClassFixture<FooApplicationFactory>
{
    [Fact]
    public async Task DeletingFoo_ReturnsOk()
    {
        using var httpClient = app.CreateClient();
        var client = new Foo.Foo(httpClient);
        var response = await client.Foo_(10)
            .DeleteAsync(CancellationToken);
        response.Should().BeOfType<Foo.Foo.Foo1.DeleteResponse.OK200.Empty>();
    }
}

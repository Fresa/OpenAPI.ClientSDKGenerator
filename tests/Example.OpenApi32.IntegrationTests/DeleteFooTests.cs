namespace Example.OpenApi32.IntegrationTests;

public class DeleteFooTests(FooApplicationFactory app) : FooTestSpecification, IClassFixture<FooApplicationFactory>
{
    [Fact]
    public async Task When_Deleting_Foo_It_Should_Return_Ok()
    {
        using var httpClient = new HttpClient();
        var client = new Foo.Foo(httpClient);
        client.Foo_(10);
    }
}

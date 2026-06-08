namespace Example.OpenApi30.IntegrationTests;

public abstract class FooTestSpecification
{
    protected CancellationToken CancellationToken { get; } = TestContext.Current.CancellationToken;
}

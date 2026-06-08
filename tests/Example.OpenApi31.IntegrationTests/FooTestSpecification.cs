namespace Example.OpenApi31.IntegrationTests;

public abstract class FooTestSpecification
{
    protected CancellationToken CancellationToken { get; } = TestContext.Current.CancellationToken;
}
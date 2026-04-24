using JetBrains.Annotations;

namespace Example.OpenApi32.IntegrationTests;

[UsedImplicitly]
public class FooApplicationFactory : IAsyncLifetime
{
    public async ValueTask DisposeAsync()
    {
        // TODO release managed resources here
    }

    public ValueTask InitializeAsync()
    {
        return new ValueTask();
    }
}

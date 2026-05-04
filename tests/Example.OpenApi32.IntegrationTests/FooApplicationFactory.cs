using JetBrains.Annotations;
using Microsoft.AspNetCore.TestHost;

namespace Example.OpenApi32.IntegrationTests;

[UsedImplicitly]
                                                                                        
public sealed class FooApplicationFactory : IAsyncLifetime                          
{                                                                                     
    private WebApplication? _app;                                             
                  
    public HttpClient CreateClient() => _app?.GetTestClient() ?? throw new InvalidOperationException("Test server not started");

    public ValueTask DisposeAsync() => _app == null ? ValueTask.CompletedTask : new ValueTask(_app.StopAsync());

    public ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();                                 
        builder.WebHost.UseTestServer();                                              
        builder.AddOperations(builder.Configuration.Get<WebApiConfiguration>());
        _app = builder.Build();                                                       
        _app.MapOperations();                                                         
        return new ValueTask(_app.StartAsync());
    }
}
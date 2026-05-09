using Corvus.Json;
using Example.OpenApi.Auth;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.TestHost;
using Microsoft.IdentityModel.Tokens;

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
        builder.Services.AddAuthentication()
            .AddJwtBearer(SecuritySchemes.PetstoreAuthKey, options =>
            {
                var authority =
                    new Uri(SecuritySchemes.PetstoreAuth.Flows.Implicit.AuthorizationUrl).GetLeftPart(UriPartial.Authority);
                options.Authority = authority;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = authority,
                    ValidAudience = authority,
                };
            })
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                SecuritySchemes.SecretKeyKey,
                options =>
                {
                    options.GetApiKey = context =>
                    {
                        var parameter = SecuritySchemes.SecretKey.GetParameter(context);
                        return parameter.Validate(ValidationContext.ValidContext).IsValid
                            ? (true, parameter.GetString()!)
                            : (false, null);
                    };
                })
            .AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>(
                SecuritySchemes.BasicAuthKey,
                _ => { })
            .AddCertificate(SecuritySchemes.MutualTLSKey, options =>
            {
                options.AllowedCertificateTypes = CertificateTypes.All;
            })
            .AddCookie()
            .AddOpenIdConnect(SecuritySchemes.OpenIdConnectKey, options =>
            {
                options.Authority = SecuritySchemes.OpenIdConnect.OpenIdConnectUrl;
                options.ClientId = "example-client";
                options.SignInScheme = "Cookies";
            });
        builder.AddOperations(builder.Configuration.Get<WebApiConfiguration>());
        _app = builder.Build();                                                       
        _app.MapOperations();                                                         
        return new ValueTask(_app.StartAsync());
    }
}
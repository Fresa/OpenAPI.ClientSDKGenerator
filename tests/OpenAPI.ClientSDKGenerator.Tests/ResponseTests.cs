using System.Threading;
using AwesomeAssertions;
using OpenAPI.ClientSDKGenerator.Tests.Utils;
using Xunit;

namespace OpenAPI.ClientSDKGenerator.Tests;

public class ResponseTests(ITestOutputHelper testOutputHelper)
{
    private CancellationToken Cancellation => TestContext.Current.CancellationToken;

    [Fact]
    public void SingleOkResponseWithoutContent_GeneratesEmptyResponseClass()
    {
        const string spec = """
        {
          "openapi": "3.0.0",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/foo": {
              "get": { "responses": { "200": { "description": "OK" } } }
            }
          }
        }
        """;

        var compilation = ClientSdkGenerator.SetupFromContent(spec,
            clientName: "TestClient",
            @namespace: "Example",
            cancellationToken: Cancellation,
            diagnostics: out var diagnostics);

        diagnostics.Should().BeEmpty();
        compilation.Output("TestClient.Foo0.GetResponse.g.cs", testOutputHelper, Cancellation);
        compilation.GetSource("TestClient.Foo0.GetResponse.g.cs", Cancellation).Should().Be(
""""
#nullable enable
using Corvus.Json;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Example;
internal sealed partial class TestClient
{
    internal sealed partial class Foo0
    {
        /// <summary>
        /// Contains the operation's response objects
        /// </summary>
        internal abstract partial class GetResponse
        {
            /// <summary>
            /// Check if status code is 1xx
            /// </summary>
            /// <param name="code">Status code to match</param>
            /// <returns>true if code matches</returns>
            protected static bool Matches1xxStatusCode(int code) =>
                code >= 100 && code <= 199;

            /// <summary>
            /// Check if status code is 2xx
            /// </summary>
            /// <param name="code">Status code to match</param>
            /// <returns>true if code matches</returns>
            protected static bool Matches2xxStatusCode(int code) =>
                code >= 200 && code <= 299;

            /// <summary>
            /// Check if status code is 3xx
            /// </summary>
            /// <param name="code">Status code to match</param>
            /// <returns>true if code matches</returns>
            protected static bool Matches3xxStatusCode(int code) =>
                code >= 300 && code <= 399;

            /// <summary>
            /// Check if status code is 4xx
            /// </summary>
            /// <param name="code">Status code to match</param>
            /// <returns>true if code matches</returns>
            protected static bool Matches4xxStatusCode(int code) =>
                code >= 400 && code <= 499;

            /// <summary>
            /// Check if status code is 5xx
            /// </summary>
            /// <param name="code">Status code to match</param>
            /// <returns>true if code matches</returns>
            protected static bool Matches5xxStatusCode(int code) =>
                code >= 500 && code <= 599;

            /// <summary>
            /// Validate the response
            /// </summary>
            /// <param name="validationLevel">Validation level</param>
            /// <returns>The validation result</returns>
            internal abstract ValidationContext Validate(ValidationLevel validationLevel);

            /// <summary>
            /// Read response content as json
            /// </summary>
            /// <param name="response">Response message</param>
            /// <param name="cancellationToken">Cancellation token</param>
            protected static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
            {
                var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
                    .ConfigureAwait(false);
                var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                return document.RootElement.Clone();
            }

            /// <summary>
            /// Construct response
            /// </summary>
            /// <param name="response">Response message</param>
            /// <param name="cancellationToken">Cancellation token</param>
            internal static Task<GetResponse> BindAsync(HttpResponseMessage response, CancellationToken cancellationToken = default) =>
                response.StatusCode switch
                {
                    _ when OK200.MatchesStatusCode(response.StatusCode) => OK200.BindAsync(response, cancellationToken),
                    _ => GetResponse.Unknown.BindAsync(response, cancellationToken)
                };

            /// <summary>
            /// Unknown response
            /// </summary>
            internal sealed class Unknown : GetResponse
            {
                internal Stream Content { get; }

                private Unknown(Stream content, HttpResponseMessage response)
                {
                    Content = content;
                    StatusCode = response.StatusCode;
                }

                /// <summary>
                /// Construct unknown response
                /// </summary>
                /// <param name="response">Response message</param>
                /// <param name="cancellationToken">Cancellation token</param>
                internal new static async Task<GetResponse> BindAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
                {
                    var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
                        .ConfigureAwait(false);
                    return new Unknown(stream, response);
                }

                /// <summary>
                /// Response status code
                /// </summary>
                internal HttpStatusCode StatusCode { get; private set; }

                /// <inheritdoc/>
                internal override ValidationContext Validate(ValidationLevel validationLevel) =>
                    ValidationContext.ValidContext.UsingStack().UsingResults();
            }

            /// <summary>
            /// <para>
            /// OK
            /// </para>
            /// </summary>
            internal abstract class OK200 : GetResponse
            {
                /// <summary>
                /// Response with empty content
                /// </summary>
                internal sealed class Empty : OK200
                {
                    private Empty(HttpResponseMessage response)
                    {
                        StatusCode = response.StatusCode;
                    }

                    /// <summary>
                    /// Construct response for empty content
                    /// </summary>
                    /// <param name="response">Response message</param>
                    /// <param name="cancellationToken">Cancellation token</param>
                    internal new static Task<GetResponse> BindAsync(HttpResponseMessage response, CancellationToken cancellationToken = default) =>
                        Task.FromResult<GetResponse>(new Empty(response));

                    /// <inheritdoc/>
                    internal override ValidationContext Validate(ValidationLevel validationLevel) =>
                        base.Validate(validationLevel);
                }

                internal static bool MatchesStatusCode(HttpStatusCode statusCode) =>
                    ((int)statusCode) == 200;

                /// <summary>
                /// Response status code
                /// </summary>
                internal HttpStatusCode StatusCode { get; private set; }

                /// <summary>
                /// Bind content from http response
                /// </summary>
                /// <param name="response">Http response message to bind from</param>
                /// <param name="cancellationToken">Cancellation token</param>
                /// <returns>An awaitable task for the response content</returns>
                internal static Task<GetResponse> BindAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
                {
                    return Empty.BindAsync(response, cancellationToken);
                }

                /// <summary>
                /// Create a validation context
                /// </summary>
                /// <returns>Validation context</returns>
                protected ValidationContext CreateValidationContext() =>
                    ValidationContext.ValidContext.UsingStack().UsingResults();

                /// <inheritdoc/>
                internal override ValidationContext Validate(ValidationLevel validationLevel)
                {
                    var validationContext = CreateValidationContext();
                    return validationContext;
                }
            }
        }
    }
}
#nullable restore
"""".ReplaceLineEndings("\n"));
    }

    [Fact]
    public void DefaultStatusCode_DefaultClassMatchesAllStatusCodes()
    {
        const string spec = """
        {
          "openapi": "3.0.0",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/foo": {
              "get": {
                "responses": {
                  "default": { "description": "Default response" }
                }
              }
            }
          }
        }
        """;

        var compilation = ClientSdkGenerator.SetupFromContent(spec,
            clientName: "TestClient",
            @namespace: "Example",
            cancellationToken: Cancellation,
            diagnostics: out var diagnostics);

        diagnostics.Should().BeEmpty();

        compilation.GetSource("TestClient.Foo0.GetResponse.g.cs", Cancellation).Should().Be(
""""
#nullable enable
using Corvus.Json;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Example;
internal sealed partial class TestClient
{
    internal sealed partial class Foo0
    {
        /// <summary>
        /// Contains the operation's response objects
        /// </summary>
        internal abstract partial class GetResponse
        {
            /// <summary>
            /// Check if status code is 1xx
            /// </summary>
            /// <param name="code">Status code to match</param>
            /// <returns>true if code matches</returns>
            protected static bool Matches1xxStatusCode(int code) =>
                code >= 100 && code <= 199;

            /// <summary>
            /// Check if status code is 2xx
            /// </summary>
            /// <param name="code">Status code to match</param>
            /// <returns>true if code matches</returns>
            protected static bool Matches2xxStatusCode(int code) =>
                code >= 200 && code <= 299;

            /// <summary>
            /// Check if status code is 3xx
            /// </summary>
            /// <param name="code">Status code to match</param>
            /// <returns>true if code matches</returns>
            protected static bool Matches3xxStatusCode(int code) =>
                code >= 300 && code <= 399;

            /// <summary>
            /// Check if status code is 4xx
            /// </summary>
            /// <param name="code">Status code to match</param>
            /// <returns>true if code matches</returns>
            protected static bool Matches4xxStatusCode(int code) =>
                code >= 400 && code <= 499;

            /// <summary>
            /// Check if status code is 5xx
            /// </summary>
            /// <param name="code">Status code to match</param>
            /// <returns>true if code matches</returns>
            protected static bool Matches5xxStatusCode(int code) =>
                code >= 500 && code <= 599;

            /// <summary>
            /// Validate the response
            /// </summary>
            /// <param name="validationLevel">Validation level</param>
            /// <returns>The validation result</returns>
            internal abstract ValidationContext Validate(ValidationLevel validationLevel);

            /// <summary>
            /// Read response content as json
            /// </summary>
            /// <param name="response">Response message</param>
            /// <param name="cancellationToken">Cancellation token</param>
            protected static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
            {
                var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
                    .ConfigureAwait(false);
                var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                return document.RootElement.Clone();
            }

            /// <summary>
            /// Construct response
            /// </summary>
            /// <param name="response">Response message</param>
            /// <param name="cancellationToken">Cancellation token</param>
            internal static Task<GetResponse> BindAsync(HttpResponseMessage response, CancellationToken cancellationToken = default) =>
                response.StatusCode switch
                {
                    _ when Default.MatchesStatusCode(response.StatusCode) => Default.BindAsync(response, cancellationToken),
                    _ => GetResponse.Unknown.BindAsync(response, cancellationToken)
                };

            /// <summary>
            /// Unknown response
            /// </summary>
            internal sealed class Unknown : GetResponse
            {
                internal Stream Content { get; }

                private Unknown(Stream content, HttpResponseMessage response)
                {
                    Content = content;
                    StatusCode = response.StatusCode;
                }

                /// <summary>
                /// Construct unknown response
                /// </summary>
                /// <param name="response">Response message</param>
                /// <param name="cancellationToken">Cancellation token</param>
                internal new static async Task<GetResponse> BindAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
                {
                    var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
                        .ConfigureAwait(false);
                    return new Unknown(stream, response);
                }

                /// <summary>
                /// Response status code
                /// </summary>
                internal HttpStatusCode StatusCode { get; private set; }

                /// <inheritdoc/>
                internal override ValidationContext Validate(ValidationLevel validationLevel) =>
                    ValidationContext.ValidContext.UsingStack().UsingResults();
            }

            /// <summary>
            /// <para>
            /// Default response
            /// </para>
            /// </summary>
            internal abstract class Default : GetResponse
            {
                /// <summary>
                /// Response with empty content
                /// </summary>
                internal sealed class Empty : Default
                {
                    private Empty(HttpResponseMessage response)
                    {
                        StatusCode = response.StatusCode;
                    }

                    /// <summary>
                    /// Construct response for empty content
                    /// </summary>
                    /// <param name="response">Response message</param>
                    /// <param name="cancellationToken">Cancellation token</param>
                    internal new static Task<GetResponse> BindAsync(HttpResponseMessage response, CancellationToken cancellationToken = default) =>
                        Task.FromResult<GetResponse>(new Empty(response));

                    /// <inheritdoc/>
                    internal override ValidationContext Validate(ValidationLevel validationLevel) =>
                        base.Validate(validationLevel);
                }

                internal static bool MatchesStatusCode(HttpStatusCode statusCode) =>
                    true;

                /// <summary>
                /// Response status code
                /// </summary>
                internal HttpStatusCode StatusCode { get; private set; }

                /// <summary>
                /// Bind content from http response
                /// </summary>
                /// <param name="response">Http response message to bind from</param>
                /// <param name="cancellationToken">Cancellation token</param>
                /// <returns>An awaitable task for the response content</returns>
                internal static Task<GetResponse> BindAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
                {
                    return Empty.BindAsync(response, cancellationToken);
                }

                /// <summary>
                /// Create a validation context
                /// </summary>
                /// <returns>Validation context</returns>
                protected ValidationContext CreateValidationContext() =>
                    ValidationContext.ValidContext.UsingStack().UsingResults();

                /// <inheritdoc/>
                internal override ValidationContext Validate(ValidationLevel validationLevel)
                {
                    var validationContext = CreateValidationContext();
                    return validationContext;
                }
            }
        }
    }
}
#nullable restore
"""".ReplaceLineEndings("\n"));
    }

    [Fact]
    public void ResponseWithJsonContent_GeneratesContentTypedResponseClass()
    {
        const string spec = """
        {
          "openapi": "3.0.0",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/foo": {
              "get": {
                "responses": {
                  "200": {
                    "description": "OK",
                    "content": {
                      "application/json": {
                        "schema": {
                          "type": "object",
                          "properties": {
                            "name": { "type": "string" }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
        """;

        var compilation = ClientSdkGenerator.SetupFromContent(spec,
            clientName: "TestClient",
            @namespace: "Example",
            cancellationToken: Cancellation,
            diagnostics: out var diagnostics);

        diagnostics.Should().BeEmpty();
        compilation.GetSource("TestClient.Foo0.GetResponse.g.cs", Cancellation).Should().Be(
""""
#nullable enable
using Corvus.Json;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Example;
internal sealed partial class TestClient
{
    internal sealed partial class Foo0
    {
        /// <summary>
        /// Contains the operation's response objects
        /// </summary>
        internal abstract partial class GetResponse
        {
            /// <summary>
            /// Check if status code is 1xx
            /// </summary>
            /// <param name="code">Status code to match</param>
            /// <returns>true if code matches</returns>
            protected static bool Matches1xxStatusCode(int code) =>
                code >= 100 && code <= 199;

            /// <summary>
            /// Check if status code is 2xx
            /// </summary>
            /// <param name="code">Status code to match</param>
            /// <returns>true if code matches</returns>
            protected static bool Matches2xxStatusCode(int code) =>
                code >= 200 && code <= 299;

            /// <summary>
            /// Check if status code is 3xx
            /// </summary>
            /// <param name="code">Status code to match</param>
            /// <returns>true if code matches</returns>
            protected static bool Matches3xxStatusCode(int code) =>
                code >= 300 && code <= 399;

            /// <summary>
            /// Check if status code is 4xx
            /// </summary>
            /// <param name="code">Status code to match</param>
            /// <returns>true if code matches</returns>
            protected static bool Matches4xxStatusCode(int code) =>
                code >= 400 && code <= 499;

            /// <summary>
            /// Check if status code is 5xx
            /// </summary>
            /// <param name="code">Status code to match</param>
            /// <returns>true if code matches</returns>
            protected static bool Matches5xxStatusCode(int code) =>
                code >= 500 && code <= 599;

            /// <summary>
            /// Validate the response
            /// </summary>
            /// <param name="validationLevel">Validation level</param>
            /// <returns>The validation result</returns>
            internal abstract ValidationContext Validate(ValidationLevel validationLevel);

            /// <summary>
            /// Read response content as json
            /// </summary>
            /// <param name="response">Response message</param>
            /// <param name="cancellationToken">Cancellation token</param>
            protected static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
            {
                var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
                    .ConfigureAwait(false);
                var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                return document.RootElement.Clone();
            }

            /// <summary>
            /// Construct response
            /// </summary>
            /// <param name="response">Response message</param>
            /// <param name="cancellationToken">Cancellation token</param>
            internal static Task<GetResponse> BindAsync(HttpResponseMessage response, CancellationToken cancellationToken = default) =>
                response.StatusCode switch
                {
                    _ when OK200.MatchesStatusCode(response.StatusCode) => OK200.BindAsync(response, cancellationToken),
                    _ => GetResponse.Unknown.BindAsync(response, cancellationToken)
                };

            /// <summary>
            /// Unknown response
            /// </summary>
            internal sealed class Unknown : GetResponse
            {
                internal Stream Content { get; }

                private Unknown(Stream content, HttpResponseMessage response)
                {
                    Content = content;
                    StatusCode = response.StatusCode;
                }

                /// <summary>
                /// Construct unknown response
                /// </summary>
                /// <param name="response">Response message</param>
                /// <param name="cancellationToken">Cancellation token</param>
                internal new static async Task<GetResponse> BindAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
                {
                    var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
                        .ConfigureAwait(false);
                    return new Unknown(stream, response);
                }

                /// <summary>
                /// Response status code
                /// </summary>
                internal HttpStatusCode StatusCode { get; private set; }

                /// <inheritdoc/>
                internal override ValidationContext Validate(ValidationLevel validationLevel) =>
                    ValidationContext.ValidContext.UsingStack().UsingResults();
            }

            /// <summary>
            /// <para>
            /// OK
            /// </para>
            /// </summary>
            internal abstract class OK200 : GetResponse
            {
                /// <summary>
                /// Response for content application/json
                /// </summary>
                internal sealed class ApplicationJson : OK200
                {
                    internal Components.Schemas.FooGet200ApplicationJson Content { get; }

                    private ApplicationJson(JsonElement content, HttpResponseMessage response)
                    {
                        Content = Components.Schemas.FooGet200ApplicationJson.FromJson(content);
                        StatusCode = response.StatusCode;
                    }

                    /// <summary>
                    /// Construct response for content application/json
                    /// </summary>
                    /// <param name="response">Response message</param>
                    /// <param name="cancellationToken">Cancellation token</param>
                    internal new static async Task<GetResponse> BindAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
                    {
                        var content = await OK200.ReadJsonAsync(response, cancellationToken)
                            .ConfigureAwait(false);
                        return new ApplicationJson(content, response);
                    }

                    internal static MediaTypeHeaderValue MediaType { get; } = MediaTypeHeaderValue.Parse("application/json");

                    private const string ContentSchemaLocation = "#/paths/~1foo/get/responses/200/content/application~1json/schema";

                    /// <inheritdoc/>
                    internal override ValidationContext Validate(ValidationLevel validationLevel)
                    {
                        var validationContext = base.Validate(validationLevel);
                        return Content.Validate(ContentSchemaLocation, true, validationContext, validationLevel);
                    }
                }

                /// <summary>
                /// Response for unknown content
                /// </summary>
                internal sealed class Unknown : OK200
                {
                    internal Stream Content { get; }

                    private Unknown(Stream content, HttpResponseMessage response)
                    {
                        Content = content;
                        StatusCode = response.StatusCode;
                    }

                    /// <summary>
                    /// Construct response for unknown content
                    /// </summary>
                    /// <param name="response">Response message</param>
                    /// <param name="cancellationToken">Cancellation token</param>
                    internal new static async Task<GetResponse> BindAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
                    {
                        var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
                            .ConfigureAwait(false);
                        return new Unknown(stream, response);
                    }

                    /// <inheritdoc/>
                    internal override ValidationContext Validate(ValidationLevel validationLevel) =>
                        base.Validate(validationLevel);
                }

                internal static bool MatchesStatusCode(HttpStatusCode statusCode) =>
                    ((int)statusCode) == 200;

                /// <summary>
                /// Response status code
                /// </summary>
                internal HttpStatusCode StatusCode { get; private set; }

                /// <summary>
                /// Bind content from http response
                /// </summary>
                /// <param name="response">Http response message to bind from</param>
                /// <param name="cancellationToken">Cancellation token</param>
                /// <returns>An awaitable task for the response content</returns>
                internal static Task<GetResponse> BindAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
                {
                    var contentType = response.Content.Headers.ContentType;
                    return contentType switch
                    {
                        null => Unknown.BindAsync(response, cancellationToken),
                        _ when contentType.IsSubset(ApplicationJson.MediaType) => ApplicationJson.BindAsync(response, cancellationToken),
                        _ => Unknown.BindAsync(response, cancellationToken)
                    };
                }

                /// <summary>
                /// Create a validation context
                /// </summary>
                /// <returns>Validation context</returns>
                protected ValidationContext CreateValidationContext() =>
                    ValidationContext.ValidContext.UsingStack().UsingResults();

                /// <inheritdoc/>
                internal override ValidationContext Validate(ValidationLevel validationLevel)
                {
                    var validationContext = CreateValidationContext();
                    return validationContext;
                }
            }
        }
    }
}
#nullable restore
"""".ReplaceLineEndings("\n"));
    }
}
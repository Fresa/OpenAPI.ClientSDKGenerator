using System.Threading;
using AwesomeAssertions;
using OpenAPI.ClientSDKGenerator.Tests.Utils;
using Xunit;

namespace OpenAPI.ClientSDKGenerator.Tests;

public class EntityTests(ITestOutputHelper testOutputHelper)
{
    private CancellationToken Cancellation => TestContext.Current.CancellationToken;

    [Fact]
    public void ClientNameThatOverlapsWithARootEntity_TheOverlappingEntityShouldBeRenamed()
    {
        const string spec = """
        {
          "openapi": "3.0.0",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/pets": {
              "get": { "responses": { "200": { "description": "OK" } } }
            },
            "/pets/{petId}": {
              "parameters": [
                { "name": "petId", "in": "path", "required": true, "schema": { "type": "string" } }
              ],
              "get": { "responses": { "200": { "description": "OK" } } }
            }
          }
        }
        """;

        var compilation = ClientSdkGenerator.SetupFromContent(spec,
            clientName: "Pets",
            @namespace: "Example",
            cancellationToken: Cancellation,
            diagnostics: out var diagnostics);

        diagnostics.Should().BeEmpty();

        var source = compilation.GetSource("Pets.Pets.g.cs", Cancellation);
        source.Should().Be(
""""
#nullable enable
using Corvus.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Example;
internal sealed partial class Pets
{
    internal Pets0 Pets_()
    {
        var requestBuilder = new RequestBuilder(httpClient, _configuration);
        return new(requestBuilder);
    }

    internal sealed partial class Pets0(RequestBuilder requestBuilder)
    {
        internal Task GetAsync(
            CancellationToken cancellation = default) =>
            requestBuilder
                .SendAsync(
                    "/pets",
                    "GET",
                    null,
                    cancellation);
    }

    internal Pets1 Pets_(
        Corvus.Json.JsonString petId)
    {
        var requestBuilder = new RequestBuilder(httpClient, _configuration);
        requestBuilder.AddPathParameter("petId",
            petId,
            "#/paths/~1pets~1{petId}/parameters/0/schema",
            """
            {
              "name": "petId",
              "in": "path",
              "required": true,
              "schema": {
                "type": "string"
              }
            }
            """);
        return new(requestBuilder);
    }

    internal sealed partial class Pets1(RequestBuilder requestBuilder)
    {
        internal Task GetAsync(
            CancellationToken cancellation = default) =>
            requestBuilder
                .SendAsync(
                    "/pets/{petId}",
                    "GET",
                    null,
                    cancellation);
    }
}
#nullable restore
"""");
    }

    [Fact]
    public void MultipleRootPaths_EachPathShouldGetItsOwnRootEntity()
    {
        const string spec = """
        {
          "openapi": "3.0.0",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/foo": { "get": { "responses": { "200": { "description": "OK" } } } },
            "/bar": { "get": { "responses": { "200": { "description": "OK" } } } },
            "/baz": { "get": { "responses": { "200": { "description": "OK" } } } }
          }
        }
        """;

        var compilation = ClientSdkGenerator.SetupFromContent(spec,
            clientName: "TestClient",
            @namespace: "Example",
            cancellationToken: Cancellation,
            diagnostics: out var diagnostics);

        diagnostics.Should().BeEmpty();

        compilation.GetSource("TestClient.Foo.g.cs", Cancellation).Should().Be(
""""
#nullable enable
using Corvus.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Example;
internal sealed partial class TestClient
{
    internal Foo0 Foo()
    {
        var requestBuilder = new RequestBuilder(httpClient, _configuration);
        return new(requestBuilder);
    }

    internal sealed partial class Foo0(RequestBuilder requestBuilder)
    {
        internal Task GetAsync(
            CancellationToken cancellation = default) =>
            requestBuilder
                .SendAsync(
                    "/foo",
                    "GET",
                    null,
                    cancellation);
    }
}
#nullable restore
"""");

        compilation.GetSource("TestClient.Bar.g.cs", Cancellation).Should().Be(
""""
#nullable enable
using Corvus.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Example;
internal sealed partial class TestClient
{
    internal Bar0 Bar()
    {
        var requestBuilder = new RequestBuilder(httpClient, _configuration);
        return new(requestBuilder);
    }

    internal sealed partial class Bar0(RequestBuilder requestBuilder)
    {
        internal Task GetAsync(
            CancellationToken cancellation = default) =>
            requestBuilder
                .SendAsync(
                    "/bar",
                    "GET",
                    null,
                    cancellation);
    }
}
#nullable restore
"""");

        compilation.GetSource("TestClient.Baz.g.cs", Cancellation).Should().Be(
""""
#nullable enable
using Corvus.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Example;
internal sealed partial class TestClient
{
    internal Baz0 Baz()
    {
        var requestBuilder = new RequestBuilder(httpClient, _configuration);
        return new(requestBuilder);
    }

    internal sealed partial class Baz0(RequestBuilder requestBuilder)
    {
        internal Task GetAsync(
            CancellationToken cancellation = default) =>
            requestBuilder
                .SendAsync(
                    "/baz",
                    "GET",
                    null,
                    cancellation);
    }
}
#nullable restore
"""");
    }

    [Fact]
    public void MultipleOperations_EntityShouldHaveOneMethodPerOperation()
    {
        const string spec = """
        {
          "openapi": "3.0.0",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/items/{id}": {
              "parameters": [
                { "name": "id", "in": "path", "required": true, "schema": { "type": "string" } }
              ],
              "get": { "responses": { "200": { "description": "OK" } } },
              "put": { "responses": { "200": { "description": "OK" } } },
              "delete": { "responses": { "200": { "description": "OK" } } }
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

        compilation.GetSource("TestClient.Items.g.cs", Cancellation).Should().Be(
""""
#nullable enable
using Corvus.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Example;
internal sealed partial class TestClient
{
    internal Items1 Items(
        Corvus.Json.JsonString id)
    {
        var requestBuilder = new RequestBuilder(httpClient, _configuration);
        requestBuilder.AddPathParameter("id",
            id,
            "#/paths/~1items~1{id}/parameters/0/schema",
            """
            {
              "name": "id",
              "in": "path",
              "required": true,
              "schema": {
                "type": "string"
              }
            }
            """);
        return new(requestBuilder);
    }

    internal sealed partial class Items1(RequestBuilder requestBuilder)
    {
        internal Task GetAsync(
            CancellationToken cancellation = default) =>
            requestBuilder
                .SendAsync(
                    "/items/{id}",
                    "GET",
                    null,
                    cancellation);

        internal Task PutAsync(
            CancellationToken cancellation = default) =>
            requestBuilder
                .SendAsync(
                    "/items/{id}",
                    "PUT",
                    null,
                    cancellation);

        internal Task DeleteAsync(
            CancellationToken cancellation = default) =>
            requestBuilder
                .SendAsync(
                    "/items/{id}",
                    "DELETE",
                    null,
                    cancellation);
    }
}
#nullable restore
"""");
    }

    [Fact]
    public void NestedPath_ChildEntityShouldBeContainedByParentEntity()
    {
        const string spec = """
        {
          "openapi": "3.0.0",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/parent/{id}/child": {
              "parameters": [
                { "name": "id", "in": "path", "required": true, "schema": { "type": "string" } }
              ],
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

        compilation.GetSource("TestClient.Parent.g.cs", Cancellation).Should().Be(
""""
#nullable enable
using Corvus.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Example;
internal sealed partial class TestClient
{
    internal Parent1 Parent(
        Corvus.Json.JsonString id)
    {
        var requestBuilder = new RequestBuilder(httpClient, _configuration);
        requestBuilder.AddPathParameter("id",
            id,
            "#/paths/~1parent~1{id}~1child/parameters/0/schema",
            """
            {
              "name": "id",
              "in": "path",
              "required": true,
              "schema": {
                "type": "string"
              }
            }
            """);
        return new(requestBuilder);
    }

    internal sealed partial class Parent1(RequestBuilder requestBuilder)
    {
    }
}
#nullable restore
"""");

        compilation.GetSource("TestClient.Parent.Child.g.cs", Cancellation).Should().Be(
""""
#nullable enable
using Corvus.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Example;
internal sealed partial class TestClient
{
    internal sealed partial class Parent1
    {
        internal Child0 Child()
        {
            return new(requestBuilder);
        }

        internal sealed partial class Child0(RequestBuilder requestBuilder)
        {
            internal Task GetAsync(
                CancellationToken cancellation = default) =>
                requestBuilder
                    .SendAsync(
                        "/parent/{id}/child",
                        "GET",
                        null,
                        cancellation);
        }
    }
}
#nullable restore
"""");
    }

    [Fact]
    public void PathWithParameter_MethodShouldIncludeTheParameter()
    {
        const string spec = """
        {
          "openapi": "3.0.0",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/items/{id}": {
              "parameters": [
                { "name": "id", "in": "path", "required": true, "schema": { "type": "string" } }
              ],
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

        compilation.GetSource("TestClient.Items.g.cs", Cancellation).Should().Be(
""""
#nullable enable
using Corvus.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Example;
internal sealed partial class TestClient
{
    internal Items1 Items(
        Corvus.Json.JsonString id)
    {
        var requestBuilder = new RequestBuilder(httpClient, _configuration);
        requestBuilder.AddPathParameter("id",
            id,
            "#/paths/~1items~1{id}/parameters/0/schema",
            """
            {
              "name": "id",
              "in": "path",
              "required": true,
              "schema": {
                "type": "string"
              }
            }
            """);
        return new(requestBuilder);
    }

    internal sealed partial class Items1(RequestBuilder requestBuilder)
    {
        internal Task GetAsync(
            CancellationToken cancellation = default) =>
            requestBuilder
                .SendAsync(
                    "/items/{id}",
                    "GET",
                    null,
                    cancellation);
    }
}
#nullable restore
"""");
    }

    [Fact]
    public void OperationWithRequestBody_GeneratesContentClassAndBodyParameter()
    {
        const string spec = """
        {
          "openapi": "3.0.0",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/items": {
              "post": {
                "requestBody": {
                  "required": true,
                  "content": {
                    "application/json": {
                      "schema": { "type": "object", "properties": { "name": { "type": "string" } } }
                    }
                  }
                },
                "responses": { "200": { "description": "OK" } }
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

        var source = compilation.GetSource("TestClient.Items.g.cs", Cancellation);

        source.Should().Be("""
            #nullable enable
            using Corvus.Json;
            using System.Net.Http.Headers;
            using System.Text;
            
            namespace Example;
            internal sealed partial class TestClient
            {
                internal Items0 Items()
                {
                    var requestBuilder = new RequestBuilder(httpClient, _configuration);
                    return new(requestBuilder);
                }
            
                internal sealed partial class Items0(RequestBuilder requestBuilder)
                {
                    internal Task PostAsync(Content content, 
                        CancellationToken cancellation = default) =>
                        requestBuilder
                            .SendAsync(
                                "/items",
                                "POST",
                                content.Get(),
                                cancellation);

                    internal abstract class Content
                    {
                        internal abstract string? MediaType { get; }
            
                        /// <summary>
                        /// Ensures that the specified content type matches the specification
                        /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified content type does not match the specification</exception>
                        /// </summary>
                        /// <param name="contentType">Content type</param>
                        /// <param name="expectedContentType">Expected content type</param>
                        protected void EnsureExpectedContentType(MediaTypeHeaderValue contentType, MediaTypeHeaderValue expectedContentType)
                        {
                            if (!contentType.IsSubset(expectedContentType))
                            {
                                throw new ArgumentOutOfRangeException($"Expected content type {contentType.MediaType} to be a subset of {expectedContentType.MediaType}");
                            }
                        }
            
                        internal abstract HttpContent Get();

                        internal abstract ValidationContext Validate(ValidationContext validationContext, ValidationLevel validationLevel);
            
                        /// <summary>
                        /// Response for content application/json
                        /// </summary>
                        internal sealed class ApplicationJson : Content
                        {
                            private Example.Paths.Items.Post.RequestBody.Content.ApplicationJson _content;

                            /// <summary>
                            /// Construct response for content application/json
                            /// </summary>
                            /// <param name="applicationJson">Content</param>
                            public ApplicationJson(Example.Paths.Items.Post.RequestBody.Content.ApplicationJson applicationJson)
                            {
                                _content = applicationJson;
                                MediaType = "application/json";
                            }

                            internal override string MediaType { get; }

                            internal override HttpContent Get() =>
                               new StringContent(
                                   _content.Serialize(),
                                   encoding: Encoding.UTF8,
                                   mediaType: MediaType
                               );    
                            private const string ContentSchemaLocation = "#/paths/~1items/post/requestBody/content/application~1json/schema";
                            /// <inheritdoc/>
                            internal override ValidationContext Validate(ValidationContext validationContext, ValidationLevel validationLevel) =>
                                _content.Validate(ContentSchemaLocation, true, validationContext, validationLevel);
                        }
                    }
                }
            }
            #nullable restore
            """);
    }

    [Fact]
    public void OperationWithQueryParameters_GeneratesQueryClassWithInitProperties()
    {
        const string spec = """
        {
          "openapi": "3.0.0",
          "info": { "title": "Test", "version": "1.0.0" },
          "paths": {
            "/items": {
              "get": {
                "parameters": [
                  { "name": "limit",  "in": "query", "required": true,  "schema": { "type": "integer" } },
                  { "name": "filter", "in": "query", "required": false, "schema": { "type": "string"  } }
                ],
                "responses": { "200": { "description": "OK" } }
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

        var source = compilation.GetSource("TestClient.Items.g.cs", Cancellation);
        testOutputHelper.WriteLine(source);

        source.Should().Be(
""""
#nullable enable
using Corvus.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Example;
internal sealed partial class TestClient
{
    internal Items0 Items()
    {
        var requestBuilder = new RequestBuilder(httpClient, _configuration);
        return new(requestBuilder);
    }

    internal sealed partial class Items0(RequestBuilder requestBuilder)
    {
        internal Task GetAsync(Query query,
            CancellationToken cancellation = default) =>
            query.AddTo(requestBuilder)
                .SendAsync(
                    "/items",
                    "GET",
                    null,
                    cancellation);

        internal sealed class Query
        {
            internal required Corvus.Json.JsonInteger Limit { get; init; }
            internal Corvus.Json.JsonString? Filter { get; init; }

            internal RequestBuilder AddTo(RequestBuilder requestBuilder)
            {
                requestBuilder.AddQuery("limit",
                    Limit,
                    true,
                    "#/paths/~1items/get/parameters/0/schema",
                    """
                    {
                      "name": "limit",
                      "in": "query",
                      "required": true,
                      "schema": {
                        "type": "integer"
                      }
                    }
                    """);
                requestBuilder.AddQuery("filter",
                    Filter,
                    false,
                    "#/paths/~1items/get/parameters/1/schema",
                    """
                    {
                      "name": "filter",
                      "in": "query",
                      "schema": {
                        "type": "string"
                      }
                    }
                    """);
                return requestBuilder;
            }
        }
    }
}
#nullable restore
"""");
    }
}
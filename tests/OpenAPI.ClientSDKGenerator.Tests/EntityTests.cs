using System.Threading;
using AwesomeAssertions;
using OpenAPI.ClientSDKGenerator.Tests.Utils;
using Xunit;

namespace OpenAPI.ClientSDKGenerator.Tests;

public class EntityTests(ITestOutputHelper testOutputHelper)
{
    private CancellationToken Cancellation => TestContext.Current.CancellationToken;

    [Fact]
    public void GivenAClientNameThatOverlapsWithARootEntity_WhenGeneratingAPI_TheOverlappingEntityShouldBeRenamed()
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
        internal Task GetAsync(CancellationToken cancellation = default) =>
            requestBuilder.SendAsync(
                "/pets",
                "GET",
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
        internal Task GetAsync(CancellationToken cancellation = default) =>
            requestBuilder.SendAsync(
                "/pets/{petId}",
                "GET",
                cancellation);
    }
}
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
        internal Task GetAsync(CancellationToken cancellation = default) =>
            requestBuilder.SendAsync(
                "/foo",
                "GET",
                cancellation);
    }
}
"""");

        compilation.GetSource("TestClient.Bar.g.cs", Cancellation).Should().Be(
""""
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
        internal Task GetAsync(CancellationToken cancellation = default) =>
            requestBuilder.SendAsync(
                "/bar",
                "GET",
                cancellation);
    }
}
"""");

        compilation.GetSource("TestClient.Baz.g.cs", Cancellation).Should().Be(
""""
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
        internal Task GetAsync(CancellationToken cancellation = default) =>
            requestBuilder.SendAsync(
                "/baz",
                "GET",
                cancellation);
    }
}
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
        internal Task GetAsync(CancellationToken cancellation = default) =>
            requestBuilder.SendAsync(
                "/items/{id}",
                "GET",
                cancellation);

        internal Task PutAsync(CancellationToken cancellation = default) =>
            requestBuilder.SendAsync(
                "/items/{id}",
                "PUT",
                cancellation);

        internal Task DeleteAsync(CancellationToken cancellation = default) =>
            requestBuilder.SendAsync(
                "/items/{id}",
                "DELETE",
                cancellation);
    }
}
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
"""");

        compilation.GetSource("TestClient.Parent.Child.g.cs", Cancellation).Should().Be(
""""
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
            internal Task GetAsync(CancellationToken cancellation = default) =>
                requestBuilder.SendAsync(
                    "/parent/{id}/child",
                    "GET",
                    cancellation);
        }
    }
}
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
        internal Task GetAsync(CancellationToken cancellation = default) =>
            requestBuilder.SendAsync(
                "/items/{id}",
                "GET",
                cancellation);
    }
}
"""");
    }
}
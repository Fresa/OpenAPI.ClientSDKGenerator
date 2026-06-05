extern alias OpenAPIWebClientGenerator;

using System.Net.Http.Headers;
using AwesomeAssertions;
using OpenAPIWebClientGenerator::OpenAPI.WebClientGenerator.Extensions;
using Xunit;

namespace OpenAPI.WebClientGenerator.Tests;

public class MediaTypeExtensionsTests
{
    [Theory]
    [InlineData("*/*", 0)]
    [InlineData("application/*", 100)]
    [InlineData("application/json", 1000)]
    [InlineData("application/json-seq", 1000)]
    [InlineData("application/geo+json", 2000)]
    [InlineData("application/geo+json-seq", 2000)]
    [InlineData("application/json; charset=utf-8", 1001)]
    [InlineData("application/geo+json; charset=utf-8", 2001)]
    public void GetPrecedence_ReturnsExpectedScore(string mediaType, int expected)
    {
        MediaTypeHeaderValue.Parse(mediaType).GetPrecedence().Should().Be(expected);
    }
}
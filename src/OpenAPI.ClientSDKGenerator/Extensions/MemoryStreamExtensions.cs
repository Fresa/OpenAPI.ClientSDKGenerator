using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.YamlReader;
using SharpYaml.Serialization;

namespace OpenAPI.ClientSDKGenerator.Extensions;

internal static class MemoryStreamExtensions
{
    private static readonly OpenApiJsonReader JsonDocumentReader = new();
    private static readonly OpenApiYamlReader YamlDocumentReader = new();
    private static readonly Dictionary<string, IOpenApiReader> DocumentReaders = new()
    {
        { "json", JsonDocumentReader },
        { "yaml", YamlDocumentReader },
        { "yml", YamlDocumentReader }
    };
    
    extension(MemoryStream stream)
    {
        internal (OpenApiDocument, OpenApiSpecVersion) LoadOpenApiDocument(string format)
        {
            var openApiResult = OpenApiDocument.Load(
                stream,
                format,
                new OpenApiReaderSettings
                {
                    Readers = DocumentReaders,
                    LeaveStreamOpen = true
                });
            var version = openApiResult.Diagnostic?.SpecificationVersion ??
                          throw new InvalidOperationException("Unknown openapi version");
            if (openApiResult.Diagnostic.Errors.Any())
            {
                throw new InvalidOperationException(
                    openApiResult.Diagnostic.Errors.AggregateToString(
                        "Errors while parsing OpenAPI specification: ",
                        error => $"{(error.Pointer == null ? "" : $"{error.Pointer}: ")}{error.Message}"));
            }
            var document = openApiResult.Document ??
                           throw new InvalidOperationException(
                               "OpenAPI document is empty");
            return (document, version);
        }

        internal JsonDocument ParseJsonDocument(string format)
        {
            stream.Position = 0;
            return format switch
            {
                "json" => JsonDocument.Parse(stream),
                "yaml" or "yml" => GetFromYaml(),
                _ => throw new InvalidOperationException($"Format {format} not supported")
            };

            JsonDocument GetFromYaml()
            {
                var yamlStream = new YamlStream();
                yamlStream.Load(new StreamReader(stream));
                return JsonDocument.Parse(yamlStream.First().ToJsonNode().ToJsonString());
            }
        }
    }
}
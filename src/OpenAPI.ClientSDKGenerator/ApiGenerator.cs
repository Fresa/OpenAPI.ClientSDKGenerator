using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.OpenApi;
using OpenAPI.ClientSDKGenerator.Extensions;
using OpenAPI.ClientSDKGenerator.OpenApi;
using OpenAPI.ClientSDKGenerator.OpenApi.Visitor;
using AdditionalText = Microsoft.CodeAnalysis.AdditionalText;
using IIncrementalGenerator = Microsoft.CodeAnalysis.IIncrementalGenerator;
using IncrementalGeneratorInitializationContext = Microsoft.CodeAnalysis.IncrementalGeneratorInitializationContext;
using SourceProductionContext = Microsoft.CodeAnalysis.SourceProductionContext;

namespace OpenAPI.ClientSDKGenerator;

[Generator]
public sealed class ApiGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Debugger.Launch();
        var optionsProvider = context.AdditionalTextsProvider
            .Where(text => text.IsOptionsFile())
            .Collect();

        var openapiDocumentProvider = context.AdditionalTextsProvider
            .Where(text => text.IsOpenApiFile())
            .Collect()
            .Select((array, _) =>
                array.FirstOrDefault() ??
                throw new InvalidOperationException(
                    $"No OpenAPI specification found in AdditionalFiles matching {AdditionalTextExtensions.OpenApiFilePattern}"));
        
        
        var openApiProvider = openapiDocumentProvider
            .Combine(optionsProvider)
            .Combine(context.CompilationProvider)
            .Select((tuple, _) => (
                OpenApiSpecification: tuple.Left.Left,
                Options: tuple.Left.Right.FirstOrDefault(),
                Compilation: tuple.Right
            ));

        context.RegisterSourceOutput(openApiProvider,
            WithExceptionReporting<(AdditionalText, AdditionalText?, Microsoft.CodeAnalysis.Compilation)>(GenerateCode));
    }

    private static void GenerateCode(SourceProductionContext context,
        (AdditionalText OpenApiDocument,
        AdditionalText? Options,
            Microsoft.CodeAnalysis.Compilation Compilation) generatorContext)
    {
        var compilation = generatorContext.Compilation;
        var rootNamespace = compilation.Assembly.Name;

        var options = generatorContext.Options.LoadOptions();
        var openApiSpecification = generatorContext.OpenApiDocument.LoadOpenApiSpecification();

        var openApiVersion = openApiSpecification.Version;
        var openApi = openApiSpecification.Document;

        var openApiVisitor = OpenApiVisitor.ForSpecification(openApiSpecification);

        var operations = new List<(string Namespace, KeyValuePair<HttpMethod, OpenApiOperation> Operation)>();
        foreach (var path in openApi.Paths)
        {
            var pathExpression = path.Key;
            var pathItem = path.Value;
            var openApiPathVisitor = openApiVisitor.Visit(pathItem);
            foreach (var parameter in pathItem.Parameters ?? [])
            {
                var schemaReference = openApiPathVisitor.GetSchemaReference(parameter);
            }

            foreach (var openApiOperation in path.Value.GetOperations())
            {
                var openApiOperationVisitor = openApiPathVisitor.Visit(openApiOperation.Key);
                var operationMetadata = TypeMetadata.From(openApiOperationVisitor.Pointer);
                var operationDirectory = operationMetadata.Path;
                var operationNamespace = $"{rootNamespace}.{operationMetadata.Namespace}.{operationMetadata.Name}";
                var operation = openApiOperation.Value;

                foreach (var parameter in operation.GetParameters())
                {
                    var schemaReference = openApiOperationVisitor.GetSchemaReference(parameter);
                }

                var body = operation.RequestBody;
                if (body is not null)
                {
                    var contentGenerators = body.GetContent().Select(pair =>
                    {
                        var mediaType = pair.Value;
                        var schemaReference = openApiOperationVisitor.GetSchemaReference(mediaType);
                        return schemaReference;
                    }).ToList();
                }

                var responses = operation.Responses ??
                                throw new InvalidOperationException(
                                    $"No responses defined for operation at {openApiOperationVisitor.Pointer}");
                var responseBodyGenerators = responses.Select(content =>
                {
                    var response = content.Value;
                    var openApiResponseVisitor = openApiOperationVisitor.Visit(response);
                    
                    var responseContent =
                        // OpenAPI.NET is incorrectly adding content where there is none defined. 
                        // No content definition means NO content.
                        response.Content?.Where(responseContent => 
                            openApiResponseVisitor.HasContent(responseContent.Value)) ?? [];
                    var responseBodyGenerators = responseContent.Select(mediaContent =>
                    {
                        var contentMediaType = mediaContent.Value;
                        var contentSchemaReference = openApiResponseVisitor.GetSchemaReference(contentMediaType);
                        return contentSchemaReference;
                    }).ToList();

                    var responseHeaderGenerators = response.Headers?.Select(valuePair =>
                    {
                        var name = valuePair.Key;
                        var header = valuePair.Value;
                        var responseHeaderSchema = openApiResponseVisitor.GetSchemaReference(header);
                        return responseHeaderSchema;
                    }).ToList() ?? [];

                    return responseHeaderGenerators;
                }).ToList();
                
                operations.Add((operationNamespace, openApiOperation));
                
            }
        }
    }
 
    private static Action<SourceProductionContext, T> WithExceptionReporting<T>(
        Action<SourceProductionContext, T> handler) =>
        (productionContext, input) =>
        {
            try
            {
                handler.Invoke(productionContext, input);
            }
            catch (Exception e)
            {
                var stackTrace = new StackTrace(e, true);
                StackFrame? firstFrameWithLineNumber = null;
                for (var i = 0; i < stackTrace.FrameCount; i++)
                {
                    var frame = stackTrace.GetFrame(i);
                    if (frame.GetFileLineNumber() != 0)
                    {
                        firstFrameWithLineNumber = frame;
                        break;
                    }
                }

                var firstStackTraceLocation = firstFrameWithLineNumber == null ?
                    Location.None :
                    Location.Create(
                        firstFrameWithLineNumber.GetFileName(),
                        new TextSpan(),
                        new LinePositionSpan(
                            new LinePosition(
                                firstFrameWithLineNumber.GetFileLineNumber(),
                                firstFrameWithLineNumber.GetFileColumnNumber()),
                            new LinePosition(
                                firstFrameWithLineNumber.GetFileLineNumber(),
                                firstFrameWithLineNumber.GetFileColumnNumber() + 1)));

                productionContext.ReportDiagnostic(Diagnostic.Create(
                    UnhandledException,
                    location: firstStackTraceLocation,
                    // Only single line https://github.com/dotnet/roslyn/issues/1455
                    messageArgs: [e.ToString().Replace("\r\n", " |").Replace("\n", " |")]));
            }
        };
    
    private static readonly DiagnosticDescriptor UnhandledException =
        new(
            id: "AF0001",
            title: "Unhandled error",
            // Only single line https://github.com/dotnet/roslyn/issues/1455
            messageFormat: "{0}",
            category: "Compiler",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            // Doesn't work
            description: null,
            customTags: WellKnownDiagnosticTags.AnalyzerException);
}
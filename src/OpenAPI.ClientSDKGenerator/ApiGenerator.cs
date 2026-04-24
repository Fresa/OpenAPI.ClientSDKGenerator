using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.OpenApi;
using OpenAPI.ClientSDKGenerator.OpenApi;
using OpenAPI.ClientSDKGenerator.OpenApi.Visitor;
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
        var clientSdkGeneratorProvider = context.AdditionalTextsProvider
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select((pair, _) =>
            {
                var options = pair.Right.GetOptions(pair.Left);
                if (!options.TryGetValue("build_metadata.AdditionalFiles.SourceItemGroup", out var group) ||
                    !string.Equals(group, "ClientSDKGenerator", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
                options.TryGetValue("build_metadata.AdditionalFiles.Namespace", out var @namespace);
                return new ClientSdkGeneratorConfig(pair.Left, @namespace ?? string.Empty);
            })
            .Where(config => config is not null)
            .Select((config, _) => config!)
            .Collect();

        var openApiProvider = clientSdkGeneratorProvider
            .Combine(context.CompilationProvider)
            .Select((tuple, _) => (
                ClientSDKGenerators: tuple.Left,
                Compilation: tuple.Right
            ));

        context.RegisterSourceOutput(openApiProvider,
            WithExceptionReporting<(
                System.Collections.Immutable.ImmutableArray<ClientSdkGeneratorConfig>, 
                Compilation)>(GenerateCode));
    }

    private static void GenerateCode(SourceProductionContext context,
        (ImmutableArray<ClientSdkGeneratorConfig> ClientSDKGeneratorConfigs, Compilation Compilation) generatorContext)
    {
        foreach (var clientSdkConfig in generatorContext.ClientSDKGeneratorConfigs)
        {
            GenerateSdk(context, clientSdkConfig, generatorContext.Compilation);
        }
    }
    
    private static void GenerateSdk(SourceProductionContext context,
        ClientSdkGeneratorConfig config, Compilation compilation)
    {
        var rootNamespace = compilation.Assembly.Name;
        var openApiSpecification = config.LoadOpenApiSpecification();
        
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
                        firstFrameWithLineNumber.GetFileName() ?? string.Empty,
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
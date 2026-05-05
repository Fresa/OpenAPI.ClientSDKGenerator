using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using OpenAPI.ClientSDKGenerator.Tests.Utils;

namespace OpenAPI.ClientSDKGenerator.Tests;

internal static class ClientSdkGenerator
{
    internal static Compilation Setup(string openApiSpec,
        string clientName,
        string @namespace,
        out ImmutableArray<Diagnostic> diagnostics,
        CancellationToken cancellationToken) =>
        Run(new TestAdditionalFile($"OpenApiSpecs/{openApiSpec}"),
            clientName, @namespace, out diagnostics, cancellationToken);

    internal static Compilation SetupFromContent(string openApiSpec,
        string clientName,
        string @namespace,
        out ImmutableArray<Diagnostic> diagnostics,
        CancellationToken cancellationToken) =>
        Run(TestAdditionalFile.FromContent(openApiSpec),
            clientName, @namespace, out diagnostics, cancellationToken);

    private static Compilation Run(TestAdditionalFile clientSdkItem,
        string clientName,
        string @namespace,
        out ImmutableArray<Diagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var generator = new ClientSDKGenerator.ClientSdkGenerator();

        var metadata = ImmutableDictionary<string, string>.Empty
            .Add("build_metadata.AdditionalFiles.SourceItemGroup", "ClientSDKGenerator")
            .Add("build_metadata.AdditionalFiles.ClientName", clientName)
            .Add("build_metadata.AdditionalFiles.Namespace", @namespace);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: [clientSdkItem],
            optionsProvider: new OptionsProvider(clientSdkItem, metadata));

        const string assemblyName = nameof(ClientSdkGeneratorTests);
        var compilation = CSharpCompilation.Create(assemblyName,
            options: new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary));

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out diagnostics, cancellationToken);

        foreach (var tree in newCompilation.SyntaxTrees)
        {
            tree.GetDiagnostics().Should().NotContain(diagnostic =>
                diagnostic.Severity == DiagnosticSeverity.Error ||
                diagnostic.Severity == DiagnosticSeverity.Warning, 
                because: $"the syntax should be correct: {tree.GetText(cancellationToken)}");
        }

        return newCompilation;
    }

    private sealed class OptionsProvider(AdditionalText text, ImmutableDictionary<string, string> metadata)
        : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GlobalOptions { get; } =
            new Options(ImmutableDictionary<string, string>.Empty);

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => GlobalOptions;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) =>
            textFile == text ? new Options(metadata) : GlobalOptions;
    }

    private sealed class Options(ImmutableDictionary<string, string> values) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) =>
            values.TryGetValue(key, out value);
    }
}
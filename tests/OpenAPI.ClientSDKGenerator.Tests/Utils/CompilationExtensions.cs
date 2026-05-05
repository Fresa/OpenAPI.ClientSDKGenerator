using System.IO;
using System.Linq;
using System.Threading;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace OpenAPI.ClientSDKGenerator.Tests.Utils;

internal static class CompilationExtensions
{
    internal static void Output(this Compilation compilation,
        string fileName,
        ITestOutputHelper output,
        CancellationToken cancellationToken = default)
    {
        foreach (var tree in compilation.SyntaxTrees.Where(tree => Path.GetFileName(tree.FilePath) == fileName))
        {
            output.WriteLine($"// === {Path.GetFileName(tree.FilePath)} ===");
            output.WriteLine(tree.GetText(cancellationToken).ToString());
        }
    }

    internal static string GetSource(this Compilation compilation,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        compilation.SyntaxTrees
            .Select(tree => Path.GetFileName(tree.FilePath))
            .Should().Contain(fileName);
        return compilation.SyntaxTrees
            .Single(tree => Path.GetFileName(tree.FilePath) == fileName)
            .GetText(cancellationToken).ToString();
    }
}
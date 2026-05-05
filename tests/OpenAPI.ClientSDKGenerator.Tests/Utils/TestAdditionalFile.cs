using System.IO;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace OpenAPI.ClientSDKGenerator.Tests.Utils;

public class TestAdditionalFile : AdditionalText
{
    private readonly string? _content;

    public TestAdditionalFile(string path) => Path = path;

    private TestAdditionalFile(string path, string content)
    {
        Path = path;
        _content = content;
    }

    public static TestAdditionalFile FromContent(string content) =>
        new("openapi.json", content);

    public override SourceText GetText(CancellationToken cancellationToken = default) =>
        _content is not null
            ? SourceText.From(_content)
            : SourceText.From(File.OpenRead(Path));

    public override string Path { get; }
}
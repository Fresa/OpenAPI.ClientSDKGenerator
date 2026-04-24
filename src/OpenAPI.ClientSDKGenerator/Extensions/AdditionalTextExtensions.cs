using System;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;

namespace OpenAPI.ClientSDKGenerator.Extensions;

internal static class AdditionalTextExtensions
{
    internal static bool IsOptionsFile(this AdditionalText text) =>
        text.Path.EndsWith("OpenAPI.WebApiGenerator.json", StringComparison.InvariantCultureIgnoreCase);

    internal static Options LoadOptions(this AdditionalText? text) => 
        text == null ? Options.Default : Options.From(text);

    internal static MemoryStream AsStream(this AdditionalText text)
    {
        var content = text.GetText();
        var stream = new MemoryStream();
        if (content is null)
        {
            return stream;
        }

        using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
        {
            content.Write(writer);    
        }
        
        stream.Position = 0;
        return stream;
    }

}
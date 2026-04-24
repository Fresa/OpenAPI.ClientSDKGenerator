using System;
using Microsoft.CodeAnalysis;

namespace OpenAPI.ClientSDKGenerator.Tests.Utils;

internal static class DiagnosticExtensions
{
    internal static string GetFormattedMessage(this Diagnostic diagnostic) => 
        diagnostic.GetMessage().Replace(" |", Environment.NewLine);
}
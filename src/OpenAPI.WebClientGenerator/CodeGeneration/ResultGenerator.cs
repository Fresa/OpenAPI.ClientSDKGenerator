namespace OpenAPI.WebClientGenerator.CodeGeneration;

public class ResultGenerator(string @namespace)
{
    private const string ClassName = "Result";
    internal SourceCode GenerateClass() => new($"{ClassName}.g.cs",
$$"""
#nullable enable
using Corvus.Json;
using System.Diagnostics.CodeAnalysis;

namespace {{@namespace}};

/// <summary>
/// The result of an operation
/// </summary>
/// <typeparam name="T">The response type</typeparam>
internal sealed class {{ClassName}}<T>
{
    private {{ClassName}}(ValidationContext requestValidationContext)
    {
        ValidationContext = requestValidationContext;
        FailedRequestValidation = true;
        IsSuccessful = false;
        Response = default;
    }

    private {{ClassName}}(T response, ValidationContext responseValidationContext)
    {
        ValidationContext = responseValidationContext;
        Response = response;
        FailedRequestValidation = false;
        IsSuccessful = responseValidationContext.IsValid;
    }

    internal static {{ClassName}}<T> WithInvalidRequest(ValidationContext requestValidationContext) =>
        new(requestValidationContext);
    internal static {{ClassName}}<T> WithResponse(T response, ValidationContext responseValidationContext) =>
        new(response, responseValidationContext);

    /// <summary>
    /// The response or null if <see cref="FailedRequestValidation"/> is true
    /// </summary>
    internal T? Response { get; }

    /// <summary>
    /// Validation context for the request if <see cref="FailedRequestValidation"/> is true, otherwise validation context for the response
    /// </summary>
    internal ValidationContext ValidationContext { get; }
    
    /// <summary>
    /// True if request failed validation.
    /// Note: The request was never sent if it failed validation.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Response))]
    internal bool FailedRequestValidation { get; }
    
    /// <summary>
    /// True if the operation was executed successfully.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Response))]
    internal bool IsSuccessful { get; }
}
#nullable restore
""");
}
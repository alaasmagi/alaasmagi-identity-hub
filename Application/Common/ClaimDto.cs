namespace Application.Common;

/// <summary>
/// Response item describing a runtime claim.
/// </summary>
/// <param name="Type">The claim type.</param>
/// <param name="Value">The claim value.</param>
public sealed record ClaimDto(string Type, string Value);

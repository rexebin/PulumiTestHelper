using System.Collections.Immutable;

namespace PulumiTestHelper.Models;

/// <summary>
/// Represents a resource input.
/// </summary>
public record ResourceInput(string? Identifier, ImmutableDictionary<string, object> Inputs);
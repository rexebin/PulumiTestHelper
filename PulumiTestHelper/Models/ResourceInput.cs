using System.Collections.Immutable;

namespace PulumiTestHelper.Models;

/// <summary>
/// Represents a resource input.
/// </summary>
public class ResourceInput(string? identifier, ImmutableDictionary<string, object> inputs)
{
    internal string? Identifier { get; } = identifier;
    
    internal ImmutableDictionary<string, object> Inputs { get; } = inputs;
}
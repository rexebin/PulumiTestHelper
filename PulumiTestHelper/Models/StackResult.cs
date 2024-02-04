using System.Collections.Immutable;
using Pulumi;

namespace PulumiTestHelper.Models;

/// <summary>
/// Represents a result from building a stack.
/// </summary>
/// <param name="Resources">List of resources from the stack</param>
/// <param name="ResourceInputs">List of resource inputs from the stack</param>
public record StackResult(ImmutableArray<Resource> Resources, List<ResourceInput> ResourceInputs);
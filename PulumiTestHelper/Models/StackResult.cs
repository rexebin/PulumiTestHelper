using System.Collections.Immutable;
using Pulumi;

namespace PulumiTestHelper.Models;

/// <summary>
/// Represents a result from building a stack.
/// </summary>
/// <param name="resources">List of resources from the stack</param>
/// <param name="resourceInputs">List of resource inputs from the stack</param>
public class StackResult(ImmutableArray<Resource> resources, List<ResourceInput> resourceInputs)
{
    /// <summary>
    /// List of resources from the stack.
    /// </summary>
    public ImmutableArray<Resource> Resources { get; } = resources;
    /// <summary>
    /// List of resource inputs from the stack.
    /// </summary>
    public List<ResourceInput> ResourceInputs { get; } = resourceInputs;
}
using Pulumi.Testing;

namespace PulumiTestHelper.Models;

/// <summary>
/// Represents a resource mock function used for mocking resources in Pulumi tests.
/// </summary>
public class ResourceMockFunc(Type resourceType, Func<MockResourceArgs, Dictionary<string, object>> mockFunc)
{
    internal Type ResourceType { get; } = resourceType;
    internal Func<MockResourceArgs, Dictionary<string, object>> MockFunc { get; } = mockFunc;
}
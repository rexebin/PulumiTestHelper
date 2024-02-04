namespace PulumiTestHelper.Models;

/// <summary>
/// Represents a mock for a Pulumi resource.
/// </summary>
public class ResourceMock(Type resourceType, Dictionary<string, object> mocks)
{
    internal Type ResourceType { get; } = resourceType;
    internal Dictionary<string, object> Mocks { get; set; } = mocks;
}
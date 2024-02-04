namespace PulumiTestHelper.Models;

/// <summary>
/// Represents a mock for a Pulumi resource.
/// </summary>
public record ResourceMock(Type ResourceType, Dictionary<string, object> Mocks);
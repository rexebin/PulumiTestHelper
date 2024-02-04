using Pulumi;

namespace PulumiTestHelper.Models;

/// <summary>
/// Represents a mock for a Pulumi stack reference.
/// </summary>
public record StackReferenceMock : ResourceMock
{
    /// <inheritdoc />
    public StackReferenceMock(string stackReferenceName, Dictionary<string, object> mocks) : base(
        typeof(StackReference), mocks)
    {
        StackReferenceName = stackReferenceName;
        Mocks = mocks;
    }
    
    internal string StackReferenceName { get; }
}
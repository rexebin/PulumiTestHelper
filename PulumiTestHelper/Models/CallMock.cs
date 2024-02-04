namespace PulumiTestHelper.Models;

/// <summary>
/// Represents a mock for a function call.
/// </summary>
public class CallMock(Type callType, Dictionary<string, object> mocks)
{
    internal Type CallType { get; } = callType;
    internal Dictionary<string, object> Mocks { get; } = mocks;
}
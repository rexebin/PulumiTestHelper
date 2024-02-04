using Pulumi.Testing;

namespace PulumiTestHelper.Models;

/// <summary>
/// Represents a class that provides a way to define and manage call mock functions.
/// </summary>
public class CallMockFunc(Type callType, Func<MockCallArgs, Dictionary<string, object>> mockFunc)
{
    internal Type CallType { get; } = callType;
    
    internal Func<MockCallArgs, Dictionary<string, object>> MockFunc { get; } = mockFunc;
}
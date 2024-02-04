using Pulumi.Testing;

namespace PulumiTestHelper.Models;

/// <summary>
/// Represents a class that provides a way to define and manage call mock functions.
/// </summary>
public record CallMockFunc(Type CallType, Func<MockCallArgs, Dictionary<string, object>> MockFunc);
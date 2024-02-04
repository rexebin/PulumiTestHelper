namespace PulumiTestHelper.Models;

/// <summary>
/// Represents a mock for a function call.
/// </summary>
public record CallMock(Type CallType, Dictionary<string, object> Mocks);
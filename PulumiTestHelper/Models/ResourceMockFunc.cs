using Pulumi.Testing;

namespace PulumiTestHelper.Models;

/// <summary>
/// Represents a resource mock function used for mocking resources in Pulumi tests.
/// </summary>
public record ResourceMockFunc(Type ResourceType, Func<MockResourceArgs, Dictionary<string, object>> MockFunc);
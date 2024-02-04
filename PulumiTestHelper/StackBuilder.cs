using Pulumi;
using Pulumi.Testing;
using PulumiTestHelper.Internals;
using PulumiTestHelper.Models;

namespace PulumiTestHelper;

/// <summary>
/// Class for building a stack with mocks for testing purposes.
/// </summary>
public class StackBuilder
{
    private readonly List<CallMockFunc> _callMocksFuncs = new();
    private readonly List<ResourceInput> _resourceInputs = new();
    private readonly List<ResourceMockFunc> _resourceMocksFuncs = new();

    private Func<MockResourceArgs, Dictionary<string, object>> _mocksForAllResources =
        _ => new Dictionary<string, object>();

    private List<ResourceMock> ResourceMocks { get; } = new();
    private List<CallMock> CallMocks { get; } = new();

    /// <summary>
    /// Adds mocks for all resources in the stack.
    /// </summary>
    /// <param name="mocks">
    /// A function that takes a <see cref="MockResourceArgs"/> and returns a dictionary of mocks.
    /// </param>
    /// <returns>
    /// The <see cref="StackBuilder"/> instance.
    /// </returns>
    public StackBuilder AddMocksForAllResources(Func<MockResourceArgs, Dictionary<string, object>> mocks)
    {
        _mocksForAllResources = mocks;
        return this;
    }

    /// <summary>
    /// Adds a resource mock to the stack builder.
    /// </summary>
    /// <param name="resourceMock">The resource mock to add.</param>
    /// <returns>The updated stack builder.</returns>
    public StackBuilder AddResourceMock(ResourceMock resourceMock)
    {
        ResourceMocks.Add(resourceMock);
        return this;
    }

    /// <summary>
    /// Adds a list of resource mocks to the stack builder.
    /// </summary>
    /// <param name="resourceMocks">The list of resource mocks to add.</param>
    /// <returns>The modified stack builder.</returns>
    public StackBuilder AddResourceMocks(List<ResourceMock> resourceMocks)
    {
        ResourceMocks.AddRange(resourceMocks);
        return this;
    }

    /// <summary>
    /// Adds a resource mock function to the stack builder.
    /// </summary>
    /// <param name="resourceMockFunc">The resource mock function to add.</param>
    /// <returns>The updated stack builder.</returns>
    public StackBuilder AddResourceMockFunc(ResourceMockFunc resourceMockFunc)
    {
        _resourceMocksFuncs.Add(resourceMockFunc);
        return this;
    }

    /// <summary>
    /// Adds a list of resource mock functions to the stack builder to be used for testing purposes.
    /// </summary>
    /// <param name="resourceMockFuncs">The list of resource mock functions to add.</param>
    /// <returns>The updated stack builder.</returns>
    public StackBuilder AddResourceMockFuncs(List<ResourceMockFunc> resourceMockFuncs)
    {
        _resourceMocksFuncs.AddRange(resourceMockFuncs);
        return this;
    }

    /// <summary>
    /// Adds a CallMock to the StackBuilder.
    /// </summary>
    /// <param name="callMock">The CallMock object to add.</param>
    /// <returns>The updated StackBuilder object.</returns>
    public StackBuilder AddCallMock(CallMock callMock)
    {
        CallMocks.Add(callMock);
        return this;
    }

    /// <summary>
    /// Adds a list of call mocks to the stack builder.
    /// </summary>
    /// <param name="callMocks">The list of call mocks to add.</param>
    /// <returns>The updated stack builder.</returns>
    public StackBuilder AddCallMocks(List<CallMock> callMocks)
    {
        CallMocks.AddRange(callMocks);
        return this;
    }

    /// <summary>
    /// Add a call mock function to the stack builder.
    /// </summary>
    /// <param name="callMockFunc">The call mock function to add.</param>
    /// <returns>The updated stack builder.</returns>
    public StackBuilder AddCallMockFunc(CallMockFunc callMockFunc)
    {
        _callMocksFuncs.Add(callMockFunc);
        return this;
    }

    /// <summary>
    /// Adds a stack reference mock to the stack builder.
    /// </summary>
    /// <param name="stackReferenceMock">The stack reference mock to add.</param>
    /// <returns>The modified stack builder.</returns>
    public StackBuilder AddStackReferenceMock(StackReferenceMock stackReferenceMock)
    {
        ResourceMocks.Add(stackReferenceMock);
        return this;
    }

    /// <summary>
    /// Asynchronously builds a Pulumi stack using the provided stack type and test options.
    /// </summary>
    /// <typeparam name="T">The type of the Pulumi stack to build.</typeparam>
    /// <param name="testOptions">Optional test options.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task result is a <see cref="StackResult"/>.</returns>
    public async Task<StackResult> BuildStackAsync<T>(TestOptions? testOptions = null) where T : Stack, new()
    {
        var resources = await Deployment.TestAsync<T>(
            new Mocks(ResourceMocks, CallMocks, _resourceMocksFuncs, _callMocksFuncs, _resourceInputs,
                _mocksForAllResources),
            testOptions ?? new TestOptions {IsPreview = false});
        return new StackResult(resources, _resourceInputs);
    }

    /// <summary>
    /// Builds a stack with mocks for testing purposes.
    /// </summary>
    /// <typeparam name="T">The type of the stack.</typeparam>
    /// <param name="serviceProvider">The service provider used for creating the stack.</param>
    /// <param name="testOptions">The test options.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task result contains a <see cref="StackResult"/> instance.</returns>
    public async Task<StackResult> BuildStackAsync<T>(IServiceProvider serviceProvider,
        TestOptions? testOptions = null) where T : Stack, new()
    {
        var resources = await Deployment.TestWithServiceProviderAsync<T>(
            new Mocks(ResourceMocks, CallMocks, _resourceMocksFuncs, _callMocksFuncs, _resourceInputs,
                _mocksForAllResources), serviceProvider,
            testOptions ?? new TestOptions {IsPreview = false});
        return new StackResult(resources, _resourceInputs);
    }
}
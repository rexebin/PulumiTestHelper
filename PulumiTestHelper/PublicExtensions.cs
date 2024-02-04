using System.Collections.Immutable;
using Pulumi;
using PulumiTestHelper.Models;

namespace PulumiTestHelper;

/// <summary>
/// Provides extension methods for public usage.
/// </summary>
public static class PublicExtensions
{
    /// <summary>
    /// Gets the value asynchronously from the output.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="output">The output to get the value from.</param>
    /// <returns>A task representing the asynchronous operation that returns the value.</returns>
    public static Task<T> GetValueAsync<T>(this Output<T> output)
    {
        var tcs = new TaskCompletionSource<T>();
        output.Apply(v =>
        {
            tcs.SetResult(v);
            return v;
        });
        return tcs.Task;
    }

    /// <summary>
    /// Retrieves the value of an Output asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="output">The Output to get the value from.</param>
    /// <returns>The value of the Output.</returns>
    public static T GetValue<T>(this Output<T> output)
    {
        return GetValueAsync(output).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Determines if the given resource has the specified name.
    /// </summary>
    /// <param name="resource">The resource to check.</param>
    /// <param name="name">The name to search for.</param>
    /// <returns>True if the resource has the specified name; otherwise, false.</returns>
    public static bool HasName(this Resource resource, string name)
    {
        return resource.Urn.GetValue().Contains(name);
    }

    /// <summary>
    /// Retrieves the inputs for a specific resource.
    /// </summary>
    /// <param name="resourceInputs">The collection of resource inputs.</param>
    /// <param name="name">The name of the resource.</param>
    /// <returns>An immutable dictionary containing the inputs for the specified resource.</returns>
    public static ImmutableDictionary<string, object> GetInputs(this IEnumerable<ResourceInput> resourceInputs,
        string name)
    {
        var inputs = resourceInputs.SingleOrDefault(x => name == x.Identifier)?.Inputs;
        if (inputs == null) throw new Exception($"Resource {name} is not found.");

        return inputs;
    }
}
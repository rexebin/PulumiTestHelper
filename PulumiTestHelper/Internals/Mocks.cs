using System.Collections.Immutable;
using Pulumi.Testing;
using PulumiTestHelper.Models;

namespace PulumiTestHelper.Internals;

internal class Mocks(
    IEnumerable<ResourceMock> resourceMocks,
    IEnumerable<CallMock> callMocks,
    IEnumerable<ResourceMockFunc> resourceMocksFuncs,
    IEnumerable<CallMockFunc> callMocksFuncs,
    ICollection<ResourceInput> resourceInputs,
    Func<MockResourceArgs, Dictionary<string, object>> mocksForAllResources)
    : IMocks
{
    public Task<(string? id, object state)> NewResourceAsync(MockResourceArgs args)
    {
        var outputs = ImmutableDictionary.CreateBuilder<string, object>();

        if (args.Type == "pulumi:pulumi:StackReference")
        {
            AddStackReferenceMocks(args, outputs);
            outputs.AddRange(args.Inputs);
        }
        else
        {
            var inputs = new Dictionary<string, object>(args.Inputs);
            var mocks = GetResourceMocks(args).DeepMergeInto(mocksForAllResources(args));
            inputs = mocks.DeepMergeInto(inputs);
            outputs.AddRange(inputs);

            var id = $"{args.Name}_id";
            args.Id = string.IsNullOrWhiteSpace(args.Id) ? id : args.Id;
            outputs.Add("name", outputs.GetValueOrDefault("name") ?? $"{args.Name}_name");
        }

        resourceInputs.Add(new ResourceInput(args.Name, outputs.ToImmutable()));

        return Task.FromResult<(string? id, object state)>((args.Id, outputs));
    }

    public Task<object> CallAsync(MockCallArgs args)
    {
        var outputs = ImmutableDictionary.CreateBuilder<string, object>();

        var mocks = GetCallMocks(args);
        outputs.AddRange(mocks);

        resourceInputs.Add(new ResourceInput(args.Token, outputs.ToImmutable()));

        return Task.FromResult((object) outputs);
    }

    private Dictionary<string, object> GetResourceMocks(MockResourceArgs args)
    {
        var mocks = resourceMocks.Where(x =>
            x.ResourceType.GetResourceCustomAttributeTypes().Contains(args.Type)).Select(x => x.Mocks);

        var mocksFromFunc = resourceMocksFuncs
            .Where(x => x.ResourceType.GetResourceCustomAttributeTypes().Contains(args.Type))
            .Select(x => x.MockFunc(args));

        var mergeMerges = mocks.DeepMergeMany();
        var mocksFromFuncMerge = mocksFromFunc.DeepMergeMany();
        return mergeMerges.DeepMergeInto(mocksFromFuncMerge);
    }

    private void AddStackReferenceMocks(MockResourceArgs args, ImmutableDictionary<string, object>.Builder outputs)
    {
        var mocks = resourceMocks
            .OfType<StackReferenceMock>()
            .Where(x => x.StackReferenceName == args.Name)
            .Select(x => x.Mocks)
            .DeepMergeMany();

        outputs.Add("secretOutputNames", new List<string>());
        outputs.Add("outputs", mocks);
    }

    private Dictionary<string, object> GetCallMocks(MockCallArgs args)
    {
        var mocks = callMocks.Where(x => x.CallType.IsCallMockType(args.Token)).Select(x => x.Mocks).DeepMergeMany();

        var mocksFromFunc = callMocksFuncs
            .Where(x => x.CallType.IsCallMockType(args.Token))
            .Select(x => x.MockFunc(args)).DeepMergeMany();

        return mocks.DeepMergeInto(mocksFromFunc);
    }
}
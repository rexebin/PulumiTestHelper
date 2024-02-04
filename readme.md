
# Pulumi Test Helper

This is a helper package for testing Pulumi programs.

The goal of this package is to make testing feel like first class citizen in Pulumi.

# Motivation

The build in testing support in Pulumi has the following pain points:
1. to add a mock, we have to figure out the string representation of the resource identifiers
2. the mock replaces the property as a whole, potentially removing child properties that are needed for the test
3. mocking stack reference is not straightforward
4. a lot of setup code is needed to create a mock
5. only outputs of resources can be tested 

This package aims to solve these problems by providing a simple and easy to use API for mocking resources, calls and stack references.

It also provide a way to test the raw inputs of resources.

# Usage

## Build the stack

Two build methods are provided: 
```c# 
public async Task<StackResult> BuildStackAsync<T>(TestOptions? testOptions = null) where T : Stack, new()
    
public async Task<StackResult> BuildStackAsync<T>(IServiceProvider serviceProvider,
        TestOptions? testOptions = null) where T : Stack, new()
```
Example:
```c#
var result = await new StackBuilder().BuildStackAsync<MyStack>();

var resources = result.Resources;
var resourceInputs = result.ResourceInputs;
```

```c# 
var result = await new StackBuilder().BuildStackAsync<MyStack>(serviceProvider);

var resources = result.Resources;
var resourceInputs = result.ResourceInputs;
```

## Add Mocks for all resources
`AddMocksForAllResources` is provided to add mocks for all resources in the stack.

It takes `MockResourceArgs` as input and returns a dictionary of the resource properties, so that you can provide mocks based on existing resource inputs.

```c#
```c#
public StackBuilder AddMocksForAllResources(Func<MockResourceArgs, Dictionary<string, object>> mocks)
```
For example, below tests shows that `arn` property of all resources is mocked with the resource name appended with `_arn`.

```c#
[Fact]
public async Task Should_Mock_All_Resource_Properties_With_Given_Rule()
{
    var result = await _baseStackBuilder.BuildStackAsync<AwsStack>();

    var repository = result.Resources.OfType<Repository>().Single();
    repository.Arn.GetValue().Should().BeNull();

    result = await _baseStackBuilder.AddMocksForAllResources(args => new Dictionary<string, object>
    {
        {"arn", $"{args.Name}_arn"}
    }).BuildStackAsync<AwsStack>();

    var resources = result.Resources;

    repository = resources.OfType<Repository>().Single();
    repository.Arn.GetValue().Should().Be("my-repository_arn");

    var bucket = resources.OfType<Bucket>().Single();
    bucket.Arn.GetValue().Should().Be("my-bucket_arn");
}

```

## Mock resources
Four methods are provided to add mocks for resources:
```c#
AddResourceMock(ResourceMock resourceMock)
AddResourceMocks(List<ResourceMock> resourceMocks)
AddResourceMockFunc(ResourceMockFunc resourceMockFunc)
AddResourceMockFuncs(List<ResourceMockFunc> resourceMockFuncs)
```

AddResourceMockFunc takes the pulumi `MockResourceArgs` as input and returns a dictionary of the resource properties, so that you can provide mocks based on existing resource inputs. 
```c#

Example:
```c#
var result = await new StackBuilder()
        .AddResourceMock(new ResourceMock(typeof(Image),
            new Dictionary<string, object>
            {
                {"imageUri", imageUri}
            }))
        .BuildStackAsync<MyStack>();

image = result.Resources.OfType<Image>().Single(x => x.HasName("my-image"));
image.ImageUri.GetValue().Should().Be(imageUri);
```

```c#
var result = await new StackBuilder()
    .AddResourceMockFunc(
        new ResourceMockFunc(typeof(Image),
                    args => new Dictionary<string, object>
                    {
                        {"imageUri", $"{args.Name}-{imageUri}"}
                    })
        )
        .BuildStackAsync<MyStack>();
```

## Mock calls
Three methods are provided to add mocks for calls:
```c#
AddCallMock(CallMock callMock)
AddCallMocks(List<CallMock> callMocks)
AddCallMockFunc(CallMockFunc callMockFunc)
```
Examples:

Below code makes a call to `GetRepository` and assigns it to `RepositoryUrl` property of the stack as stack output.
```c#
[Output] public Output<string> RepositoryUrl { get; set; }
public MyStack()
{
    var invoke = GetRepository.Invoke(new GetRepositoryInvokeArgs
    {
        Name = "my-remote-repository"
    });

    RepositoryUrl = invoke.Apply(x => x.RepositoryUrl);
}
```
To test the above:
```c#
var result = await new StackBuilder()
    .AddCallMock(new CallMock(typeof(GetRepository),
        new Dictionary<string, object>
        {
            {"repositoryUrl", mock}
        })).BuildStackAsync<MyStack>();
var stack = result.Resources.OfType<MyStack>().Single();
stack.RepositoryUrl.GetValue().Should().Be(mock);
```

## Mock stack references

To mock stack references, use `AddStackReferenceMock` method.

Example:

Below code uses a stack reference to get the `hosted-zone-id` output from another stack.
```c#
public class CoreStackReference
{
    public readonly Output<string> HostedZoneId;

    public CoreStackReference()
    {
        var coreStackReference = new StackReference("core");
        HostedZoneId = coreStackReference.RequireOutput("hosted-zone-id").Apply(x => x.ToString())!;
    }
}
```

```c#
var coreStackReference = new CoreStackReference();

_ = new Bucket("my-bucket", new BucketArgs
{
    BucketName = "my-bucket",
    HostedZoneId = coreStackReference.HostedZoneId
});
```
To test the above:
```c#
[Fact]
public async Task Should_Add_StackReference_Mock()
{
    var noStackReferenceMock = () => new StackBuilder().BuildStackAsync<AwsStack>();
    await noStackReferenceMock.Should().ThrowAsync<Exception>()
        .WithMessage("*Required output 'hosted-zone-id' does not exist on stack 'core'*");

    var result = await new StackBuilder()
        .AddStackReferenceMock(new StackReferenceMock("core", new Dictionary<string, object>
        {
            {"hosted-zone-id", "hosted-zone-id-mock"}
        })).BuildStackAsync<AwsStack>();
    var bucket = result.Resources.OfType<Bucket>().Single();
    bucket.HostedZoneId.GetValue().Should().Be("hosted-zone-id-mock");
}
```
## Test raw inputs
The resource list from Pulumi `Deployment.TestAsync` (which is used by `BuildStackAsync`) containers list of resources specified in the stack. 

However, the properties of the resources only includes the Pulumi outputs of the resources, many of the raw properties are absent.

In case you want to protect against changes in the inputs of the resources, you can use `ResourceInputs` property of the `StackResult` to test the inputs of the resources.

For example, `Awsx.Erc.Image` resource only have `imageUri` output, it does not output other properties like `Platform`, `Context` etc. 
```c#:
var repository = new Repository("my-repository", new RepositoryArgs
        {
            ImageScanningConfiguration = new RepositoryImageScanningConfigurationArgs
            {
                ScanOnPush = false
            },
            ForceDelete = false,
            ImageTagMutability = "MUTABLE"
        });
        
new Image("my-image", new ImageArgs
{
    Platform = "linux/amd64",
    Context = "./",
    RepositoryUrl = repository.RepositoryUrl
});
```

To test the inputs of the resource `Image`:
```c#
var result = await _baseStackBuilder.AddResourceMock(new ResourceMock(typeof(Repository),
    new Dictionary<string, object>
    {
        {"repositoryUrl", "my-repository_name"}
    })).BuildStackAsync<AwsStack>();

var inputs = result.ResourceInputs.GetInputs("my-image");
var platform = inputs.GetValueOrDefault("platform");
platform.Should().Be("linux/amd64");
        
var context = inputs.GetValueOrDefault("context");
context.Should().Be("./");

var repositoryUrl = inputs.GetValueOrDefault("repositoryUrl");
repositoryUrl.Should().Be("my-repository_name");
```

# Contribution

Please feel free to contribute to this project. PRs are welcome.

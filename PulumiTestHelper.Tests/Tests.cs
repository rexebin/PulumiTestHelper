using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;
using Pulumi.Aws.Ecr;
using Pulumi.Aws.S3;
using Pulumi.Awsx.Ecr;
using Pulumi.AzureNative.Network;
using Pulumi.AzureNative.Storage;
using PulumiTestHelper.Models;
using Repository = Pulumi.Aws.Ecr.Repository;

namespace PulumiTestHelper.Tests;

public class Tests
{
    private readonly StackBuilder _baseStackBuilder = new StackBuilder()
        .AddStackReferenceMock(new StackReferenceMock("core", new Dictionary<string, object>
        {
            {"hosted-zone-id", "hosted-zone-id-mock"}
        }));

    [Fact]
    public async Task Should_Add_Name_Suffix()
    {
        var result = await _baseStackBuilder.BuildStackAsync<AwsStack>();
        var repository = result.Resources.OfType<Repository>().Single();
        repository.Name.GetValue().Should().Be("my-repository_name");
    }

    [Fact]
    public async Task Should_Mock_Id()
    {
        var result = await _baseStackBuilder.BuildStackAsync<AwsStack>();
        var repository = result.Resources.OfType<Repository>().Single();
        repository.Id.GetValue().Should().Be("my-repository_id");
    }

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

    [Theory]
    [InlineData("my-image-uri")]
    [InlineData("my-image-uri2")]
    public async Task Should_Add_ResourceMock(string imageUri)
    {
        var result = await _baseStackBuilder.BuildStackAsync<AwsStack>();
        var image = result.Resources.OfType<Image>().Single(x => x.HasName("my-image"));
        image.ImageUri.GetValue().Should().Be(null);

        result = await _baseStackBuilder.AddResourceMock(new ResourceMock(typeof(Image),
            new Dictionary<string, object>
            {
                {"imageUri", imageUri}
            })).BuildStackAsync<AwsStack>();

        image = result.Resources.OfType<Image>().Single(x => x.HasName("my-image"));
        image.ImageUri.GetValue().Should().Be(imageUri);
    }

    [Theory]
    [InlineData("my-image-uri")]
    [InlineData("my-image-uri2")]
    public async Task Should_Add_ResourceMockFunc(string imageUri)
    {
        var result = await _baseStackBuilder.BuildStackAsync<AwsStack>();
        var image = result.Resources.OfType<Image>().Single(x => x.HasName("my-image"));
        image.ImageUri.GetValue().Should().Be(null);

        result = await _baseStackBuilder.AddResourceMockFunc(new ResourceMockFunc(typeof(Image),
            args => new Dictionary<string, object>
            {
                {"imageUri", $"{args.Name}-{imageUri}"}
            })).BuildStackAsync<AwsStack>();

        image = result.Resources.OfType<Image>().Single(x => x.HasName("my-image"));
        image.ImageUri.GetValue().Should().Be($"my-image-{imageUri}");
    }

    [Fact]
    public async Task Should_Add_ResourceMocks()
    {
        var result = await _baseStackBuilder.AddResourceMocks([
            new ResourceMock(typeof(Image),
                new Dictionary<string, object>
                {
                    {"imageUri", "my-image-uri"}
                }),
            new ResourceMock(typeof(Repository), new Dictionary<string, object>
            {
                {"repositoryUrl", "my-repository-url"}
            })
        ]).BuildStackAsync<AwsStack>();

        var image = result.Resources.OfType<Image>().Single(x => x.HasName("my-image"));
        image.ImageUri.GetValue().Should().Be("my-image-uri");

        var repository = result.Resources.OfType<Repository>().Single();
        repository.RepositoryUrl.GetValue().Should().Be("my-repository-url");
    }

    [Fact]
    public async Task Should_Add_ResourceMock_Funcs()
    {
        var result = await _baseStackBuilder.AddResourceMockFuncs([
            new ResourceMockFunc(typeof(Image),
                args => new Dictionary<string, object>
                {
                    {"imageUri", $"{args.Name}-image-uri-mock"}
                }),
            new ResourceMockFunc(typeof(Repository), args => new Dictionary<string, object>
            {
                {"repositoryUrl", $"{args.Name}-url-mock"}
            })
        ]).BuildStackAsync<AwsStack>();

        var image = result.Resources.OfType<Image>().Single(x => x.HasName("my-image"));
        image.ImageUri.GetValue().Should().Be("my-image-image-uri-mock");
        
        var repository = result.Resources.OfType<Repository>().Single();
        repository.RepositoryUrl.GetValue().Should().Be("my-repository-url-mock");
    }

    [Fact]
    public async Task Should_Return_Resources_Raw_Inputs()
    {
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
    }

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

    [Theory]
    [InlineData("get-repository-url-mock")]
    [InlineData("get-repository-url-mock2")]
    public async Task Should_Add_CallMock(string mock)
    {
        var result = await _baseStackBuilder.BuildStackAsync<AwsStack>();
        var stack = result.Resources.OfType<AwsStack>().Single();
        stack.RepositoryUrl.GetValue().Should().BeNull();

        result = await _baseStackBuilder.AddCallMock(new CallMock(typeof(GetRepository),
            new Dictionary<string, object>
            {
                {"repositoryUrl", mock}
            })).BuildStackAsync<AwsStack>();
        stack = result.Resources.OfType<AwsStack>().Single();
        stack.RepositoryUrl.GetValue().Should().Be(mock);
    }

    [Fact]
    public async Task Should_Add_CallMockFunc()
    {
        var result = await _baseStackBuilder.BuildStackAsync<AwsStack>();
        var stack = result.Resources.OfType<AwsStack>().Single();
        stack.RepositoryUrl.GetValue().Should().BeNull();

        result = await _baseStackBuilder.AddCallMockFunc(new CallMockFunc(typeof(GetRepository),
            args => new Dictionary<string, object>
            {
                {"repositoryUrl", $"{args.Args.GetValueOrDefault("name")}-repository-url"}
            })).BuildStackAsync<AwsStack>();
        stack = result.Resources.OfType<AwsStack>().Single();
        stack.RepositoryUrl.GetValue().Should().Be("my-remote-repository-repository-url");
    }

    [Fact]
    public async Task Should_Add_CallMocks()
    {
        var result = await _baseStackBuilder.BuildStackAsync<AwsStack>();
        var stack = result.Resources.OfType<AwsStack>().Single();
        stack.RepositoryUrl.GetValue().Should().BeNull();

        result = await _baseStackBuilder.AddCallMocks([
            new CallMock(typeof(GetRepository),
                new Dictionary<string, object>
                {
                    {"repositoryUrl", "my-repository-url"}
                }),
            new CallMock(typeof(GetRepository),
                new Dictionary<string, object>
                {
                    {"arn", "my-repository-arn"}
                })
        ]).BuildStackAsync<AwsStack>();

        stack = result.Resources.OfType<AwsStack>().Single();
        stack.RepositoryUrl.GetValue().Should().Be("my-repository-url");
        stack.RepositoryArn.GetValue().Should().Be("my-repository-arn");
    }

    [Theory]
    [InlineData("my-type-mock")]
    [InlineData("my-type-mock2")]
    public async Task AddResourceMock_Should_WorkForAzure(string mock)
    {
        var result = await new StackBuilder().BuildStackAsync<AzureStack>();
        var storageAccount = result.Resources.OfType<StorageAccount>().Single();
        storageAccount.Type.GetValue().Should().BeNull();

        result = await new StackBuilder().AddResourceMock(new ResourceMock(typeof(StorageAccount),
            new Dictionary<string, object>
            {
                {"type", mock}
            })).BuildStackAsync<AzureStack>();
        storageAccount = result.Resources.OfType<StorageAccount>().Single();
        storageAccount.Type.GetValue().Should().Be(mock);
    }

    [Theory]
    [InlineData("10.1.1.1")]
    [InlineData("10.0.0.0")]
    public async Task CallMocks_Should_WorkForAzure(string address)
    {
        var result = await new StackBuilder().BuildStackAsync<AzureStack>();
        var stack = result.Resources.OfType<AzureStack>().Single();
        (await stack.VirtualNetworkFirstAddressPrefix.GetValueAsync()).Should().BeNull();

        result = await new StackBuilder().AddCallMock(new CallMock(typeof(GetVirtualNetwork),
            new Dictionary<string, object>
            {
                {
                    "addressSpace", new Dictionary<string, List<string>>
                    {
                        {"addressPrefixes", new List<string> {address}}
                    }
                }
            })).BuildStackAsync<AzureStack>();
        stack = result.Resources.OfType<AzureStack>().Single();
        stack.VirtualNetworkFirstAddressPrefix.GetValue().Should().Be(address);
    }

    [Fact]
    public async Task Mock_Should_Not_Override_Existing_Inputs()
    {
        var result = await _baseStackBuilder.BuildStackAsync<AwsStack>();
        var repository = result.Resources.OfType<Repository>().Single();
        repository.ImageTagMutability.GetValue().Should().Be("MUTABLE");

        result = await _baseStackBuilder.AddResourceMock(new ResourceMock(typeof(Repository),
            new Dictionary<string, object>
            {
                {"imageTagMutability", "NOT-MUTABLE"}
            })).BuildStackAsync<AwsStack>();
        repository = result.Resources.OfType<Repository>().Single();
        repository.ImageTagMutability.GetValue().Should().Be("MUTABLE");
    }

    [Fact]
    public void Custom()
    {
        var bucketCallIdentifier = "aws:s3/getBucket:getBucket";
        var attributes = typeof(GetBucket).GetCustomAttributes(true);
        var types = typeof(GetBucket).Name;
        var identifier = $"{types}:{types}".ToLower();
        identifier.Should().Be("getbucket:getbucket");

        bucketCallIdentifier.ToLower().Should().Contain(identifier);
    }
}
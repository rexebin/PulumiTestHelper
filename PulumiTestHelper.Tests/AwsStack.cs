using Pulumi;
using Pulumi.Aws.Ecr;
using Pulumi.Aws.Ecr.Inputs;
using Pulumi.Aws.S3;
using Pulumi.Awsx.Ecr;
using Repository = Pulumi.Aws.Ecr.Repository;
using RepositoryArgs = Pulumi.Aws.Ecr.RepositoryArgs;

namespace PulumiTestHelper.Tests;

public class AwsStack : Stack
{
    public AwsStack()
    {
        var repository = new Repository("my-repository", new RepositoryArgs
        {
            ImageScanningConfiguration = new RepositoryImageScanningConfigurationArgs
            {
                ScanOnPush = false
            },
            ForceDelete = false,
            ImageTagMutability = "MUTABLE"
        });

        _ = new Image("my-image", new ImageArgs
        {
            Platform = "linux/amd64",
            Context = "./",
            RepositoryUrl = repository.RepositoryUrl
        });

        var invoke = GetRepository.Invoke(new GetRepositoryInvokeArgs
        {
            Name = "my-remote-repository"
        });

        RepositoryUrl = invoke.Apply(x => x.RepositoryUrl);

        RepositoryArn = invoke.Apply(x => x.Arn);

        var coreStackReference = new CoreStackReference();

        _ = new Bucket("my-bucket", new BucketArgs
        {
            BucketName = "my-bucket",
            HostedZoneId = coreStackReference.HostedZoneId
        });
    }

    [Output] public Output<string> RepositoryArn { get; set; }

    [Output] public Output<string> RepositoryUrl { get; set; }
}
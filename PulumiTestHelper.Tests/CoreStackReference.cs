using Pulumi;

namespace PulumiTestHelper.Tests;

public class CoreStackReference
{
    public readonly Output<string> HostedZoneId;

    public CoreStackReference()
    {
        var coreStackReference = new StackReference("core");
        HostedZoneId = coreStackReference.RequireOutput("hosted-zone-id").Apply(x => x.ToString())!;
    }
}
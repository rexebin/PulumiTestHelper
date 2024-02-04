using Pulumi;
using Pulumi.Azure.Core;
using Pulumi.AzureNative.Network;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;

namespace PulumiTestHelper.Tests;

public class AzureStack : Stack
{
    public AzureStack()
    {
        var resourceGroup = new ResourceGroup("my-resource-group", new ResourceGroupArgs
        {
            Name = "my-resource-group"
        });

        _ = new StorageAccount("storageAccount", new StorageAccountArgs
        {
            AccountName = "sto4445",
            EnableHttpsTrafficOnly = false,
            EnableNfsV3 = true,
            IsHnsEnabled = true,
            Kind = "BlockBlobStorage",
            Location = "eastus",
            NetworkRuleSet = new NetworkRuleSetArgs
            {
                Bypass = "AzureServices",
                DefaultAction = DefaultAction.Allow,
                IpRules = new InputList<IPRuleArgs>(),
                VirtualNetworkRules = new[]
                {
                    new VirtualNetworkRuleArgs
                    {
                        VirtualNetworkResourceId =
                            "/subscriptions/{subscription-id}/resourceGroups/res9101/providers/Microsoft.Network/virtualNetworks/net123/subnets/subnet12"
                    }
                }
            },
            ResourceGroupName = resourceGroup.Name,
            Sku = new SkuArgs
            {
                Name = "Premium_LRS"
            }
        });

        VirtualNetworkFirstAddressPrefix = GetVirtualNetwork.Invoke(new GetVirtualNetworkInvokeArgs
        {
            ResourceGroupName = "my-vnet-rg",
            VirtualNetworkName = "my-vnet-name"
        }).Apply(x => x.AddressSpace?.AddressPrefixes.First());
    }

    [Output] public Output<string?> VirtualNetworkFirstAddressPrefix { get; set; }
}
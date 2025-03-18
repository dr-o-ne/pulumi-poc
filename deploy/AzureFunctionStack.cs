using Pulumi;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using Pulumi.AzureNative.Resources;
using Pulumi.Command.Local;
using Time = Pulumiverse.Time;
using System.Collections.Generic;

internal sealed class AzureFunctionStack : Stack
{
    public AzureFunctionStack()
    {
        const string FunctionPublishDir = "../src/TestAzureFunction/bin/Release/net8.0/publish";

        var resourceGroup = new ResourceGroup("resourceGroup");

        var storageAccount = new StorageAccount("sa", new StorageAccountArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Sku = new SkuArgs
            {
                Name = SkuName.Standard_LRS
            },
            Kind = Pulumi.AzureNative.Storage.Kind.StorageV2
        });

        var storageAccountKeys = ListStorageAccountKeys.Invoke(new ListStorageAccountKeysInvokeArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = storageAccount.Name
        });

        var primaryStorageKey = storageAccountKeys.Apply(accountKeys =>
        {
            var firstKey = accountKeys.Keys[0].Value;
            return Output.CreateSecret(firstKey);
        });

        var appServicePlan = new AppServicePlan("funcPlan", new AppServicePlanArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Kind = "FunctionApp",
            Sku = new SkuDescriptionArgs { Tier = "Dynamic", Name = "Y1" }
        });

        var functionApp = new WebApp("myfunctionapp", new WebAppArgs
        {
            ResourceGroupName = resourceGroup.Name,
            ServerFarmId = appServicePlan.Id,
            Kind = "FunctionApp",
            SiteConfig = new SiteConfigArgs
            {
                AppSettings = new List<NameValuePairArgs>
                {
                    new() { Name = "AzureWebJobsStorage", Value = storageAccount.PrimaryEndpoints.Apply(ep => ep.Blob) },
                    new() { Name = "FUNCTIONS_WORKER_RUNTIME", Value = "dotnet-isolated" },
                }
            }
        });

        var wait30Seconds = new Time.Sleep("wait30Seconds", new()
        {
            CreateDuration = "30s",
        }, new CustomResourceOptions
        {
            DependsOn =
            {
                functionApp
            },
        });

        var publishFunction = functionApp.Name.Apply(name => new Command("publish-function", new CommandArgs
        {
            Create = $"func azure functionapp publish {name} --dotnet-isolated",
            Dir = FunctionPublishDir,
            Environment = new InputMap<string>
            {
                { "AZURE_FUNCTIONAPP_NAME", name },
                { "RESOURCE_GROUP", resourceGroup.Name }
            },
        }, new CustomResourceOptions
        {
            DependsOn =
            {
                wait30Seconds
            },
        }
        ));
        
        Endpoint = Output.Format($"https://{functionApp.DefaultHostName}");
    }

    [Output] public Output<string> Endpoint { get; set; }
}

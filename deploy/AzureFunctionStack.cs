using Pulumi;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using Pulumi.AzureNative.Resources;
using Pulumi.Command.Local;
using Time = Pulumiverse.Time;
using System.Collections.Generic;
using System;
using System.IO;

internal sealed class AzureFunctionStack : Stack
{
    public AzureFunctionStack()
    {
        var stackName = Pulumi.Deployment.Instance.StackName;
        var functionDir = GetFunctionDir(stackName);

        var resourceGroup = new ResourceGroup($"rg{stackName}".ToLower());

        var storageAccount = new StorageAccount($"sa{stackName}".ToLower(), new StorageAccountArgs
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

        var appServicePlan = new AppServicePlan($"plan{stackName}".ToLower(), new AppServicePlanArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Kind = "FunctionApp",
            Sku = new SkuDescriptionArgs { Tier = "Dynamic", Name = "Y1" }
        });

        var functionApp = new WebApp($"app{stackName}".ToLower(), new WebAppArgs
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
            Dir = functionDir,
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

        StackName = Output.Create(stackName);
        ResourceGroup = resourceGroup.Name;
        Endpoint = Output.Format($"https://{functionApp.DefaultHostName}");
    }

    [Output]
    public Output<string> StackName { get; set; }

    [Output]
    public Output<string> ResourceGroup { get; set; }

    [Output]
    public Output<string> Endpoint { get; set; }

    private static string GetFunctionDir(string stackName)
    {
        var dir = stackName switch
        {
            "functionA" => "../src/FunctionA/bin/Release/net8.0/publish",
            "functionB" => "../src/FunctionB/bin/Release/net8.0/publish",
            _ => throw new ArgumentException(nameof(stackName)),
        };

        if (!Directory.Exists(dir))
            throw new ArgumentException(nameof(stackName));

        return dir;
    }
}


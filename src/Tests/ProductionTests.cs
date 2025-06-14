using System;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace Devlooped;

public class ProductionDocTests : DocumentRepositoryTests
{
    static readonly CloudStorageAccount storage = CloudStorageAccount.Parse(new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddUserSecrets<ProductionDocTests>()
        .Build()["Production"] ?? throw new InvalidOperationException("Missing 'Production' configuration for CosmosDB Table Storage."));

    public ProductionDocTests() : base(storage) { }
}

public class ProductionRepoTests(ITestOutputHelper output) : RepositoryTests(output, storage)
{
    static readonly CloudStorageAccount storage = CloudStorageAccount.Parse(new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .AddUserSecrets<ProductionDocTests>()
        .Build()["Production"] ?? throw new InvalidOperationException("Missing 'Production' configuration for CosmosDB Table Storage."));

    public override void Dispose()
    {
        base.Dispose();
        var client = storage.CreateTableServiceClient();
        foreach (var table in client.Query())
        {
            client.DeleteTable(table.Name);
        }
    }
}

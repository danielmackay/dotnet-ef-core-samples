using System.Collections.ObjectModel;
using System.Net;
using Common;
using Microsoft.Azure.Cosmos;

namespace CosmosDb;

/// <summary>
/// Probes the Cosmos endpoint for features the Linux (vNext) emulator doesn't support yet, so the sample can
/// skip just those checks with a clear message instead of failing. Against Azure Cosmos DB every probe passes.
/// </summary>
public static class EmulatorCapabilities
{
    /// <summary>
    /// Creates a throwaway container whose composite index includes an array path (/items/[]/total).
    /// Azure Cosmos DB accepts it; the emulator answers 400 "The indexing policy is invalid".
    /// </summary>
    public static async Task<bool> SupportsArrayCompositeIndexAsync()
    {
        using var client = new CosmosClient(DbConnectionFactory.CreateCosmos(), new CosmosClientOptions
        {
            ConnectionMode = ConnectionMode.Gateway,
            LimitToEndpoint = true,
        });
        Database database = await client.CreateDatabaseIfNotExistsAsync(ApplicationDbContext.DatabaseName);

        var properties = new ContainerProperties("capability-probe", "/pk");
        properties.IndexingPolicy.CompositeIndexes.Add(new Collection<CompositePath>
        {
            new() { Path = "/pk", Order = CompositePathSortOrder.Ascending },
            new() { Path = "/items/[]/total", Order = CompositePathSortOrder.Descending },
        });

        try
        {
            Container probe = await database.CreateContainerIfNotExistsAsync(properties);
            await probe.DeleteContainerAsync();
            return true;
        }
        catch (CosmosException ex) when (IsIndexingPolicyInvalid(ex))
        {
            return false;
        }
    }

    // Only the emulator's "indexing policy is invalid" answer means "unsupported". Any other error
    // (another 400, a timeout, a connection failure) propagates and fails the sample.
    private static bool IsIndexingPolicyInvalid(CosmosException ex) =>
        ex.StatusCode == HttpStatusCode.BadRequest
        && ex.Message.Contains("The indexing policy is invalid", StringComparison.Ordinal);
}

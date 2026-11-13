using Microsoft.Data.SqlClient;

namespace Common;

public static class DbConnectionFactory
{
    public static string Create(string databaseName)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = "127.0.0.1,1700",
            InitialCatalog = databaseName,
            UserID = "sa",
            Password = "yourStrong(!)Password",
            TrustServerCertificate = true,
            MultipleActiveResultSets = true
        };

        Console.WriteLine($"Generated connection string: {builder.ConnectionString}");

        return builder.ConnectionString;
    }

    /// <summary>
    /// Connection string for the Cosmos DB emulator the AppHost runs. The emulator serves plain HTTP on a
    /// fixed host port and uses the well-known emulator key, so it's safe to hard-code here.
    /// Pass it to <c>UseCosmos(connectionString, databaseName)</c>.
    /// </summary>
    public static string CreateCosmos()
    {
        var connectionString =
            $"AccountEndpoint=http://127.0.0.1:{CosmosEmulatorPort}/;AccountKey={CosmosEmulatorKey}";

        Console.WriteLine($"Cosmos connection string: {connectionString}");

        return connectionString;
    }

    // Must match the gateway port in AppHost/Program.cs.
    private const int CosmosEmulatorPort = 8081;

    // The emulator's published, fixed key: https://learn.microsoft.com/azure/cosmos-db/emulator#authentication
    private const string CosmosEmulatorKey =
        "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
}
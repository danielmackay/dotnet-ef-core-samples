using Microsoft.Data.SqlClient;

namespace Common;

public static class DbConnectionFactory
{
    public static string Create(string databaseName)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = "localhost",
            InitialCatalog = databaseName,
            UserID = "sa",
            Password = "yourStrong(!)Password",
            TrustServerCertificate = true,
            MultipleActiveResultSets = true
        };

        Console.WriteLine($"Generated connection string: {builder.ConnectionString}");

        return builder.ConnectionString;
    }
}
using Microsoft.Data.SqlClient;

namespace Common;

public static class DbConnectionFactory
{
    public static string Create(string databaseName)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = "localhost:1433",
            InitialCatalog = databaseName,
            IntegratedSecurity = true,
            MultipleActiveResultSets = true
        };

        return builder.ConnectionString;
    }
}
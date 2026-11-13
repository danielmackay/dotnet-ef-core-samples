using Microsoft.Data.SqlClient;

namespace Common;

/// <summary>
/// EF Core 11 emits the latest VECTOR_SEARCH syntax (<c>SELECT TOP(n) WITH APPROXIMATE ... FROM VECTOR_SEARCH(...)</c>).
/// Azure SQL Database supports it; boxed SQL Server 2025 (RTM to CU11 at the time of writing) only has the earlier
/// syntax and rejects the query with "Incorrect syntax near 'APPROXIMATE'" (or 'TOP_N'), error 102.
/// </summary>
public static class VectorSearchSyntax
{
    private const int IncorrectSyntax = 102;

    private static readonly string[] s_unsupportedTokens = ["'APPROXIMATE'", "'TOP_N'","'VECTOR_SEARCH'"];

    /// <summary>
    /// True only when the server rejected EF Core 11's VECTOR_SEARCH syntax. Any other error, including other
    /// syntax errors, returns false, so the sample fails instead of skipping.
    /// </summary>
    public static bool IsUnsupported(SqlException ex) =>
        ex.Number == IncorrectSyntax
        && s_unsupportedTokens.Any(token => ex.Message.Contains($"Incorrect syntax near {token}", StringComparison.Ordinal));
}

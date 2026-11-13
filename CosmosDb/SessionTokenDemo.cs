using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure;

namespace CosmosDb;

/// <summary>
/// Session consistency guarantees read-your-writes only within a session, identified by a session token.
/// EF 11 lets you take the token from one context and hand it to another (another request, another app
/// instance), so the second context also reads the first one's writes.
/// </summary>
public static class SessionTokenDemo
{
    private const string Region = "SESSION";

    public static async Task RunAsync()
    {
        Console.WriteLine();
        Console.WriteLine("== Session tokens ==");

        var capture = new CosmosCapture();

        // The default, FullyAutomatic, keeps tokens inside EF and doesn't expose them.
        await using (var db = new ApplicationDbContext(capture))
        {
            try
            {
                db.Database.GetSessionToken();
                Check.That(false, "GetSessionToken() throws in the default FullyAutomatic mode");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"  FullyAutomatic: {ex.Message.Split('\n')[0].Trim()}");
                Check.That(true, "GetSessionToken() throws in the default FullyAutomatic mode");
            }
        }

        // SemiAutomatic: EF still tracks tokens itself, and also lets you read them.
        string? token;
        await using (var writer = new ApplicationDbContext(capture,
                         cosmos => cosmos.SessionTokenManagementMode(SessionTokenManagementMode.SemiAutomatic)))
        {
            writer.Customers.Add(new Customer
            {
                Id = "session-1",
                Region = Region,
                Name = "Written by context 1",
                ShippingAddress = new Address { Street = "1 Session Ln", City = "Hobart", Postcode = "7000", Location = new Geo(-42.88, 147.33) },
            });
            await writer.SaveChangesAsync();

            // The token for the default container (HasDefaultContainer in the model). GetSessionTokens()
            // returns one per container.
            token = writer.Database.GetSessionToken();
            Console.WriteLine($"Session token after the write: {token}");
            Console.WriteLine($"All tokens: {string.Join(", ", writer.Database.GetSessionTokens().Select(t => $"{t.Key}={t.Value}"))}");
            Check.That(!string.IsNullOrEmpty(token), "the writing context exposes a session token after SaveChanges");
        }

        // Manual: EF uses only the tokens you give it. A second context picks up the first one's session.
        await using var reader = new ApplicationDbContext(capture,
            cosmos => cosmos.SessionTokenManagementMode(SessionTokenManagementMode.Manual));
        if (token is not null)
        {
            reader.Database.UseSessionToken(token);
        }

        var customer = await reader.Customers.FindAsync("session-1", Region);
        Console.WriteLine($"Read in context 2: {customer?.Name ?? "(not found)"}");
        Check.Equal("Written by context 1", customer?.Name,
            "a second context using the first context's session token reads the new document");
    }
}

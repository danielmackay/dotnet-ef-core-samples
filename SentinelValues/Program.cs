using Microsoft.EntityFrameworkCore;
using SentinelValues;

Console.WriteLine("Sentinel Values Sample");

using var db = new ApplicationDbContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

var accounts = new List<Account>
{
    new(),
    new() { Balance = 0, Credits = 0 }, // NOTE: Without Sentinel values this would use the defaults of Balance=100, and Credits=10.
    new() { Balance = 100, Credits = 10 },
};

db.Accounts.AddRange(accounts);
db.SaveChanges();

var allAccounts = db.Accounts.ToList();
Console.WriteLine("All Account");
allAccounts.ForEach(Console.WriteLine);

Console.ReadLine();
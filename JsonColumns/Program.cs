using JsonColumns;

Console.WriteLine("Json Columns Sample");

using var db = new ApplicationDbContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

var contacts = new List<Contact>
{
    new()
    {
        Name = "John Doe",
        Address = new Address { Line1 = "123 Main St.", Line2 = "Suite 101", City = "Seattle", State = "WA" },
        Notes =
            new List<Note>
            {
                new() { Text = "Note 1" }, new() { Text = "Note 2" }, new() { Text = "Note 3" }
            }
    },
    new()
    {
        Name = "Jane Doe",
        Address = new Address { Line1 = "456 Main St.", Line2 = "Suite 202", City = "Seattle", State = "WA" },
        Notes = new List<Note>
        {
            new() { Text = "Note 4" }, new() { Text = "Note 5" }, new() { Text = "Note 6" }
        }
    },
    new()
    {
        Name = "John Smith",
        Address = new Address { Line1 = "789 Main St.", Line2 = "Suite 303", City = "Seattle", State = "WA" },
        Notes = new List<Note>
        {
            new() { Text = "Note 7" }, new() { Text = "Note 8" }, new() { Text = "Note 9" }
        }
    }
};

Console.WriteLine("Inserting Data");
db.Contacts.AddRange(contacts);
db.SaveChanges();

var contact = db.Contacts.First(c => c.Address.Line1 == "456 Main St.");
Console.WriteLine(contact);

var notes = db.Contacts
    .First(c => c.Name == "John Smith")
    .Notes;

ArgumentNullException.ThrowIfNull(notes);

foreach (var note in notes)
    Console.WriteLine(note);

Console.WriteLine("Updating Data");
var contactToUpdate = db.Contacts.First(c => c.Name == "John Doe");
contactToUpdate.Address.Line1 = "111 Update St.";
db.SaveChanges();

Console.WriteLine("Deleting Data");
var contactToDelete = db.Contacts.First(c => c.Name == "Jane Doe");
contactToDelete.Address = new Address();
db.SaveChanges();

Console.ReadLine();
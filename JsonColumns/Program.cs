using JsonColumns;

Console.WriteLine("Json Columns Sample");

using var db = new ApplicationDbContext();
await db.Database.EnsureDeletedAsync();
await db.Database.EnsureCreatedAsync();

var contacts = new List<Contact>
{
    new Contact
    {
        Name = "John Doe",
        Address = new Address
        {
            Line1 = "123 Main St.",
            Line2 = "Suite 101",
            City = "Seattle",
            State = "WA"
        },
        Notes = new List<Note>
        {
            new Note { Text = "Note 1"},
            new Note { Text = "Note 2"},
            new Note { Text = "Note 3"}
        }
    },
    new Contact
    {
        Name = "Jane Doe",
        Address = new Address
        {
            Line1 = "456 Main St.",
            Line2 = "Suite 202",
            City = "Seattle",
            State = "WA"
        },
        Notes = new List<Note>
        {
            new Note { Text = "Note 4"},
            new Note { Text = "Note 5"},
            new Note { Text = "Note 6"}
        }
    },
    new Contact
    {
        Name = "John Smith",
        Address = new Address
        {
            Line1 = "789 Main St.",
            Line2 = "Suite 303",
            City = "Seattle",
            State = "WA"
        },
        Notes = new List<Note>
        {
            new Note { Text = "Note 7"},
            new Note { Text = "Note 8"},
            new Note { Text = "Note 9"}
        }
    }
};

Console.WriteLine("Inserting Data");
db.Contacts.AddRange(contacts);
db.SaveChanges();

// TODO: Query data

// TODO: Update data

// TODO: Delete data

Console.ReadLine();
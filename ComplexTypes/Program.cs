using ComplexTypes;

Console.WriteLine("Complex Types Sample");

using var db = new ApplicationDbContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

var contacts = new List<Contact>
{
    new()
    {
        Name = "John Doe",
        Address = new Address
        {
            Street = "123 Main Street",
            City = "Redmond",
            PostCode = "WA 98052"
        }
    },
    new()
    {
        Name = "Jane Doe",
        Address = new Address
        {
            Street = "123 Main Street",
            City = "Redmond",
            PostCode = "WA 98052"
        }
    }
};

db.Contacts.AddRange(contacts);
db.SaveChanges();

var allContacts = db.Contacts.ToList();
allContacts.ForEach(Console.WriteLine);

Console.ReadLine();
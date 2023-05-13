Console.WriteLine("Owned Entities Sample");

using var db = new ApplicationDbContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

var orders = new List<Order>()
{
    new Order(),
    new Order(),
    new Order()
};

db.Orders.AddRange(orders);
db.SaveChanges();

var allOrders = db.Orders.ToList();
allOrders.ForEach(Console.WriteLine);

Console.ReadLine();


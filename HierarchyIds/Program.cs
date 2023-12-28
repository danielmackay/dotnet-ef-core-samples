using HierarchyIds;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Hierarchy IDs Sample");

using var db = new ApplicationDbContext();
db.Database.EnsureDeleted();
db.Database.EnsureCreated();

var employees = new List<Employee>
{
    new() { Name = "CEO", Path = HierarchyId.Parse("/")},
    new() { Name = "Product Manager", Path = HierarchyId.Parse("/1/")},
    new() { Name = "Tech Lead", Path = HierarchyId.Parse("/1/1/")},
    new() { Name = "Senior Dev", Path = HierarchyId.Parse("/1/1/1/")},
    new() { Name = "Junior Dev", Path = HierarchyId.Parse("/1/1/2/")},
    new() { Name = "Intern", Path = HierarchyId.Parse("/1/1/3/")},
};

db.Employees.AddRange(employees);
db.SaveChanges();

var techLead = db.Employees.First(e => e.Path == HierarchyId.Parse("/1/1/"));

// Get tech lead subordinates
var techLeadSubordinates = db.Employees
    .AsNoTracking()
    .Where(e => e.Path.IsDescendantOf(techLead.Path))
    .ToList();

Console.WriteLine("Tech Lead Team");
techLeadSubordinates.ForEach(Console.WriteLine);

// Get tech lead parents
var techLeadManagers = FindAllAncestors("Tech Lead").ToList();
Console.WriteLine("Tech Lead Managers");
techLeadManagers.ForEach(Console.WriteLine);

var ceo = db.Employees.First(e => e.Path == HierarchyId.Parse("/"));

Console.WriteLine($"Is Tech Lead a descendant of CEO? {techLead.Path.IsDescendantOf(ceo.Path)}");
Console.WriteLine($"Is CEO a descendant of Tech Lead? {ceo.Path.IsDescendantOf(techLead.Path)}");

Console.ReadLine();

IQueryable<Employee> FindAllAncestors(string name)
    => db.Employees.Where(
            ancestor => db.Employees
                .Single(
                    descendent =>
                        descendent.Name == name
                        && ancestor.Id != descendent.Id)
                .Path.IsDescendantOf(ancestor.Path))
        .OrderByDescending(ancestor => ancestor.Path.GetLevel());
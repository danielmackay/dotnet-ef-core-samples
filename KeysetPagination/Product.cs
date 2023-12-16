namespace KeysetPagination;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }

    override public string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Price: {Price}";
    }
}
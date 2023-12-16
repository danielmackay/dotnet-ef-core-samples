namespace BackingFields;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }

    private string? _voucherCode;

    public string? VoucherCode
    {
        get => _voucherCode;
        set => _voucherCode = value?.ToUpperInvariant();
    }

    private DateTime _createdTs;

    public Product()
    {
        _createdTs = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Price: {Price}, VoucherCode: {VoucherCode}, CreatedTs: {_createdTs}";
    }
}
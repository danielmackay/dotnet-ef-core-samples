public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsDeleted { get; set; }
    public DateTime CreatedTs { get; set; }
    public DateTime? UpdatedTs { get; set; }
}

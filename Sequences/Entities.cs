public class Order
{
    public int Id { get; set; }
    public int OrderNumber { get; set; }

    override public string ToString()
    {
        return $"Id: {Id}, OrderNumber: {OrderNumber}";
    }
}

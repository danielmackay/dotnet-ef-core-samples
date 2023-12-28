namespace SentinelValues;

public class Account
{
    public int Id { get; private set; }

    // One way to use sentinel values is to use a nullable backing field
    private int? _balance;
    public int Balance
    {
        get => _balance ?? 100;
        set => _balance = value;
    }

    // Another way to use sentinel values is to use a sentinel value that's been configured in ApplicationDbContext
    public int Credits { get; set; } = -1;

    public override string ToString()
    {
        return $"Id: {Id}, Balance: {Balance}, Credits: {Credits}";
    }
}
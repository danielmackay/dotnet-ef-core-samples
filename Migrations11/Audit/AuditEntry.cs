namespace Migrations11.Audit;

public class AuditEntry
{
    public int Id { get; set; }
    public required string Message { get; set; }
    public DateTime CreatedAt { get; set; }
}

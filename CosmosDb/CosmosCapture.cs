using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CosmosDb;

/// <summary>
/// The Cosmos counterpart of Common's SqlCapture. Cosmos has no DbCommand to intercept, so this records the
/// provider's diagnostic events instead: the Cosmos SQL of every query, and every item write, point read
/// and transactional batch. Opt in with <see cref="Attach"/>.
/// </summary>
public sealed class CosmosCapture
{
    private readonly List<EventData> _events = [];
    private readonly Lock _lock = new();

    /// <summary>Every captured event since creation or the last <see cref="Clear"/>, oldest first.</summary>
    public IReadOnlyList<EventData> Events
    {
        get
        {
            lock (_lock)
            {
                return [.. _events];
            }
        }
    }

    /// <summary>The Cosmos SQL text of every query executed.</summary>
    public IReadOnlyList<string> Queries => [.. Events.OfType<CosmosQueryEventData>().Select(e => e.QuerySql)];

    /// <summary>The most recent query's SQL, or an empty string if none ran.</summary>
    public string LastQuery => Queries.LastOrDefault() ?? string.Empty;

    /// <summary>Captured events of one kind, e.g. <c>Of&lt;CosmosTransactionalBatchExecutedEventData&gt;()</c>.</summary>
    public IReadOnlyList<T> Of<T>() where T : EventData => [.. Events.OfType<T>()];

    public void Clear()
    {
        lock (_lock)
        {
            _events.Clear();
        }
    }

    public DbContextOptionsBuilder Attach(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.LogTo((eventId, _) => CapturedEventIds.Contains(eventId.Id), Record);

    private void Record(EventData eventData)
    {
        lock (_lock)
        {
            _events.Add(eventData);
        }
    }

    private static readonly HashSet<int> CapturedEventIds =
    [
        CosmosEventId.ExecutingSqlQuery.Id,
        CosmosEventId.ExecutingReadItem.Id,
        CosmosEventId.ExecutedCreateItem.Id,
        CosmosEventId.ExecutedReplaceItem.Id,
        CosmosEventId.ExecutedDeleteItem.Id,
        CosmosEventId.ExecutedTransactionalBatch.Id,
    ];
}

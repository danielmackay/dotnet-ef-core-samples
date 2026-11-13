using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Common;

/// <summary>
/// Records the text of every command a DbContext executes, so a sample can check what SQL ran.
/// Opt in with <c>optionsBuilder.AddInterceptors(sqlCapture)</c>.
/// </summary>
public sealed class SqlCapture : DbCommandInterceptor
{
    private readonly List<string> _commands = [];
    private readonly Lock _lock = new();

    /// <summary>Every command captured since creation or the last <see cref="Clear"/>, oldest first.</summary>
    public IReadOnlyList<string> Commands
    {
        get { lock (_lock) return [.. _commands]; }
    }

    /// <summary>The most recent command, or an empty string if none has run.</summary>
    public string Last
    {
        get { lock (_lock) return _commands.Count > 0 ? _commands[^1] : string.Empty; }
    }

    /// <summary>Forget captured commands, typically just before the query you want to check.</summary>
    public void Clear()
    {
        lock (_lock) _commands.Clear();
    }

    private void Record(DbCommand command)
    {
        lock (_lock) _commands.Add(command.CommandText);
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Record(command);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Record(command);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        Record(command);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Record(command);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        Record(command);
        return result;
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        Record(command);
        return ValueTask.FromResult(result);
    }
}

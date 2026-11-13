namespace Common;

/// <summary>
/// Self-checks for samples. Each check prints a PASS or FAIL line. Any failure sets
/// <see cref="Environment.ExitCode"/> to 1, so the process exits non-zero when it ends
/// (as long as the sample doesn't return its own exit code from Main).
/// </summary>
public static class Check
{
    private static int s_passed;
    private static int s_failed;
    private static int s_skipped;

    public static bool That(bool condition, string description)
    {
        if (condition)
        {
            s_passed++;
            Console.WriteLine($"PASS: {description}");
        }
        else
        {
            s_failed++;
            Environment.ExitCode = 1;
            Console.WriteLine($"FAIL: {description}");
        }

        return condition;
    }

    public static bool Equal<T>(T expected, T actual, string description)
    {
        var equal = EqualityComparer<T>.Default.Equals(expected, actual);
        return That(equal, equal ? description : $"{description} (expected: {expected}, actual: {actual})");
    }

    /// <summary>Checks that each value is no greater than the next.</summary>
    public static bool Ascending<T>(IEnumerable<T> values, string description) =>
        That(IsOrdered(values, descending: false), description);

    /// <summary>Checks that each value is no smaller than the next.</summary>
    public static bool Descending<T>(IEnumerable<T> values, string description) =>
        That(IsOrdered(values, descending: true), description);

    /// <summary>
    /// Records a check that couldn't run here (e.g. the server lacks a feature) and prints a SKIP line.
    /// A skip doesn't fail the run, but it shows in <see cref="Summary"/>.
    /// </summary>
    public static void Skip(string description, string reason)
    {
        s_skipped++;
        Console.WriteLine($"SKIP: {description} ({reason})");
    }

    /// <summary>Prints the totals. Optional: the exit code is already set by any failed check.</summary>
    public static void Summary()
    {
        var totals = s_failed == 0
            ? $"All {s_passed} checks passed"
            : $"{s_failed} of {s_passed + s_failed} checks FAILED";
        Console.WriteLine(s_skipped == 0 ? totals : $"{totals}, {s_skipped} skipped");
    }

    private static bool IsOrdered<T>(IEnumerable<T> values, bool descending)
    {
        var sign = descending ? -1 : 1;
        var list = values.ToList();
        return list.Zip(list.Skip(1)).All(pair => sign * Comparer<T>.Default.Compare(pair.First, pair.Second) <= 0);
    }
}

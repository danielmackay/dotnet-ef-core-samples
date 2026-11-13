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

    public static bool Equal<T>(T expected, T actual, string description) =>
        That(EqualityComparer<T>.Default.Equals(expected, actual),
            EqualityComparer<T>.Default.Equals(expected, actual)
                ? description
                : $"{description} (expected: {expected}, actual: {actual})");

    /// <summary>Prints the totals. Optional: the exit code is already set by any failed check.</summary>
    public static void Summary() =>
        Console.WriteLine(s_failed == 0
            ? $"All {s_passed} checks passed"
            : $"{s_failed} of {s_passed + s_failed} checks FAILED");
}

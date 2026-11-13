namespace Common;

public static class StringExtensions
{
    /// <summary>The first line of a (usually multi-line) message, trimmed, for printing a short summary.</summary>
    public static string FirstLine(this string text) => text.Split('\n')[0].Trim();
}

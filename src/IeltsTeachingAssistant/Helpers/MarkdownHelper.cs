using System.Text.RegularExpressions;

namespace IeltsTeachingAssistant.Helpers;

public static class MarkdownHelper
{
    public static string ParseToHtml(string? markdown)
    {
        if (string.IsNullOrEmpty(markdown)) return string.Empty;

        // Clean up or handle malformed markers (e.g. unclosed tags or mismatched asterisks)
        var result = markdown;

        // Replace bold **text** with <strong>text</strong>
        result = Regex.Replace(result, @"\*\*(.*?)\*\*", "<strong>$1</strong>");

        // Replace italic *text* with <em>text</em>
        result = Regex.Replace(result, @"\*(.*?)\*", "<em>$1</em>");

        return result;
    }
}

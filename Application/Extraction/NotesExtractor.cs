using System.Text.RegularExpressions;

namespace Application.Extraction;

public static class NotesExtractor
{
    // Matches d/m/yyyy, dd/mm/yyyy, d/m/yy - day-first, confirmed by data (day values up to 31 appear)
    private static readonly Regex DateRegex = new(@"\b(\d{1,2})/(\d{1,2})/(\d{2,4})\b", RegexOptions.Compiled);

    // Title-case token: first letter upper, rest lower (incl. Albanian ç/ë).
    private static readonly Regex TitleCaseTokenRegex = new(@"^[A-ZÇË][a-zçë]+$", RegexOptions.Compiled);

    public static DateTime? ExtractDate(string notes)
    {
        var match = DateRegex.Match(notes);
        if (!match.Success) return null;

        int day = int.Parse(match.Groups[1].Value);
        int month = int.Parse(match.Groups[2].Value);
        int year = int.Parse(match.Groups[3].Value);
        if (year < 100) year += 2000;

        try
        {
            return new DateTime(year, month, day);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null; // malformed date (e.g. day=32) - let the caller treat this row as a failure
        }
    }

    // Generates candidate 2-word name phrases from runs of Title-Case tokens.
    public static List<string> ExtractNameCandidates(string notes)
    {
        var tokens = Regex.Matches(notes, @"[\p{L}-]+")
            .Select(m => m.Value)
            .ToList();

        var candidates = new List<string>();
        for (int i = 0; i < tokens.Count - 1; i++)
        {
            if (TitleCaseTokenRegex.IsMatch(tokens[i]) && TitleCaseTokenRegex.IsMatch(tokens[i + 1]))
            {
                candidates.Add($"{tokens[i]} {tokens[i + 1]}");
            }
        }

        return candidates;
    }
}
using Application.Dtos;
using Application.Extraction;
using Application.Interfaces;
using Domain.Entities;
using F23.StringSimilarity;

namespace Infrastructure.Matching;

public class JaroWinklerNameMatcher : INameMatcher
{
    private const double MatchThreshold = 0.82;

    private readonly JaroWinkler _similarity = new();

    public ClientMatch? FindBestMatch(string notesText, IReadOnlyCollection<Client> candidates)
    {
        var namePhrases = NotesExtractor.ExtractNameCandidates(notesText);
        if (namePhrases.Count == 0 || candidates.Count == 0)
            return null;

        ClientMatch? best = null;

        foreach (var phrase in namePhrases)
        {
            var normalizedPhrase = Normalize(phrase);

            foreach (var client in candidates)
            {
                var fullName = $"{client.FirstName} {client.LastName}";
                var score = _similarity.Similarity(normalizedPhrase, Normalize(fullName));

                if (best is null || score > best.Score)
                {
                    best = new ClientMatch
                    {
                        Client = client,
                        Score = score,
                        MatchedCandidateText = phrase
                    };
                }
            }
        }

        return best is not null && best.Score >= MatchThreshold ? best : null;
    }

    private static string Normalize(string input) =>
        input.Trim().ToLowerInvariant().Replace("ç", "c").Replace("ë", "e");
}
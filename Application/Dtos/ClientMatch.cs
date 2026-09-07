using Domain.Entities;

namespace Application.Dtos;

public class ClientMatch
{
    public required Client Client { get; init; }
    public required double Score { get; init; }
    public required string MatchedCandidateText { get; init; }
}
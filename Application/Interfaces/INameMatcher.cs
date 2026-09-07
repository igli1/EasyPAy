using Application.Dtos;
using Domain.Entities;

namespace Application.Interfaces;

public interface INameMatcher
{
    ClientMatch? FindBestMatch(string notesText, IReadOnlyCollection<Client> candidates);
}
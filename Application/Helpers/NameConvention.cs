using Application.Dtos;

namespace Application.Helpers;

public class NameConvention
{
    public static Domain.Entities.Client ToClient(RawPersonRow row)
    {
        var (first, last) = SplitName(row.FullName);
        return new Domain.Entities.Client { FirstName = first, LastName = last };
    }

    public static Domain.Entities.Technician ToTechnician(RawPersonRow row)
    {
        var (first, last) = SplitName(row.FullName);
        return new Domain.Entities.Technician { FirstName = first, LastName = last };
    }

    private static (string First, string Last) SplitName(string fullName)
    {
        var parts = fullName.Trim().Split(' ', 2);
        return parts.Length == 2 ? (parts[0], parts[1]) : (parts[0], string.Empty);
    }
}
using Application.Dtos;
using Application.Interfaces;
using ClosedXML.Excel;

namespace Infrastructure.Excel;

public class ExcelPersonReader: IExcelReader<RawPersonRow>
{
    public IEnumerable<RawPersonRow> ReadRows(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        // Row 1 is the header ("Client" / "Technician"), so data starts at row 2
        var rows = worksheet.RowsUsed().Skip(1);

        foreach (var row in rows)
        {
            var fullName = row.Cell(1).GetString().Trim();

            if (string.IsNullOrWhiteSpace(fullName))
                continue;

            yield return new RawPersonRow { FullName = fullName };
        }
    }
}
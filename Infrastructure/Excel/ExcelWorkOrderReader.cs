using Application.Dtos;
using Application.Interfaces;
using ClosedXML.Excel;

namespace Infrastructure.Excel;
    public class ExcelWorkOrderReader : IExcelReader<RawWorkOrderRow>
    {
        public IEnumerable<RawWorkOrderRow> ReadRows(Stream stream)
        {
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var rows = worksheet.RowsUsed().Skip(1);

            foreach (var row in rows)
            {
                var technician = row.Cell(1).GetString().Trim();
                var notes = row.Cell(2).GetString().Trim();
                var totalCell = row.Cell(3);

                if (string.IsNullOrWhiteSpace(technician) && string.IsNullOrWhiteSpace(notes))
                    continue;

                decimal total = totalCell.TryGetValue<decimal>(out var value) ? value : 0m;

                yield return new RawWorkOrderRow
                {
                    TechnicianName = technician,
                    Notes = notes,
                    Total = total
                };
            }
        }
    }
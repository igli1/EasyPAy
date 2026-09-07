namespace Application.Dtos;

public class RawWorkOrderRow
{
    public string TechnicianName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
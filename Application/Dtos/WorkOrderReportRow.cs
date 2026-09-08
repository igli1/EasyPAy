namespace Application.Dtos;

public class WorkOrderReportRow
{
    public long WorkOrderId { get; set; }
    public string TechnicianName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Information { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
}
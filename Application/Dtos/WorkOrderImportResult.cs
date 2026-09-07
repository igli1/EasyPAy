namespace Application.Dtos;

public class WorkOrderImportResult
{
    public int RowNumber { get; set; }
    public string TechnicianName { get; set; } = string.Empty;
    public string RawNotes { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public string? MatchedClientName { get; set; }
    public double? MatchScore { get; set; }
    public DateTime? ExtractedDate { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class WorkOrder
{
    [Key]
    public long Id { get; set; }
    public string Information { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Total { get; set; }

    public int ClientId { get; set; }
    public int TechnicianId { get; set; }

    public Client Client { get; set; } = null!;
    public Technician Technician { get; set; } = null!;
}
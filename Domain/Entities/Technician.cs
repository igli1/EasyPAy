using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Technician
{
    [Key]
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } =  string.Empty;
}
public class Appointment
{
    [System.ComponentModel.DataAnnotations.Key]
    public string? Id { get; set; }
    public string? Date { get; set; }
    public string? Time { get; set; }
    public string? Status { get; set; }
}


public class Appointment
{
	[System.ComponentModel.DataAnnotations.Key]
    public int Id { get; }
    public string? Date { get; }
    public string? Time { get; }
    public string? Status { get; }
}


public enum AppointmentStatus
{
    Available,
    Booked,
}
public class Appointment
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public DateTime Time { get; set; }
    public AppointmentStatus Status { get; set; }
}


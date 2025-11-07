public class AppointmentRequest
{
    public required System.Guid AppointmentId { get; set; }
    public required string ServiceName { get; set; }
    public required ScheduledAppointment ScheduledAppointment { get; set; }
}

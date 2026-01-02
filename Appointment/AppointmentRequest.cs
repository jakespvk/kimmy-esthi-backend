namespace KimmyEsthi.Appointment;

public class AppointmentRequest
{
    public System.Guid AppointmentId { get; set; }
    public required ScheduledAppointment ScheduledAppointment { get; set; }
}

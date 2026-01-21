using System;

namespace KimmyEsthi.Appointment;

public class AppointmentRequest
{
    public Guid AppointmentId { get; set; }
    public required ScheduledAppointment ScheduledAppointment { get; set; }
    public Promotion? Promotion { get; set; }
}

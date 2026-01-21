using System;

namespace KimmyEsthi.Appointment;

public class Appointment
{
    public Guid Id { get; set; }
    public DateTime DateTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public ScheduledAppointment? ScheduledAppointment { get; set; }
    public ConsentForm.ConsentForm? ConsentForm { get; set; }
    public Promotion? Promotion { get; set; }

    public override string ToString()
    {
        return "ID: "
            + Id.ToString()
            + "DateTime: "
            + DateTime.ToString()
            + "AppointmentStatus: "
            + Status.ToString()
            + ScheduledAppointment?.ToString();
    }
}

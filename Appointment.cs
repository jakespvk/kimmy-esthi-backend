public class Appointment
{
    public System.Guid Id { get; set; }
    public System.DateTime DateTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public ScheduledAppointment? ScheduledAppointment { get; set; }

    public override string ToString()
    {
        return "ID: " + Id.ToString()
            + "DateTime: " + DateTime.ToString()
            + "AppointmentStatus: " + Status.ToString()
            + ScheduledAppointment?.ToString();
    }
}


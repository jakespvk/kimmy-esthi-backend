using System;
using KimmyEsthi.Clients;

namespace KimmyEsthi.Appointments;

public class ScheduledAppointment
{
    public Guid AppointmentId { get; set; }
    public Guid ClientId { get; set; }
    public required string ServiceName { get; set; }
    public required Client Client { get; set; }
    public required string SkinConcerns { get; set; }
}

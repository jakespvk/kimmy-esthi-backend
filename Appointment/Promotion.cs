using System;

namespace KimmyEsthi.Appointment;

public class Promotion
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Guid AppointmentId { get; set; }
}

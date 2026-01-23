using System;

namespace KimmyEsthi.Appointments;

public class Promotion
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Guid AppointmentId { get; set; }
}

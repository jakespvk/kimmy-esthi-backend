using System;

namespace KimmyEsthi.Appointment;

public record ConsentForm
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public required string PrintedName { get; set; }
    public required string[] InitialedStatements { get; set; }
    public required string Initials { get; set; }
    public required string Signature { get; set; }
}

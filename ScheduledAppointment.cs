using System;

public class ScheduledAppointment
{
    public required Guid AppointmentId { get; set; }
    public required string PreferredName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string SkinConcerns { get; set; }
}

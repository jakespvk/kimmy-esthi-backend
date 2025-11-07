using System;
using System.ComponentModel.DataAnnotations;

public class Appointment
{
    [Key]
    public Guid Id { get; set; }
    public DateTime DateTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public required string ServiceName { get; set; }
    public ScheduledAppointment? ScheduledAppointment { get; set; }
}


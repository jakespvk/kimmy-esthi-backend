using System;
using System.ComponentModel.DataAnnotations;

public class Appointment
{
    [Key]
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public DateTime Time { get; set; }
    public AppointmentStatus Status { get; set; }
    public ScheduledAppointment? ScheduledAppointment { get; set; }
}


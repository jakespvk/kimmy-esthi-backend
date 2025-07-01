using System.ComponentModel.DataAnnotations;

public class ScheduledAppointment
{
    [Key]
    public required Guid AppointmentId;
    //
    // private string _preferredName;
    // private string _email;
    // private string _phoneNumber;
    // private string _skinConcerns;
    //
    // public ScheduledAppointment(string preferredName,
    //         string email, string phoneNumber, string skinConcerns)
    // {
    //     _preferredName = preferredName;
    //     _email = email;
    //     _phoneNumber = phoneNumber;
    //     _skinConcerns = skinConcerns;
    // }

    public required string PreferredName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string SkinConcerns { get; set; }
}

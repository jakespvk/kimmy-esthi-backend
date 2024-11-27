public class ScheduledAppointment
{
    private string _preferredName;
    private string _email;
    private string _phoneNumber;
    private string _skinConcerns;

    public ScheduledAppointment(string preferredName, 
            string email, string phoneNumber, string skinConcerns) 
    {
        _preferredName = preferredName;
        _email = email;
        _phoneNumber = phoneNumber;
        _skinConcerns = skinConcerns;
    }

    public string PreferredName { get { return _preferredName; } }
    public string Email { get { return _email; } }
    public string PhoneNumber { get { return _phoneNumber; } }
    public string SkinConcerns { get { return _skinConcerns; } }
}

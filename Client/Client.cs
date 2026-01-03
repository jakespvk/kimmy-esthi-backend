using System;
using System.Collections.Generic;

namespace KimmyEsthi.Client;

public class Client
{
    public Guid ClientId { get; set; }
    public Guid AppointmentId { get; set; }
    public ConsentForm.ConsentForm? ConsentForm { get; set; }
    public required string PreferredName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public List<string>? SkinConcerns { get; set; }
}

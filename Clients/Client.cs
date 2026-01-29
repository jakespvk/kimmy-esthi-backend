using System;
using System.Collections.Generic;

namespace KimmyEsthi.Clients;

public class Client
{
    public Guid ClientId { get; set; }
    public Guid AppointmentId { get; set; }
    public ConsentForm.ConsentForm? ConsentForm { get; set; }
    public required string PreferredName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public List<string>? SkinConcerns { get; set; }
}

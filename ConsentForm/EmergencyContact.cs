using System;

namespace KimmyEsthi.ConsentForm;

public class EmergencyContact
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public required string Name { get; set; }
    public required string Phone { get; set; }
    public required string Relationship { get; set; }
}

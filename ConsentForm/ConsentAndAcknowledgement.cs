using System;

namespace KimmyEsthi.ConsentForm;

public class ConsentAndAcknowledgement
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public required string Signature { get; set; }
}

using System;

namespace KimmyEsthi.ConsentForm;

public sealed class ConsentFormSummaryDTO
{
    public Guid ConsentFormId { get; set; }
    public required string ClientName { get; set; }
    public required string ClientEmail { get; set; }
    public required string ClientPhone { get; set; }
}

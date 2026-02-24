using System;

namespace KimmyEsthi.ConsentForm;

public class ProductsUsed
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public required string Products { get; set; }
}

namespace KimmyEsthi.ConsentForm;

public class ConsentFormStatement
{
    public int Id { get; set; }
    public required string Statement { get; set; }
    public bool IsActive { get; set; } = true;
}

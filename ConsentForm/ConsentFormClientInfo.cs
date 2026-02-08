using System;
namespace KimmyEsthi.ConsentForm;

public class ConsentFormClientInfo
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required DateTime DOB { get; set; }
    public required string Gender { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Email { get; set; }
}

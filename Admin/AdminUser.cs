namespace KimmyEsthi.Admin;

public class AdminUser
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? Token { get; set; }
}

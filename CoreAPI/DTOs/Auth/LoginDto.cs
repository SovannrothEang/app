namespace CoreAPI.DTOs.Auth;

public class LoginDto
{
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
}

namespace CoreAPI.DTOs.Auth;

public class ChangePasswordCommand
{
    public string CurrentPassword { get; init; } = default!;
    public string NewPassword { get; init; } = default!;
}

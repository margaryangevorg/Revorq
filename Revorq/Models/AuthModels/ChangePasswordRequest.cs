namespace Revorq.API.Models.AuthModels;

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmedPassword { get; set; } = string.Empty;
}

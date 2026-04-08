namespace Application.DTOs;

public class ResetPasswordData
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
}

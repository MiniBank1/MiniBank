namespace Infrastructure.Services;

public class PendingRegisterData
{
    public string UserName { get; set; } = null!;
    public string UserSurname { get; set; } = null!;
    public string TcNo { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Code { get; set; } = null!;
}

public class ResetPasswordData
{
    public int UserId { get; set; }
    public string Email { get; set; } = null!;
}
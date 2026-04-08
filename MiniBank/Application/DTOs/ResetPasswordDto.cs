using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class ResetPasswordDto
{
    [Required(ErrorMessage = "Token is required.")]
    public string Token { get; set; } = null!;

    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters.")]
    public string NewPassword { get; set; } = null!;

    [Required(ErrorMessage = "Confirm password is required.")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = null!;
}
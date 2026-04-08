using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class VerifyRegisterDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Verification code is required.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be 6 digits.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Verification code must contain only digits.")]
    public string Code { get; set; } = string.Empty;
}
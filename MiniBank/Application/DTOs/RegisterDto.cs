using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    public string UserSurname { get; set; } = null!;

    [Required(ErrorMessage = "ID number is required.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "ID number must be 11 digits.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "ID number must contain only digits.")]
    public string TcNo { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Confirm password is required.")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = null!;
}
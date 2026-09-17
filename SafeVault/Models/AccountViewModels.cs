using System.ComponentModel.DataAnnotations;

namespace SafeVault.Models;

public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(32, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 32 characters.")]
    [RegularExpression("^[A-Za-z0-9_]+$", ErrorMessage = "Username may contain letters, numbers, and underscores only.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(254, ErrorMessage = "Email is too long.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(128, MinimumLength = 12, ErrorMessage = "Password must be between 12 and 128 characters.")]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).+$", ErrorMessage = "Password must include uppercase, lowercase, number, and symbol characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(32)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

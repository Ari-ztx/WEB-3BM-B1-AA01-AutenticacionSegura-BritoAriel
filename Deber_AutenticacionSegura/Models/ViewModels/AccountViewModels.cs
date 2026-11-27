using System.ComponentModel.DataAnnotations;
namespace Deber_AutenticacionSegura.Models.ViewModels;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Escribe tu correo."), EmailAddress(ErrorMessage = "Escribe un correo válido."), StringLength(256), Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;
}
public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "Escribe tu correo."), EmailAddress(ErrorMessage = "Escribe un correo válido."), StringLength(256), Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Token { get; set; } = string.Empty;
    [Required(ErrorMessage = "Escribe una contraseña."), StringLength(128), DataType(DataType.Password), Display(Name = "Nueva contraseña")]
    public string Password { get; set; } = string.Empty;
    [Required(ErrorMessage = "Confirma tu contraseña."), StringLength(128), DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden."), Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
public class ProfileViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Escribe tu contraseña actual."), StringLength(128), DataType(DataType.Password), Display(Name = "Contraseña actual")]
    public string CurrentPassword { get; set; } = string.Empty;
    [Required(ErrorMessage = "Escribe una nueva contraseña."), StringLength(128), DataType(DataType.Password), Display(Name = "Nueva contraseña")]
    public string NewPassword { get; set; } = string.Empty;
    [Required(ErrorMessage = "Confirma la nueva contraseña."), StringLength(128), DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden."), Display(Name = "Confirmar nueva contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
public class AdminUserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}


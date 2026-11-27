using System.ComponentModel.DataAnnotations;
namespace Deber_AutenticacionSegura.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Escribe tu nombre."), StringLength(100), Display(Name = "Nombre completo")]
    public string FullName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Escribe tu correo."), EmailAddress(ErrorMessage = "Escribe un correo válido."), StringLength(256), Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Escribe una contraseña."), StringLength(128), DataType(DataType.Password), Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;
    [Required(ErrorMessage = "Confirma tu contraseña."), StringLength(128), DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden."), Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;
namespace Deber_AutenticacionSegura.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio. ")]
    [EmailAddress(ErrorMessage = "Escribe un correo valido. ")]
    [Display(Name = "Correo Electronico")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La contraseña es obligatorio. ")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;
    
    [Display(Name = "Recordarme")]
    public bool RememberMe { get; set; }
}
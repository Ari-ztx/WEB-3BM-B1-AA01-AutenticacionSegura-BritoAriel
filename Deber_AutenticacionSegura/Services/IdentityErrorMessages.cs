using Microsoft.AspNetCore.Identity;
namespace Deber_AutenticacionSegura.Services;
public static class IdentityErrorMessages
{
    public static string Translate(IdentityError error) => error.Code switch
    {
        "DuplicateEmail" or "DuplicateUserName" => "Ya existe una cuenta con ese correo.",
        "InvalidEmail" or "InvalidUserName" => "El correo indicado no es válido.",
        "PasswordTooShort" => "La contraseña no alcanza la longitud mínima configurada.",
        "PasswordRequiresNonAlphanumeric" => "La contraseña debe contener un carácter especial.",
        "PasswordRequiresDigit" => "La contraseña debe contener un número.",
        "PasswordRequiresLower" => "La contraseña debe contener una letra minúscula.",
        "PasswordRequiresUpper" => "La contraseña debe contener una letra mayúscula.",
        "PasswordMismatch" => "La contraseña actual es incorrecta.",
        "InvalidToken" => "El enlace no es válido, ha caducado o ya fue utilizado.",
        _ => error.Description
    };
}

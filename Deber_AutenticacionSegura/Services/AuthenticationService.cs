using Deber_AutenticacionSegura.Data;
using Deber_AutenticacionSegura.Models;
using Deber_AutenticacionSegura.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Deber_AutenticacionSegura.Services;

public class AuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    public async Task<IdentityResult> RegisterAsync(
        RegisterViewModel model)
    {
        // La transacción agrupa la creación del usuario y su rol.
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var user = new ApplicationUser
        {
            Fullname = model.FullName.Trim(),
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            IsEnabled = true
        };

        // Identity valida los datos y protege la contraseña.
        var result = await _userManager.CreateAsync(
            user,
            model.Password
        );

        if (!result.Succeeded)
        {
            return result;
        }

        // Toda cuenta creada desde el registro público será Cliente.
        var roleResult = await _userManager.AddToRoleAsync(
            user,
            "Cliente"
        );

        if (!roleResult.Succeeded)
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Description = "No se pudo asignar el rol al usuario."
                }
            );
        }

        await transaction.CommitAsync();

        return IdentityResult.Success;
    }
    public async Task<bool> LoginAsync(LoginViewModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email.Trim());

        if (user == null || !user.IsEnabled)
        {
            return false;
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true
        );

        return result.Succeeded;
    }
    public async Task LogoutAsync(ClaimsPrincipal principal)
    {
        var user = await _userManager.GetUserAsync(principal);

        if (user != null)
        {
            var result = await _userManager.UpdateSecurityStampAsync(user);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "No se pudo invalidar la sesión."
                );
            }
        }

        await _signInManager.SignOutAsync();
    }
}
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Deber_AutenticacionSegura.Data;
using Deber_AutenticacionSegura.Models;
using Deber_AutenticacionSegura.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace Deber_AutenticacionSegura.Services;

public class AuthenticationService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly IEmailService _email;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(ApplicationDbContext db, UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn,
        IEmailService email, IConfiguration configuration, ILogger<AuthenticationService> logger)
    {
        _db = db; _users = users; _signIn = signIn; _email = email; _configuration = configuration; _logger = logger;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var user = new ApplicationUser { UserName = model.Email.Trim(), Email = model.Email.Trim(), Fullname = model.FullName.Trim(), IsEnabled = true };
        var result = await _users.CreateAsync(user, model.Password);
        if (!result.Succeeded) return result;
        result = await _users.AddToRoleAsync(user, "Cliente");
        if (!result.Succeeded)
            return IdentityResult.Failed(new IdentityError { Description = "No se pudo completar el registro. Inténtalo de nuevo." });
        await transaction.CommitAsync();
        return IdentityResult.Success;
    }

    public async Task<bool> LoginAsync(LoginViewModel model)
    {
        var user = await _users.FindByEmailAsync(model.Email.Trim());
        if (user is null || !user.IsEnabled) return false;
        var result = await _signIn.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
        return result.Succeeded;
    }

    public async Task LogoutAsync(ClaimsPrincipal principal)
    {
        // Invalida también copias de la cookie ya emitida; afecta a todas las sesiones de esta cuenta.
        try
        {
            var user = await _users.GetUserAsync(principal);
            if (user is not null && !(await _users.UpdateSecurityStampAsync(user)).Succeeded)
                throw new InvalidOperationException("No se pudieron invalidar las sesiones de la cuenta.");
        }
        finally
        {
            await _signIn.SignOutAsync();
        }
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        var user = await _users.FindByEmailAsync(email.Trim());
        if (user is null || !user.IsEnabled || user.Email is null) return;
        try
        {
            var baseUrl = _configuration["App:BaseUrl"];
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttps && !(uri.IsLoopback && uri.Scheme == Uri.UriSchemeHttp)))
                throw new InvalidOperationException("App:BaseUrl debe ser una URL HTTPS absoluta (o HTTP localhost para desarrollo).");
            var token = await _users.GeneratePasswordResetTokenAsync(user);
            var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetUrl = QueryHelpers.AddQueryString(new Uri(uri, "/Account/ResetPassword").ToString(),
                new Dictionary<string, string?> { ["email"] = user.Email, ["token"] = encoded });
            var safeUrl = HtmlEncoder.Default.Encode(resetUrl);
            await _email.SendAsync(user.Email, "Recuperar contraseña", $"<p>Recibimos una solicitud para cambiar tu contraseña.</p><p><a href=\"{safeUrl}\">Restablecer contraseña</a></p><p>Si no solicitaste el cambio, ignora este mensaje. El enlace caduca y solo puede utilizarse una vez.</p>");
        }
        catch (Exception ex)
        {
            // Nunca registrar el enlace, el token ni las credenciales de correo.
            _logger.LogError(ex, "No se pudo procesar una solicitud de recuperación. Revisa la configuración del correo y la URL de la aplicación.");
        }
    }

    public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordViewModel model)
    {
        var user = await _users.FindByEmailAsync(model.Email.Trim());
        if (user is null || !user.IsEnabled) return InvalidReset();
        try
        {
            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            var result = await _users.ResetPasswordAsync(user, token, model.Password);
            if (!result.Succeeded && result.Errors.Any(e => e.Code == "InvalidToken")) return InvalidReset();
            return result;
        }
        catch (FormatException) { return InvalidReset(); }
    }

    private static IdentityResult InvalidReset() => IdentityResult.Failed(new IdentityError { Description = "El enlace no es válido, ha caducado o ya fue utilizado." });

    public async Task<ProfileViewModel?> GetProfileAsync(ClaimsPrincipal principal)
    {
        var user = await _users.GetUserAsync(principal);
        return user is null || !user.IsEnabled ? null : new ProfileViewModel { FullName = user.Fullname, Email = user.Email ?? "" };
    }

    public async Task<IdentityResult> ChangePasswordAsync(ClaimsPrincipal principal, ChangePasswordViewModel model)
    {
        var user = await _users.GetUserAsync(principal);
        if (user is null || !user.IsEnabled) return IdentityResult.Failed(new IdentityError { Description = "No se pudo cambiar la contraseña." });
        var result = await _users.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded) await _signIn.RefreshSignInAsync(user);
        return result;
    }
}


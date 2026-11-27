using Deber_AutenticacionSegura.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Deber_AutenticacionSegura.Data;

public static class IdentitySeeder
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");

        foreach (var name in new[] { "Cliente", "Administrador" })
        {
            if (!await roleManager.RoleExistsAsync(name))
                EnsureSuccess(await roleManager.CreateAsync(new IdentityRole(name)));
        }

        // La configuración inicial no modifica contraseñas ni promueve cuentas existentes.
        if ((await userManager.GetUsersInRoleAsync("Administrador")).Count > 0) return;
        var email = configuration["Admin:Email"];
        var password = configuration["Admin:Password"];
        var nameAdmin = configuration["Admin:FullName"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(nameAdmin))
        {
            logger.LogWarning("Para crear el primer administrador, configura Admin:Email, Admin:Password y Admin:FullName mediante User Secrets.");
            return;
        }
        if (await userManager.FindByEmailAsync(email.Trim()) is not null)
            throw new InvalidOperationException("El correo del administrador inicial ya pertenece a otra cuenta; no se promoverá automáticamente.");

        await using var transaction = await db.Database.BeginTransactionAsync();
        var admin = new ApplicationUser
        {
            UserName = email.Trim(), Email = email.Trim(), Fullname = nameAdmin.Trim(), IsEnabled = true
        };
        EnsureSuccess(await userManager.CreateAsync(admin, password));
        EnsureSuccess(await userManager.AddToRoleAsync(admin, "Administrador"));
        await transaction.CommitAsync();
        logger.LogInformation("Se creó el administrador inicial.");
    }

    private static void EnsureSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException("No se pudo inicializar Identity: " + string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}

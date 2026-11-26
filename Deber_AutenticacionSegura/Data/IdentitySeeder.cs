using Microsoft.AspNetCore.Identity;

namespace Deber_AutenticacionSegura.Data;

public static class IdentitySeeder
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Cliente", "Administrador" };

        foreach (string roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(roleName)
                );

                if (!result.Succeeded)
                {
                    throw new Exception(
                        $"No se pudo crear el rol {roleName}."
                    );
                }
            }
        }
    }
}
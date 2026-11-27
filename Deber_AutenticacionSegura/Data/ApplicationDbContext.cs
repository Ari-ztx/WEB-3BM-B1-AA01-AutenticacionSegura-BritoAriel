using Deber_AutenticacionSegura.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Deber_AutenticacionSegura.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationUser>().ToTable("Usuarios");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UsuariosRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("DatosUsuarios");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("DatosRoles");
        builder.Entity<IdentityUserLogin<string>>().ToTable("AccesosExternos");
        builder.Entity<IdentityUserToken<string>>().ToTable("TokensUsuarios");
    }
}

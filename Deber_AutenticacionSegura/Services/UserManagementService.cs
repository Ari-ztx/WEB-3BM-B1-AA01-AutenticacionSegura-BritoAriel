using System.Data;
using Deber_AutenticacionSegura.Data;
using Deber_AutenticacionSegura.Models;
using Deber_AutenticacionSegura.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Deber_AutenticacionSegura.Services;

public record ManagementResult(bool Succeeded, string Message);

public class UserManagementService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    public UserManagementService(ApplicationDbContext db, UserManager<ApplicationUser> users) { _db = db; _users = users; }

    public async Task<List<AdminUserViewModel>> ListAsync()
    {
        var users = await _users.Users.OrderBy(u => u.Fullname).ToListAsync();
        var result = new List<AdminUserViewModel>();
        foreach (var user in users)
        {
            var roles = await _users.GetRolesAsync(user);
            result.Add(new AdminUserViewModel { Id = user.Id, FullName = user.Fullname, Email = user.Email ?? "", Role = roles.Contains("Administrador") ? "Administrador" : "Cliente", IsEnabled = user.IsEnabled });
        }
        return result;
    }

    // Serializable impide que dos cambios simultáneos dejen al sistema sin administrador activo.
    public async Task<ManagementResult> UpdateRoleAsync(string userId, string role)
    {
        if (role != "Cliente" && role != "Administrador") return new(false, "El rol indicado no es válido.");
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var user = await _users.FindByIdAsync(userId);
        if (user is null) return new(false, "No se encontró la cuenta.");
        var roles = await _users.GetRolesAsync(user);
        if (role != "Administrador" && user.IsEnabled && roles.Contains("Administrador") && await ActiveAdministratorCountAsync() <= 1)
            return new(false, "Debe permanecer al menos un administrador activo.");
        if (roles.Count == 1 && roles.Contains(role)) return new(true, "El rol ya estaba asignado.");
        if (roles.Count > 0 && !(await _users.RemoveFromRolesAsync(user, roles)).Succeeded)
            return new(false, "No se pudo cambiar el rol.");
        if (!(await _users.AddToRoleAsync(user, role)).Succeeded || !(await _users.UpdateSecurityStampAsync(user)).Succeeded)
            return new(false, "No se pudo cambiar el rol.");
        await transaction.CommitAsync();
        return new(true, "Rol actualizado correctamente.");
    }

    public async Task<ManagementResult> UpdateStatusAsync(string userId, bool isEnabled)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var user = await _users.FindByIdAsync(userId);
        if (user is null) return new(false, "No se encontró la cuenta.");
        if (!isEnabled && user.IsEnabled && await _users.IsInRoleAsync(user, "Administrador") && await ActiveAdministratorCountAsync() <= 1)
            return new(false, "Debe permanecer al menos un administrador activo.");
        if (user.IsEnabled == isEnabled) return new(true, "La cuenta ya tenía ese estado.");
        user.IsEnabled = isEnabled;
        if (!(await _users.UpdateAsync(user)).Succeeded || !(await _users.UpdateSecurityStampAsync(user)).Succeeded)
            return new(false, "No se pudo actualizar la cuenta.");
        await transaction.CommitAsync();
        return new(true, isEnabled ? "Cuenta habilitada." : "Cuenta deshabilitada.");
    }

    private Task<int> ActiveAdministratorCountAsync() =>
        (from user in _db.Users
         join userRole in _db.UserRoles on user.Id equals userRole.UserId
         join role in _db.Roles on userRole.RoleId equals role.Id
         where user.IsEnabled && role.NormalizedName == "ADMINISTRADOR"
         select user.Id).Distinct().CountAsync();
}

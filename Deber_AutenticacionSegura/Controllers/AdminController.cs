using Deber_AutenticacionSegura.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Deber_AutenticacionSegura.Controllers;

[Authorize(Roles = "Administrador")]
public class AdminController : Controller
{
    private readonly UserManagementService _management;
    public AdminController(UserManagementService management) => _management = management;

    [HttpGet]
    public async Task<IActionResult> Index() => View(await _management.ListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRole(string userId, string role)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role)) return BadRequest();
        var result = await _management.UpdateRoleAsync(userId, role);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string userId, bool? isEnabled)
    {
        if (!ModelState.IsValid || !isEnabled.HasValue || string.IsNullOrWhiteSpace(userId)) return BadRequest();
        var result = await _management.UpdateStatusAsync(userId, isEnabled.Value);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}

using Deber_AutenticacionSegura.Models.ViewModels;
using Deber_AutenticacionSegura.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Deber_AutenticacionSegura.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly AuthenticationService _auth;
    public ProfileController(AuthenticationService auth) => _auth = auth;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var profile = await _auth.GetProfileAsync(User);
        return profile is null ? Challenge() : View(profile);
    }

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _auth.ChangePasswordAsync(User, model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, IdentityErrorMessages.Translate(error));
            return View(model);
        }
        TempData["Success"] = "Contraseña actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}


using Deber_AutenticacionSegura.Models.ViewModels;
using Deber_AutenticacionSegura.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Deber_AutenticacionSegura.Controllers;

public class AccountController : Controller
{
    private readonly AuthenticationService _authenticationService;

    public AccountController(
        AuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    // Muestra el formulario.
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // Recibe los datos del formulario.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authenticationService.RegisterAsync(model);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        TempData["Success"] = "Tu cuenta fue creada correctamente.";

        return RedirectToAction("Index", "Home");
    }
    // Muestra el formulario de inicio de sesión.
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

// Recibe los datos del formulario.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var inicioCorrecto = await _authenticationService.LoginAsync(model);

        if (!inicioCorrecto)
        {
            ModelState.AddModelError(
                string.Empty,
                "No fue posible iniciar sesión. Revisa tus datos o inténtalo más tarde."
            );

            return View(model);
        }

        TempData["Success"] = "Iniciaste sesión correctamente.";

        return RedirectToAction("Index", "Home");
    }
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authenticationService.LogoutAsync(User);

        TempData["Success"] = "Cerraste sesión correctamente.";

        return RedirectToAction("Index", "Home");
    }
}
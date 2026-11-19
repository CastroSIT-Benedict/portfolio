
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using portfolio.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace portfolio.Controllers;

public class AccountController : Controller
{
    private readonly IConfiguration _configuration;

    public AccountController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var username = _configuration["Authentication:Username"];
        var password = _configuration["Authentication:Password"];

        if (string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError("", "Login is not configured.");
            return View(model);
        }

        // Check the submitted credentials.
        bool validUsername = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(model.Username),
            Encoding.UTF8.GetBytes(username));

        bool validPassword = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(model.Password),
            Encoding.UTF8.GetBytes(password));

        if (!validUsername || !validPassword)
        {
            ModelState.AddModelError("", "Invalid username or password.");
            return View(model);
        }

        // Create the editor identity.
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Editor")
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        if (!string.IsNullOrEmpty(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize(Roles = "Editor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Index", "Home");
    }
}

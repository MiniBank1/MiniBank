using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace MiniBank.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _authService.RegisterAsync(dto);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(dto);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction("VerifyRegister", new { email = dto.Email });
    }

    [HttpGet]
    public IActionResult VerifyRegister()
    {
        var model = new VerifyRegisterDto
        {
            Email = TempData["Email"]?.ToString() ?? string.Empty
        };

        return View(model);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyRegister(VerifyRegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _authService.VerifyRegisterAsync(dto);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(dto);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var user = await _authService.LoginAsync(dto);

        if (user == null)
        {
            TempData["ErrorMessage"] = "Invalid email or password.";
            return View(dto);
        }

        var claims = new List<Claim>
        {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Name, $"{user.UserName} {user.UserSurname}".Trim()),
        new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        var claimsIdentity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            claimsPrincipal);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var resetBaseUrl = Url.Action("ResetPassword", "Auth", null, Request.Scheme)!;
        var result = await _authService.ForgotPasswordAsync(dto, resetBaseUrl);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(dto);
        }

        TempData["SuccessMessage"] = result.Message;
        return View(dto);
    }

    [HttpGet]
    public IActionResult ResetPassword(string token)
    {
        var model = new ResetPasswordDto
        {
            Token = token
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _authService.ResetPasswordAsync(dto);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(dto);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction("Login");
    }
}
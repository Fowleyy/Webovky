using Microsoft.AspNetCore.Mvc;
using Semestralka.Domain.Exceptions;
using Semestralka.Infrastructure.Services;
using Semestralka.Presentation.Models.DTOs;

namespace Semestralka.Presentation.Controllers;

public class AccountController : Controller
{
    private readonly AuthService _authService;

    public AccountController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("/login")]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("userid") != null)
            return Redirect("/");

        return View();
    }

    [HttpPost("/login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        try
        {
            var user = await _authService.LoginAsync(dto);

            HttpContext.Session.SetString("userid", user.Id.ToString());
            HttpContext.Session.SetString("email", user.Email ?? "");
            HttpContext.Session.SetString("fullname", user.FullName ?? "Uživatel");
            HttpContext.Session.SetString("avatar", user.AvatarPath ?? "/avatars/default.png");
            HttpContext.Session.SetString("isAdmin", user.IsAdmin ? "1" : "0");

            return Redirect("/");
        }
        catch (DomainValidationException ex)
        {
            ViewData["Error"] = ex.Message;
            return View();
        }
    }

    [HttpGet("/register")]
    public IActionResult Register() => View();

    [HttpPost("/register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        try
        {
            await _authService.RegisterAsync(dto);

            return Redirect("/login");
        }
        catch (DomainValidationException ex)
        {
            ViewData["Error"] = ex.Message;
            return View();
        }
    }

    [HttpGet("/logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Redirect("/login");
    }
}

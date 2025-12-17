using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Semestralka.Domain.Entities;
using Semestralka.Domain.Exceptions;
using Semestralka.Infrastructure.Services;
using Semestralka.Presentation.Models.DTOs;

namespace Semestralka.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    private const string JwtKey =
        "THIS_IS_THE_FINAL_TEST_KEY_1234567890_ABCDEF_0987654321";

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        try
        {
            await _authService.RegisterAsync(dto);
            return Ok();
        }
        catch (DomainValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        try
        {
            var user = await _authService.LoginAsync(dto);

            var token = GenerateToken(user);

            HttpContext.Session.SetString("userid", user.Id.ToString());
            HttpContext.Session.SetString("email", user.Email ?? "");
            HttpContext.Session.SetString("fullname", user.FullName ?? "Uživatel");
            HttpContext.Session.SetString("isAdmin", user.IsAdmin ? "1" : "0");

            return Ok(new { token });
        }
        catch (DomainValidationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(JwtKey)
        );

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var claims = new[]
        {
            new Claim("userid", user.Id.ToString()),
            new Claim("email", user.Email!)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

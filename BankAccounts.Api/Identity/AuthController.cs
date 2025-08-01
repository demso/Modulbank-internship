using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Notes.Identity.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BankAccounts.Api.Identity;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController(
    SignInManager<BankUser> signInManager,
    UserManager<BankUser> userManager,
    IIdentityServerInteractionService interactionService, IConfiguration configuration, IdentityDbContext<BankUser> iDbContext)
    : Controller
{
    [HttpPost]
    public async Task<ActionResult> Login(LoginData data) {
        var user = await userManager.FindByNameAsync(data.Username);
        if (user == null)
            return BadRequest("User not found");

        var result = await signInManager.PasswordSignInAsync(data.Username,
            data.Password, false, false);
        if (!result.Succeeded)
            return Problem("Login failed.");

        var token = GenerateJwtToken(data.Username);

        return Ok(token);
    }

    private string GenerateJwtToken(string username)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, username)
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(60),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost]
    public async Task<ActionResult> Register(RegisterData data)
    {
        var user = new BankUser {
            UserName = data.Username
        };

        var result = await userManager.CreateAsync(user, data.Password);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join("\n | ", result.Errors.Select(error => error.Description));
            return Problem(errorMessage);
        }
        
        await signInManager.SignInAsync(user, false);

        return Ok("Register succeed.");
    }

    [HttpGet]
    public async Task<ActionResult> Logout(string logoutId) {
        await signInManager.SignOutAsync();
        var logoutRequest = await interactionService.GetLogoutContextAsync(logoutId);
        return Ok(logoutRequest.PostLogoutRedirectUri);
    }
}
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notes.Identity.Models;

namespace BankAccounts.Api.Identity;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController(
    SignInManager<BankUser> signInManager,
    UserManager<BankUser> userManager,
    IIdentityServerInteractionService interactionService)
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

        return Ok("Login succeed.");
    }

    [HttpPost]
    public async Task<ActionResult> Register(RegisterData data) {
        var user = new BankUser {
            UserName = data.Username
        };

        var result = await userManager.CreateAsync(user, data.Password);
        if (!result.Succeeded)
            return Problem("Register error.");
        
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
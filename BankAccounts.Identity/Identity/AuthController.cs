using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

// ReSharper disable GrammarMistakeInComment

namespace BankAccounts.Api.Identity;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController(
    SignInManager<BankUser> signInManager,
    UserManager<BankUser> userManager,
    IConfiguration configuration)
    : ControllerBase
{
    /// <summary>
    /// Registers user.
    /// </summary>
    /// <remarks>
    /// <code>
    /// POST {{address}}/api/auth/register </code>
    /// </remarks>
    /// <returns>Returns MbResult&lt;string&gt;</returns>
    /// <response code="200">Success</response>
    /// <response code="400">Registration error</response>
    [HttpPost]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Register(RegisterData data)
    {
        var user = new BankUser
        {
            UserName = data.Username
        };

        var result = await userManager.CreateAsync(user, data.Password!);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join("\n | ", result.Errors.Select(error => error.Description));
            return BadRequest(errorMessage);
        }

        await signInManager.SignInAsync(user, false);

        return Ok("Register succeed.");
    }

    /// <summary>
    /// Logins user. Returns token, use it to authorize account operations.
    /// </summary>
    /// <remarks>
    /// <code>
    /// POST {{address}}/api/auth/login </code>
    /// </remarks>
    /// <returns>Returns MbResult&lt;string&gt; with token</returns>
    /// <response code="200">Success</response>
    /// <response code="400">Login error</response>
    ///  <response code="404">User is not registered</response>
    [HttpPost]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Login(LoginData data) {
        var user = await userManager.FindByNameAsync(data.Username!);
        if (user == null)
            return NotFound("User is not registered");

        var result = await signInManager.PasswordSignInAsync(data.Username!,
            data.Password!, false, false);
        if (!result.Succeeded)
            return BadRequest("Login failed");

        var token = GenerateJwtToken(user);

        return Ok(token);
    }

    private string GenerateJwtToken(BankUser user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
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
}
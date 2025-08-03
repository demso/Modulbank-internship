using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

// ReSharper disable GrammarMistakeInComment

namespace BankAccounts.Api.Identity;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController(
    SignInManager<BankUser> signInManager,
    UserManager<BankUser> userManager,
    IConfiguration configuration)
    : CustomControllerBase
{
    /// <summary>
    /// Registers user. You need to login to be able to operate accounts.
    /// </summary>
    /// <remarks>
    /// <code>
    /// POST {{address}}/api/auth/register </code>
    /// </remarks>
    /// <returns>Returns MbResult&lt;string&gt;</returns>
    /// <response code="200">Success</response>
    /// <response code="400">Registration error</response>
    [HttpPost]
    [ProducesResponseType(typeof(MbResult<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult<string>), StatusCodes.Status400BadRequest)]
    public async Task<MbResult<string>> Register(RegisterData data)
    {
        var user = new BankUser
        {
            UserName = data.Username
        };

        var result = await userManager.CreateAsync(user, data.Password!);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join("\n | ", result.Errors.Select(error => error.Description));
            return Failure(StatusCodes.Status400BadRequest, errorMessage);
        }

        await signInManager.SignInAsync(user, false);

        return Success(StatusCodes.Status200OK, "Register succeed.");
    }

    /// <summary>
    /// Logins user.
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
    [ProducesResponseType(typeof(MbResult<string>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MbResult<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MbResult<string>), StatusCodes.Status404NotFound)]
    public async Task<MbResult<string>> Login(LoginData data) {
        var user = await userManager.FindByNameAsync(data.Username!);
        if (user == null)
            return Failure(StatusCodes.Status404NotFound, "User is not registered");

        var claims = new List<Claim>
        {
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            claimsPrincipal,
            new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10) 
            });

        return Success(StatusCodes.Status200OK, "Login successful");
    }

}

public class SubjectClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity.IsAuthenticated)
        {
            var identity = (ClaimsIdentity)principal.Identity;

            // Если нет sub claim - добавляем
            if (!principal.HasClaim(c => c.Type == "sub"))
            {
                var nameId = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (nameId != null)
                {
                    identity.AddClaim(new Claim("sub", nameId.Value));
                }
                else
                {
                    // Добавляем фиктивный sub
                    identity.AddClaim(new Claim("sub", Guid.NewGuid().ToString()));
                }
            }
        }

        return Task.FromResult(principal);
    }
}
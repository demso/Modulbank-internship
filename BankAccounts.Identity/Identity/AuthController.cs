using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BankAccounts.Identity.Identity;

/// <summary>
/// Контроллер аутентификации
/// </summary>
[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController(
    SignInManager<BankUser> signInManager,
    UserManager<BankUser> userManager,
    IConfiguration configuration)
    : ControllerBase
{
    /// <summary>
    /// Регистрирует пользователя
    /// </summary>
    /// <remarks>
    /// <code>
    /// POST {{address}}/api/auth/register </code>
    /// </remarks>
    /// <returns>string message</returns>
    /// <response code="200">Успешно</response>
    /// <response code="500">Ошибка при регистрации</response>
    [HttpPost]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
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
            return StatusCode(500, errorMessage);
        }

        await signInManager.SignInAsync(user, false);

        return Ok("Register succeed.");
    }

    /// <summary>
    /// Аутентифицирует пользователя. Возвращает токен, используйте его для доступа к операциям сервиса BankAccountsAPI.
    /// </summary>
    /// <remarks>
    /// <code>
    /// POST {{address}}/api/auth/login </code>
    /// </remarks>
    /// <returns>Токен</returns>
    /// <response code="200">Успешно</response>
    ///  <response code="404">Пользователь не зарегистрирован</response>
    /// <response code="500">Ошибка входа</response>
    [HttpPost]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Login(LoginData data) {
        var user = await userManager.FindByNameAsync(data.Username!);
        if (user == null)
            return NotFound("User is not registered");

        var result = await signInManager.PasswordSignInAsync(data.Username!,
            data.Password!, false, false);
        if (!result.Succeeded)
            return StatusCode(500, "Login failed");

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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace BankAccounts.Identity.Identity;

/// <summary>
/// Контроллер аутентификации
/// </summary>
[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController(
    SignInManager<BankUser> signInManager,
    UserManager<BankUser> userManager,
    AuthDbContext dbContext,
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
        BankUser user = new()
        {
            UserName = data.Username
        };

        IdentityResult result = await userManager.CreateAsync(user, data.Password!);
        if (!result.Succeeded)
        {
            string errorMessage = string.Join("\n | ", result.Errors.Select(error => error.Description));
            return StatusCode(500, errorMessage);
        }

        await signInManager.SignInAsync(user, false);

        await dbContext.SaveChangesAsync();

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
    /// <response code="400">Ошибка входа</response>
    [HttpPost]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Login(LoginData data) {
        BankUser? user = await userManager.FindByNameAsync(data.Username!);
        if (user == null)
            return NotFound("User is not registered");

        SignInResult result = await signInManager.PasswordSignInAsync(data.Username!,
            data.Password!, false, false);
        if (!result.Succeeded)
            return StatusCode(400, "Login failed");

        string token = GenerateJwtToken(user);

        return Ok(token);
    }

    private string GenerateJwtToken(BankUser user)
    {
        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.NameIdentifier, CreateGuidFromString(user.UserName!).ToString())
        ];

        JwtSecurityToken token = new(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static Guid CreateGuidFromString(string input)
    {
        byte[] hash = SHA1.HashData(Encoding.UTF8.GetBytes(input));

        // Берём первые 16 байт для GUID
        byte[] guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);

        // Устанавливаем версию GUID (RFC 4122)
        guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | 0x50);
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

        return new Guid(guidBytes);
    }
}
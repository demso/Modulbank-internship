using System.ComponentModel.DataAnnotations;

// ReSharper disable UnusedAutoPropertyAccessor.Global Свойства используются

namespace BankAccounts.Identity.Identity;

/// <summary>
/// Класс представляющий собой информацию, необходимую для входа пользователя
/// </summary>
public class LoginData
{
    /// <summary>
    /// Логин
    /// </summary>
    [Required]
    public string? Username { get; init; }
    /// <summary>
    /// Пароль
    /// </summary>
    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; init; }
}
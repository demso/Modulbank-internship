using System.ComponentModel.DataAnnotations;

// ReSharper disable UnusedAutoPropertyAccessor.Global Свойства используются

namespace BankAccounts.Identity.Identity;

/// <summary>
/// Класс, представляющий собой информацию, необходимую для регистрации пользователя
/// </summary>
public class RegisterData
{
    /// <summary>
    /// Логин
    /// </summary>
    [Required]
    public string? Username { get; set; }
    /// <summary>
    /// Пароль
    /// </summary>
    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}


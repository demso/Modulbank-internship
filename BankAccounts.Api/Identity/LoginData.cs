using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Api.Identity;

public class LoginData
{
    [Required]
    public string Username { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
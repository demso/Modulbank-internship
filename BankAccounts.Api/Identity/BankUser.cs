using Microsoft.AspNetCore.Identity;

namespace BankAccounts.Api.Identity;

public class BankUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
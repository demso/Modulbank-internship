using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace BankAccounts.Identity.Identity;

/// <summary>
/// Настройка сервера идентификации
/// </summary>
public static class IdentityServerConfiguration
{
    /// <summary>
    /// Области видимости
    /// </summary>
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new("BankAccountsWebAPI", "Web API")
        };

    /// <summary>
    /// Идентификационные ресурсы
    /// </summary>
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };

    /// <summary>
    /// Ресурсы API
    /// </summary>
    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new("BankAccountsWebAPI", "Web API", [JwtClaimTypes.Name])
            {
                Scopes = { "BankAccountsWebAPI" }
            }
        };

    /// <summary>
    /// Список клиентов
    /// </summary>
    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new()
            {
                ClientId = "bank-accounts-web-app",
                ClientName = "Bank Accounts Web",
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                RequirePkce = true,
                RedirectUris = { "https://localhost:80/index.html" },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "BankAccountsWebAPI"
                },
                AllowAccessTokensViaBrowser = true
            }
        };
}
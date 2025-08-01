using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;

namespace BankAccounts.Api.Identity;

public class Configuration
{
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("BankAccountsWebAPI", "Web API")
        };

    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new ApiResource("BankAccountsWebAPI", "Web API", new []
                { JwtClaimTypes.Name})
            {
                Scopes = { "BankAccountsWebAPI" }
            }
        };

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new Client
            {
                ClientId = "bank-accounts-web-app",
                ClientName = "Bank Accounts Web",
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                RequirePkce = true,
                RedirectUris =
                {
                    "https://localhost:80/signin-oidc"
                },
                AllowedCorsOrigins =
                {
                    "https://localhost:80"
                },
                PostLogoutRedirectUris =
                {
                    "https://localhost:80/signout-oidc"
                },
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
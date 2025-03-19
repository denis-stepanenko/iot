using Duende.IdentityServer.Models;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource("color", new [] { "favorite_color" })
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("scope1"),
            new ApiScope("scope2"),
            new ApiScope("iotapi", "IOT API")
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // m2m client credentials flow client
            new Client
            {
                ClientId = "m2m.client",
                ClientName = "Client Credentials Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                AllowedScopes = { "scope1" }
            },

            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "interactive",
                ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:44300/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:44300/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:44300/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "scope2" }
            },

            new Client
                {
                    ClientId = "spa",
                    ClientName = "SinglePage",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireClientSecret = false,
                    
                    AccessTokenLifetime = (int)TimeSpan.FromMinutes(15).TotalSeconds,

                    // Refresh Token Rotation
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    SlidingRefreshTokenLifetime = (int)TimeSpan.FromDays(180).TotalSeconds,
                    AllowOfflineAccess = true,

                    RedirectUris = { Environment.GetEnvironmentVariable("APP_CLIENT_URL") ?? "" },
                    PostLogoutRedirectUris = { Environment.GetEnvironmentVariable("APP_CLIENT_URL") ?? "" },
                    AllowedCorsOrigins = { Environment.GetEnvironmentVariable("APP_CLIENT_URL") ?? "" },

                    AllowedScopes = { "openid", "profile", "iotapi", "color" },
                },

           


        };
}

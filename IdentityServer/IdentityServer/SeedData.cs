using System.Security.Claims;
using IdentityModel;
using IdentityServer.Data;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityServer;

public class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDb>();

            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = userMgr.FindByNameAsync("admin").Result;
            if (admin == null)
            {
                string defaultUserUserName = Environment.GetEnvironmentVariable("APP_DEFAULT_USER_USERNAME")
                    ?? throw new Exception("Default user username is not specified");

                string defaultUserPassword = Environment.GetEnvironmentVariable("APP_DEFAULT_USER_PASSWORD")
                    ?? throw new Exception("Default user password is not specified");

                string defaultUserEmail = Environment.GetEnvironmentVariable("APP_DEFAULT_USER_EMAIL")
                    ?? throw new Exception("Default user email is not specified");

                string defaultUserName = Environment.GetEnvironmentVariable("APP_DEFAULT_USER_NAME")
                    ?? throw new Exception("Default user name is not specified");

                string defaultUserGivenName = Environment.GetEnvironmentVariable("APP_DEFAULT_USER_GIVENNAME")
                    ?? throw new Exception("Default user given name is not specified");

                string defaultUserFamilyName = Environment.GetEnvironmentVariable("APP_DEFAULT_USER_FAMILYNAME")
                    ?? throw new Exception("Default user given name is not specified");

                admin = new ApplicationUser
                {
                    UserName = defaultUserUserName,
                    Email = defaultUserEmail,
                    EmailConfirmed = true,
                };

                var result = userMgr.CreateAsync(admin, defaultUserPassword).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(admin, new Claim[]{
                            new Claim(JwtClaimTypes.Name, defaultUserName),
                            new Claim(JwtClaimTypes.GivenName, defaultUserGivenName),
                            new Claim(JwtClaimTypes.FamilyName, defaultUserFamilyName)
                        }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Debug("Default user created");
            }
            else
            {
                Log.Debug("Default user already exists");
            }
        }
    }
}

using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Models;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.Services
{
    public class CustomProfileService : ProfileService<ApplicationUser>
    {
        public CustomProfileService(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory) : base(userManager, claimsFactory)
        {
        }

        protected override async Task GetProfileDataAsync(ProfileDataRequestContext context, ApplicationUser user)
        {
            var principal = await GetUserClaimsAsync(user);

            if (principal.Identity is null)
                throw new ArgumentNullException("Identity claims were null");

            var id = (ClaimsIdentity)principal.Identity;

            id.AddClaim(new Claim("favorite_color", "user.FavoriteColor"));

            context.AddRequestedClaims(principal.Claims);

            //var claims = new List<Claim>();
            //claims.Add(new Claim("favorite_color", "user.FavoriteColor"));


            //context.IssuedClaims = claims;
        }
    }
}

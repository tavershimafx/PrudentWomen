using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Monochrome.Module.Core.Models;
using System.Security.Claims;

namespace Monochrome.Module.Core.Extensions
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Returns the user currently logged in with valid credentials in the session
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="AggregateException"></exception>
        public async static Task<User> GetCurrentUser(this IHttpContextAccessor context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                var username = context.HttpContext.User.Identity.Name;
                var emailClaim = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value;

                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                var usr = await userManager.FindByEmailAsync(emailClaim);

                if (usr == null)
                {
                    throw new AggregateException();
                }

                return usr;
            }

            throw new AggregateException("User not logged in. Cannot retrieve a user in an unauthenticated session");
        }
    }
}
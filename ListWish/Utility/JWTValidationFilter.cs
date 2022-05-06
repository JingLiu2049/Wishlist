using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Security.Claims;

namespace ListWish.Utility
{
    public class JWTValidationFilter : IAsyncActionFilter
    {
        private readonly UserManager<ListUser> userManager;
        private IMemoryCache memCache;
        public JWTValidationFilter(UserManager<ListUser> userManager, IMemoryCache memCache)
        {
            this.userManager = userManager;
            this.memCache = memCache;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var claimUserId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claimUserId is null)
            {
                await next();
                return;
            }
            string cacheKey = $"JWTValidationFilter.UserInfo.{claimUserId!.Value}";
            ListUser user = await memCache.GetOrCreateAsync(cacheKey, async e => {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(300);
                return await userManager.FindByIdAsync(claimUserId.Value);
            });

            if (user is null)
            {
                var result = new ObjectResult("UseId incorrect");
                result.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Result = result;
                return;
            }
            var jwtVerOfReq = Convert.ToInt64(context.HttpContext.User.FindFirstValue(ClaimTypes.Version));
            if (jwtVerOfReq < user.JWTTokenVersion)
            {
                var result = new ObjectResult($"Token expired");
                result.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Result = result;
                return;
            }
            await next();
        }
    }
}

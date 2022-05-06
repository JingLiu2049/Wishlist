global using ListWish.Models;
global using ListWish.DTOs.Response;
global using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace ListWish.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles ="User")]
    public class ListController : ControllerBase
    {
        private readonly UserManager<ListUser> userManager;
        private readonly ListwishDbContext db;
        private readonly IMemoryCache memoryCache;

        public ListController(UserManager<ListUser> userManager,
            ListwishDbContext db, IMemoryCache memoryCache)
        {
            this.userManager = userManager;
            this.db = db;
            this.memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<ActionResult<List<ListItemDTO>>> GetList()
        {

            ListUser user = await GetUserAsync();
            if (user is null)
            {
                return NotFound();
            }
            string cacheKey = $"WishList.UserID.{user.Id}";
            List<ListItem> items = await memoryCache.GetOrCreateAsync(cacheKey, async e => {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return await db.ListItems.AsNoTracking().Where(l => l.User == user)
                .Include(l => l.Item).ToListAsync();
            });
            return Ok(items);
        }

        [HttpPut("{itemId}")]
        public async Task<ActionResult> AddItemToList(long itemId)
        {
            ListUser user = await GetUserAsync();
            var item = await db.Items.FirstOrDefaultAsync(i=>i.Id==itemId);
            if (user is null || item is null)
            {
                return NotFound();
            }
            ListItem newListItem = new() { User=user,Item=item};
            await db.ListItems.AddAsync(newListItem);
            await db.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{ListItemId}")]
        public async Task<ActionResult> DeleteItem(long ListItemId)
        {
            ListUser user = await GetUserAsync();
            ListItem? listItem = await db.ListItems.FirstOrDefaultAsync(l => l.Id == ListItemId);

            if (user is null || listItem is null)
            {
                return NotFound();
            }
            db.ListItems.Remove(listItem);
            await db.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{ListItemId}")]
        public async Task<ActionResult> ToggleFavorite(long ListItemId)
        {
            ListUser user = await GetUserAsync();
            ListItem? listItem = await db.ListItems.FirstOrDefaultAsync(l => l.Id == ListItemId && l.User==user );

            if (user is null || listItem is null)
            {
                return NotFound();
            }
            listItem.Favorite = !listItem.Favorite;
            db.ListItems.Update(listItem);
            await db.SaveChangesAsync();
            return Ok();
        }

        
        private async Task<ListUser> GetUserAsync()
        {
            string userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            ListUser user = await userManager.FindByIdAsync(userId);
            return user;
        }

    }
}

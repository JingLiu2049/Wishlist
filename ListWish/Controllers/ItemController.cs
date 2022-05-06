global using ListWish.DTOs.Request;
using ListWish.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ListWish.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = WC.RoleAdmin+","+WC.RoleUser)]
    public class ItemController : ControllerBase
    {
        private readonly ListwishDbContext db;
        private readonly ILogger<ItemController> logger;
        private readonly IMemoryCache memoryCache; 
        
        public ItemController(ListwishDbContext db, ILogger<ItemController> logger,
            IMemoryCache memoryCache)
        {
            this.db = db;
            this.logger = logger;
            this.memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<ActionResult> GetItems()
        {
            string cacheKey = "AllItems";
            Item[] items = await memoryCache.GetOrCreateAsync(cacheKey, async e => {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return await db.Items.ToArrayAsync();
            });
            return Ok(items);
        }
        [HttpPost]
        public async Task<ActionResult> AddItem([FromForm]ItemDTO reqObj)
        {
            var file = reqObj.PhotoFile;
            if (file is null)
            {
                return BadRequest("Need a product photo");
            }
            Item newItem = new() { Name=reqObj.Item.Name,
                Price=reqObj.Item.Price,Description=reqObj.Item.Description,
                Quantity=reqObj.Item.Quantity};
            await db.Items.AddAsync(newItem);
            await db.SaveChangesAsync();
            
            if (!CheckExtension(file))
            {
                return BadRequest();
            }
            try
            {
                string filePath = await SavePhotoAsync(newItem.Id, file);
                newItem.Photo = filePath;
                db.Items.Update(newItem);
                await db.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                logger.LogError(ex.ToString());
                return BadRequest("oops, unhandled error");
            }
            return Ok();
        }
        
        [Authorize(Roles =WC.RoleAdmin)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteItem(long id)
        {
            Item? item = await db.Items.FirstOrDefaultAsync(i => i.Id == id);
            if (item is null)
            {
                return NotFound("Incorrect Item ID");
            }
            bool photoExist = System.IO.File.Exists(item.Photo);

            try
            {
                if (photoExist)
                {
                    System.IO.File.Delete(item.Photo);
                }
                db.Items.Remove(item);
                await db.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                logger.LogError(ex.ToString());
                return BadRequest("oops, unhandled error");
            }
            return Ok();
        }


        [Authorize(Roles =WC.RoleAdmin)]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateItem([FromForm]ItemDTO reqObj)
        {
            
            Item? item = await db.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id==reqObj.Item.Id);
            if (item is null)
            {
                return NotFound("Incorrect Item ID");
            }
            var file = reqObj.PhotoFile;
            if (file is not null)
            {
                if (!CheckExtension(file))
                {
                    return BadRequest();
                }
                if (System.IO.File.Exists(item.Photo))
                {
                    System.IO.File.Delete(item.Photo);
                }
                try
                {
                    string filePath = await SavePhotoAsync(item.Id, file);
                    reqObj.Item.Photo = filePath;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                    return BadRequest("oops, unhandled error");
                }
            }
            db.Items.Update(reqObj.Item);
            await db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        [Authorize(Roles = WC.RoleAdmin)]
        public async Task<ActionResult> GetLowStock()
        {
            var itemList = await db.ListItems.AsNoTracking()
                .Include(l=>l.Item)
                .Where(l => l.Favorite == true)
                .Select(l => new {ItemId= l.Item.Id,Favorite = l.Favorite,Quantity = l.Item.Quantity})
                .ToListAsync();

            var lowStockList = itemList
                .GroupBy(i => i.ItemId)
                .Select(g =>
                {
                    int? quantity = null;
                    foreach(var i in g)
                    {
                        if (quantity is null)
                        {
                            quantity = i.Quantity;
                            break;
                        }
                    }
                    return new { ItemId = g.Key, FavCount= g.Count(), Quantity=quantity };
                })
                .Where(a=>a.FavCount>a.Quantity);

            return Ok(lowStockList);
        }
        [Authorize(Roles =WC.RoleAdmin+","+WC.RoleUser)]
        [HttpGet("{id}")]
        public async Task<ActionResult> DownloadPhoto(long id)
        {
            Item item = await db.Items.FirstOrDefaultAsync(i => i.Id == id);
            if (item is null)
            {
                return NotFound();
            }

            string photoPath = item.Photo;
            string extension = Path.GetExtension(item.Photo);
            FileStream fs = new(photoPath, FileMode.Open);
            return File(fs, WC.PhotoContentType[extension]);


        }




        private bool CheckExtension(IFormFile file)
        {
            string extension = Path.GetExtension(file.FileName);
            return WC.PhotoContentType.Keys.Contains(extension);
        }

        private async Task<string> SavePhotoAsync(long id, IFormFile file)
        {
            string rootPath = Directory.GetCurrentDirectory();
            string savePath = @"\ItemPhotos\";
            string extention = Path.GetExtension(file.FileName);
            string fileName = "ItemID" + id + extention;
            string filePath = Path.Join(rootPath, savePath, fileName);

            if (!Directory.Exists(rootPath+savePath))
            {
                Directory.CreateDirectory(rootPath + savePath);
            }
            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
                await fs.FlushAsync();
            }
            return filePath;
        }
    }
}

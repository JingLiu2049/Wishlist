using ListWish.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ListWish
{
    public class ListwishDbContext:IdentityDbContext<ListUser,ListRole,long>
    {
        public ListwishDbContext(DbContextOptions<ListwishDbContext> opt) : base(opt)
        {

        }
        public DbSet<Item> Items { get; set; }
        public DbSet<ListItem> ListItems { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }
    }
}

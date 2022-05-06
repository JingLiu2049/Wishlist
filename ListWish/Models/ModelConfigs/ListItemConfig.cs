using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ListWish.Models.ModelConfigs
{
    public class ListItemConfig : IEntityTypeConfiguration<ListItem>
    {
        public void Configure(EntityTypeBuilder<ListItem> builder)
        {
            builder.HasQueryFilter(a => a.Softdelete == false);
        }
    }
}

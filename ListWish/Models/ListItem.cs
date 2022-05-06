namespace ListWish.Models
{
    public class ListItem
    {
        public long Id { get; set; }
        public ListUser User { get; set; }
        public Item Item { get; set; }
        public bool Favorite { get; set; } = false;
        public bool Softdelete { get; set; } = false;
    }
}

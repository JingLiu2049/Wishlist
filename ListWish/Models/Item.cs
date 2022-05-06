namespace ListWish.Models
{
    public class Item
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string? Photo { get; set; }
    }
}

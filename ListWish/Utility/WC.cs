namespace ListWish.Utility
{
    public class WC
    {
        public const string RoleAdmin = "Admin";
        public const string RoleUser = "User";
        public const string PhotoDir = @"\ItemPhotos\";
        public static List<string> Roles = new List<string>() { RoleAdmin, RoleUser };
        public static Dictionary<string, string> PhotoContentType { get; } = new()
        {
            { ".jpeg", "image/jpeg" },
            { ".jpg", "image/jpeg" },
            { ".png", "image/png" },
            { ".bmp", "image/bmp" },
        };
    }
}

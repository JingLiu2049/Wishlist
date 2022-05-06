global using Microsoft.AspNetCore.Identity;

namespace ListWish.Models
{
    public class ListUser:IdentityUser<long>
    {
        public long JWTTokenVersion { get; set; }
    }
}

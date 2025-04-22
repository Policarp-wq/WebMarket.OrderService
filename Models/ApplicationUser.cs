using Microsoft.AspNetCore.Identity;

namespace WebMarket.OrderService.Models
{
    public class ApplicationUser : IdentityUser<long>
    {
        public string Login { get; set; } = null!;
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpireTime { get; set; }
    }
}

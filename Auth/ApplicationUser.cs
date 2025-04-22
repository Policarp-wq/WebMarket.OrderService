namespace WebMarket.OrderService.Auth
{
    public class ApplicationUser
    {
        public string Login { get; set; } = null!;
        public AppRole Role { get; set; }
    }
}

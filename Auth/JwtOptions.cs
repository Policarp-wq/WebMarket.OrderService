namespace WebMarket.OrderService.Auth
{
    public class JwtOptions
    {
        public string KeyIssuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
    }
}

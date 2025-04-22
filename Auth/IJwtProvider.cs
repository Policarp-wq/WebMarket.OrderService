namespace WebMarket.OrderService.Auth
{
    public interface IJwtProvider
    {
        string GenerateToken(ApplicationUser user);
    }
}

namespace WebMarket.OrderService.Auth
{
    public enum AppRole
    {
        Admin,
        Customer,
        Delivery
    }
    public static class AppRoleExtension
    {
        public static string ToCustomString(this AppRole value)
        {
            return Enum.GetName(value);
        }
    }
}

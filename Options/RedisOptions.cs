namespace WebMarket.OrderService.Options
{
    public class RedisOptions
    {
        public string ConnectionString { get; set; } = null!;
        public int ConnectRetry {  get; set; }
        public int ConnectTimeout { get; set; }


    }
}

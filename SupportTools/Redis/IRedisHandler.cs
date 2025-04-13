namespace WebMarket.OrderService.SupportTools.Redis
{
    public interface IRedisHandler
    {
        public Task<bool> Save(string key, string value, TimeSpan? expiration = null);
        public Task<bool> Save(string key, int value, TimeSpan? expiration = null);
        public Task<bool> Delete(string key);
        public Task<string?> Get(string key);
        public Task<int?> GetInt(string key);
    }
}

namespace WebMarket.OrderService.SupportTools.Redis
{
    public interface IRedisHandler
    {
        public Task<bool> Save(string key, string value);
        public Task<bool> Delete(string key);
        public Task<string?> Get(string key);
        public Task<int?> GetInt(string key);
    }
}

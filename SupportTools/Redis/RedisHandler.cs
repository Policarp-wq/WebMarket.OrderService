
using StackExchange.Redis;

namespace WebMarket.OrderService.SupportTools.Redis
{
    public class RedisHandler : IRedisHandler
    {
        private readonly ILogger _logger;
        private readonly IDatabase _redis;
        public RedisHandler(IConnectionMultiplexer multiplexer, ILogger<RedisHandler> logger)
        {
            _redis = multiplexer.GetDatabase();
            _logger = logger;
        }
        public async Task<bool> Delete(string key)
        {
            _logger.LogDebug("Redis: delete key {key}", key);
            return await _redis.KeyDeleteAsync(key);
        }

        public async Task<string?> Get(string key)
        {
            _logger.LogDebug("Redis: get key {key}", key);
            var val = await _redis.StringGetAsync(key);
            if(val == RedisValue.Null)
            {
                return null;
            }
            return val;
        }

        public async Task<int?> GetInt(string key)
        {
            string? val = await Get(key);
            if(val != null && int.TryParse(val, out int res)){
                return res;
            }
            return null;
        }

        public async Task<bool> Save(string key, string value, TimeSpan? expiration = null) //expiration date
        {
            _logger.LogDebug("Redis: save key {key}", key);
            return await _redis.StringSetAsync(key, value, expiration);
        }

        public async Task<bool> Save(string key, int value, TimeSpan? expiration = null)
        {
            return await _redis.StringSetAsync(key, value.ToString(), expiration);
        }
    }
}

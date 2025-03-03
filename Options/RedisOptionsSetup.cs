using Microsoft.Extensions.Options;
using WebMarket.OrderService.Exceptions;
using WebMarket.OrderService.SupportTools;

namespace WebMarket.OrderService.Options
{
    public class RedisOptionsSetup : IConfigureOptions<RedisOptions>
    {
        private const string ConfigurationSectionName = "RedisOptions";
        private readonly IConfiguration _configuration;

        public RedisOptionsSetup(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public void Configure(RedisOptions options)
        {
            var rawConnectionString = _configuration.GetConnectionString("Redis");
            if (rawConnectionString == null)
                throw new ConfigurationException("Redis connection string was null!");
            options.ConnectionString = ConnectionStringExtractor.GetReplacedEnvVariables(rawConnectionString);
            _configuration.GetSection(ConfigurationSectionName).Bind(options);
        }
    }
}

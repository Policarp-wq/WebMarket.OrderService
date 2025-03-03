using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebMarket.OrderService.Exceptions;
using WebMarket.OrderService.SupportTools;

namespace WebMarket.OrderService.Options
{
    public class DatabaseOptionsSetup : IConfigureOptions<DatabaseOptions>
    {
        private IConfiguration _configuration;
        private const string ConfigurationSectionName = "DatabaseOptions";
        public DatabaseOptionsSetup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(DatabaseOptions options)
        {
            var rawConnectionString = _configuration.GetConnectionString("Database");
            if (rawConnectionString == null)
                throw new ConfigurationException("Database connection string was null!");
            options.ConnectionString = ConnectionStringExtractor.GetReplacedEnvVariables(rawConnectionString);
            _configuration.GetSection(ConfigurationSectionName).Bind(options);

        }
    }
}

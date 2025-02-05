using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
            options.ConnectionString = ConnectionString.GetReplacedEnvVariables(rawConnectionString,
                ["DB_SERVER", "DB_NAME", "DB_USER", "DB_PASSWORD"]);

            _configuration.GetSection(ConfigurationSectionName).Bind(options);

        }
    }
}

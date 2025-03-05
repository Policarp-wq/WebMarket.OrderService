using Serilog.Events;
using Serilog;

namespace WebMarket.OrderService.AppExtensions
{
    public static class WebApplicationBuilderExtensions
    {
        private const string SEQ_ENV = "SEQ_URL";

        public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
        {
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Extensions.Hosting", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.Extensions.Http.DefaultHttpClientFactory", LogEventLevel.Verbose)
                .WriteTo.Console(LogEventLevel.Information);
            string? sequrl = Environment.GetEnvironmentVariable(SEQ_ENV);
            if (sequrl != null)
            {
                loggerConfig.WriteTo.Seq(sequrl, LogEventLevel.Debug);
            }
            builder.Logging.ClearProviders();
            Log.Logger = loggerConfig
                .CreateLogger();
            builder.Services.AddSerilog();

            return builder;
        }
    }
}

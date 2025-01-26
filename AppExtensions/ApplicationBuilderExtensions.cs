namespace WebMarket.OrderService.AppExtensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder AddSwagger(this IApplicationBuilder builder, bool isDevelopment)
        {
            if (isDevelopment)
            {
                builder.UseSwagger();
                builder.UseSwaggerUI();
            }
            return builder;
        }
    }
}

namespace WebMarket.OrderService.Options
{
    public class DatabaseOptions
    {
        public string ConnectionString { get; set; } = null!;
        public int MaxRetries { get; set; }
        public int CommandTimeout { get; set; } 
        public bool EnabledDetailedErrors { get; set; }   
        public bool EnabledSensitiveDataLog { get; set; }    
    }
}

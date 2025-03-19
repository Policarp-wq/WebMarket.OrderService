namespace WebMarket.OrderService.Exceptions
{
    public class ServerException : Exception
    {
        public ServerException(string msg, Exception? inner = null) : base(msg, inner) { }
    }
}

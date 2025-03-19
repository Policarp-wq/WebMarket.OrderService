namespace WebMarket.OrderService.Exceptions
{
    public class PrivateServerException : ServerException
    {
        public PrivateServerException(string msg, Exception? inner = null) : base(msg, inner)
        {
        }
    }
}

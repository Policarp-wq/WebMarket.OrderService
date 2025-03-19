namespace WebMarket.OrderService.Exceptions
{
    public class InvalidArgumentException : ServerException
    {
        public InvalidArgumentException(string msg, Exception? inner = null) : base(msg, inner)
        {
        }
    }
}

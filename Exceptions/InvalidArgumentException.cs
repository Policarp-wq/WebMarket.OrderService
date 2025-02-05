namespace WebMarket.OrderService.Exceptions
{
    public class InvalidArgumentException : ServerException
    {
        public InvalidArgumentException(string msg) : base(msg)
        {
        }
    }
}

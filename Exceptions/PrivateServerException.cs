namespace WebMarket.OrderService.Exceptions
{
    public class PrivateServerException : ServerException
    {
        public PrivateServerException(string msg) : base(msg)
        {
        }
    }
}

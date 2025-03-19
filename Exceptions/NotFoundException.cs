namespace WebMarket.OrderService.Exceptions
{
    public class NotFoundException : ServerException
    {
        public NotFoundException(string msg, Exception? inner = null) : base(msg, inner)
        {
        }
    }
}

namespace WebMarket.OrderService.Exceptions
{
    public class NotFoundException : ServerException
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}

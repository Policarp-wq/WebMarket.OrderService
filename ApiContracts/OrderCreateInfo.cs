namespace WebMarket.OrderService.ApiContracts
{
    public record OrderCreateInfo(int CustomerID, int ProductID, int DeliveryPointID, int ProductOwnerId);
    
    
}

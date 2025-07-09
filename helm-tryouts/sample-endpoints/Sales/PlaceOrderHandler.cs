using NServiceBus;
using Messages;

namespace Sales;

public class PlaceOrderHandler(ILogger<PlaceOrderHandler> logger) : IHandleMessages<PlaceOrder>
{
    public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
    {
        logger.LogInformation("Received PlaceOrder for OrderId: {OrderId}, Product: {Product}, Amount: {Amount}", 
            message.OrderId, message.Product, message.Amount);

        // Simulate order processing
        await Task.Delay(1000);

        // Publish OrderPlaced event
        await context.Publish(new OrderPlaced
        {
            OrderId = message.OrderId,
            Product = message.Product,
            Amount = message.Amount,
            PlacedAt = DateTime.UtcNow
        });

        logger.LogInformation("Order {OrderId} placed successfully", message.OrderId);
    }
}

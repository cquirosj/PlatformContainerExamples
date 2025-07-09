using NServiceBus;
using Messages;

namespace Billing;

public class ShippingPolicy(ILogger<ShippingPolicy> logger) : 
    Saga<ShippingPolicy.ShippingPolicyData>,
    IAmStartedByMessages<OrderBilled>,
    IHandleMessages<PaymentFailed>
{
    public async Task Handle(OrderBilled message, IMessageHandlerContext context)
    {
        logger.LogInformation("Order {OrderId} billed successfully. Initiating shipping.", message.OrderId);
        
        Data.OrderId = message.OrderId;
        Data.Amount = message.Amount;
        Data.InvoiceNumber = message.InvoiceNumber;
        
        // Send order to shipping
        await context.Send(new ShipOrder
        {
            OrderId = message.OrderId,
            ShippingAddress = "123 Customer Street, City, State"
        });
        
        MarkAsComplete();
    }

    public Task Handle(PaymentFailed message, IMessageHandlerContext context)
    {
        logger.LogWarning("Payment failed for Order {OrderId}. Order will not be shipped.", message.OrderId);
        
        // Could implement retry logic or notification here
        MarkAsComplete();
        
        return Task.CompletedTask;
    }

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ShippingPolicyData> mapper)
    {
        mapper.MapSaga(saga => saga.OrderId)
            .ToMessage<OrderBilled>(message => message.OrderId)
            .ToMessage<PaymentFailed>(message => message.OrderId);
    }

    public class ShippingPolicyData : ContainSagaData
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
    }
}

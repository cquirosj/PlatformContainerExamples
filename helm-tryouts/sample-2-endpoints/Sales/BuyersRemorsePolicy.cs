using NServiceBus;
using Messages;

namespace Sales;

public class BuyersRemorsePolicy(ILogger<BuyersRemorsePolicy> logger) : 
    Saga<BuyersRemorsePolicy.BuyersRemorseData>,
    IAmStartedByMessages<OrderPlaced>
{
    public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
    {
        logger.LogInformation("Starting buyers remorse period for Order {OrderId}", message.OrderId);
        
        Data.OrderId = message.OrderId;
        Data.Amount = message.Amount;
        
        // Start 20-second buyers remorse period (shortened for demo)
        await RequestTimeout(context, TimeSpan.FromSeconds(5), new BuyersRemorseTimeout());
    }

    public async Task Timeout(BuyersRemorseTimeout timeout, IMessageHandlerContext context)
    {
        logger.LogInformation("Buyers remorse period expired for Order {OrderId}. Proceeding to billing.", Data.OrderId);
        
        // Send to billing for payment processing
        await context.Send(new ProcessPayment
        {
            OrderId = Data.OrderId,
            Amount = Data.Amount,
            PaymentMethod = "CreditCard"
        });
        
        MarkAsComplete();
    }

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<BuyersRemorseData> mapper)
    {
        mapper.MapSaga(saga => saga.OrderId)
            .ToMessage<OrderPlaced>(message => message.OrderId);
    }

    public class BuyersRemorseData : ContainSagaData
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class BuyersRemorseTimeout
    {
    }
}

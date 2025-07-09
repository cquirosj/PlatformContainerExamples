using NServiceBus;
using Messages;

namespace Shipping;

public class ShipOrderWorkflow(ILogger<ShipOrderWorkflow> logger) : 
    Saga<ShipOrderWorkflow.ShipOrderData>,
    IAmStartedByMessages<ShipOrder>,
    IHandleMessages<ShipmentAcceptedByMaple>,
    IHandleMessages<ShipmentAcceptedByAlpine>,
    IHandleTimeouts<ShipOrderWorkflow.ShippingEscalation>
{
    public async Task Handle(ShipOrder message, IMessageHandlerContext context)
    {
        logger.LogInformation("ShipOrderWorkflow for Order [{OrderId}] - Trying Maple first.", Data.OrderId);

        Data.OrderId = message.OrderId;
        Data.ShippingAddress = message.ShippingAddress;

        // Execute order to ship with Maple
        await context.Send(new ShipWithMaple { OrderId = Data.OrderId });

        // Add timeout to escalate if Maple did not ship in time.
        await RequestTimeout(context, TimeSpan.FromSeconds(10), new ShippingEscalation());
    }

    public Task Handle(ShipmentAcceptedByMaple message, IMessageHandlerContext context)
    {
        if (!Data.ShipmentOrderSentToAlpine)
        {
            logger.LogInformation("Order [{OrderId}] - Successfully shipped with Maple", Data.OrderId);

            Data.ShipmentAcceptedByMaple = true;
            return CompleteShipment(context, "Maple", "MAPLE-" + Data.OrderId[..8]);
        }

        return Task.CompletedTask;
    }

    public Task Handle(ShipmentAcceptedByAlpine message, IMessageHandlerContext context)
    {
        logger.LogInformation("Order [{OrderId}] - Successfully shipped with Alpine", Data.OrderId);

        Data.ShipmentAcceptedByAlpine = true;
        return CompleteShipment(context, "Alpine", "ALPINE-" + Data.OrderId[..8]);
    }

    public async Task Timeout(ShippingEscalation timeout, IMessageHandlerContext context)
    {
        if (!Data.ShipmentAcceptedByMaple)
        {
            if (!Data.ShipmentOrderSentToAlpine)
            {
                logger.LogInformation("Order [{OrderId}] - No answer from Maple, let's try Alpine.", Data.OrderId);
                Data.ShipmentOrderSentToAlpine = true;
                await context.Send(new ShipWithAlpine { OrderId = Data.OrderId });
                await RequestTimeout(context, TimeSpan.FromSeconds(10), new ShippingEscalation());
            }
            else if (!Data.ShipmentAcceptedByAlpine) // No response from Maple nor Alpine
            {
                logger.LogWarning("Order [{OrderId}] - No answer from Maple/Alpine. We need to escalate!", Data.OrderId);

                // escalate to Warehouse Manager!
                await context.Publish(new ShipmentFailed
                {
                    OrderId = Data.OrderId,
                    Reason = "Both Maple and Alpine failed to respond",
                    FailedAt = DateTime.UtcNow
                });

                MarkAsComplete();
            }
        }
    }

    private async Task CompleteShipment(IMessageHandlerContext context, string carrier, string trackingNumber)
    {
        await context.Publish(new OrderShipped
        {
            OrderId = Data.OrderId,
            Carrier = carrier,
            TrackingNumber = trackingNumber,
            ShippedAt = DateTime.UtcNow
        });

        MarkAsComplete();
    }

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ShipOrderData> mapper)
    {
        mapper.MapSaga(saga => saga.OrderId)
            .ToMessage<ShipOrder>(message => message.OrderId);
    }

    public class ShipOrderData : ContainSagaData
    {
        public string OrderId { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public bool ShipmentAcceptedByMaple { get; set; }
        public bool ShipmentOrderSentToAlpine { get; set; }
        public bool ShipmentAcceptedByAlpine { get; set; }
    }

    public class ShippingEscalation
    {
    }
}

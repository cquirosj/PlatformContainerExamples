using NServiceBus;
using Messages;

namespace Shipping;

public class ShipWithAlpineHandler(ILogger<ShipWithAlpineHandler> logger) : IHandleMessages<ShipWithAlpine>
{
    const int MaximumTimeAlpineMightRespond = 8;

    public async Task Handle(ShipWithAlpine message, IMessageHandlerContext context)
    {
        var waitingTime = Random.Shared.Next(MaximumTimeAlpineMightRespond);

        logger.LogInformation("ShipWithAlpineHandler: Delaying Order [{OrderId}] {WaitingTime} seconds.", 
            message.OrderId, waitingTime);

        await Task.Delay(waitingTime * 1000);

        await context.Reply(new ShipmentAcceptedByAlpine { OrderId = message.OrderId });
    }
}

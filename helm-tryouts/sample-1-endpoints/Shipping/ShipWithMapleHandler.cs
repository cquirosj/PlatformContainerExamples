using NServiceBus;
using Messages;

namespace Shipping;

public class ShipWithMapleHandler(ILogger<ShipWithMapleHandler> logger) : IHandleMessages<ShipWithMaple>
{
    const int MaximumTimeMapleMightRespond = 15;

    public async Task Handle(ShipWithMaple message, IMessageHandlerContext context)
    {
        var waitingTime = Random.Shared.Next(MaximumTimeMapleMightRespond);

        logger.LogInformation("ShipWithMapleHandler: Delaying Order [{OrderId}] {WaitingTime} seconds.", 
            message.OrderId, waitingTime);

        await Task.Delay(waitingTime * 1000);

        await context.Reply(new ShipmentAcceptedByMaple { OrderId = message.OrderId });
    }
}

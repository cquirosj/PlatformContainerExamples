using NServiceBus;
using Messages;

namespace Billing;

public class ProcessPaymentHandler(ILogger<ProcessPaymentHandler> logger) : IHandleMessages<ProcessPayment>
{
    public async Task Handle(ProcessPayment message, IMessageHandlerContext context)
    {
        logger.LogInformation("Processing payment for OrderId: {OrderId}, Amount: {Amount}", 
            message.OrderId, message.Amount);

        // Simulate payment processing
        await Task.Delay(2000);

        // Random success/failure for demo
        var success = Random.Shared.Next(0, 10) > 1; // 90% success rate

        if (success)
        {
            var invoiceNumber = $"INV-{message.OrderId[..8]}";
            
            await context.Publish(new OrderBilled
            {
                OrderId = message.OrderId,
                Amount = message.Amount,
                InvoiceNumber = invoiceNumber,
                BilledAt = DateTime.UtcNow
            });

            logger.LogInformation("Payment processed successfully for Order {OrderId}. Invoice: {InvoiceNumber}", 
                message.OrderId, invoiceNumber);
        }
        else
        {
            await context.Publish(new PaymentFailed
            {
                OrderId = message.OrderId,
                Reason = "Payment declined by processor",
                FailedAt = DateTime.UtcNow
            });

            logger.LogWarning("Payment failed for Order {OrderId}", message.OrderId);
        }
    }
}

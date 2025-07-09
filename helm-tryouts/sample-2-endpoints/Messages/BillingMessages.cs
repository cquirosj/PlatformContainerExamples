using NServiceBus;

namespace Messages;

// Billing Domain Messages
public class ProcessPayment : ICommand
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}

public class OrderBilled : IEvent
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime BilledAt { get; set; }
}

public class PaymentFailed : IEvent
{
    public string OrderId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
}

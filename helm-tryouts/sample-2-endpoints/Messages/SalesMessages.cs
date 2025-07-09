using NServiceBus;

namespace Messages;

// Sales Domain Messages
public class PlaceOrder : ICommand
{
    public string OrderId { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class OrderPlaced : IEvent
{
    public string OrderId { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PlacedAt { get; set; }
}

public class OrderCancelled : IEvent
{
    public string OrderId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; }
}

using NServiceBus;

namespace Messages;

// Shipping Domain Messages
public class ShipOrder : ICommand
{
    public string OrderId { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
}

public class ShipWithMaple : ICommand
{
    public string OrderId { get; set; } = string.Empty;
}

public class ShipWithAlpine : ICommand
{
    public string OrderId { get; set; } = string.Empty;
}

public class ShipmentAcceptedByMaple : IMessage
{
    public string OrderId { get; set; } = string.Empty;
}

public class ShipmentAcceptedByAlpine : IMessage
{
    public string OrderId { get; set; } = string.Empty;
}

public class ShipmentFailed : IEvent
{
    public string OrderId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
}

public class OrderShipped : IEvent
{
    public string OrderId { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public DateTime ShippedAt { get; set; }
}

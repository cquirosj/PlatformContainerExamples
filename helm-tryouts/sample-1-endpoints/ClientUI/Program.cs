using NServiceBus;
using Messages;

var endpointConfiguration = new EndpointConfiguration("ClientUI");

// Configure RabbitMQ transport
var transport = new RabbitMQTransport(RoutingTopology.Conventional, "host=localhost;username=guest;password=guest");
endpointConfiguration.UseTransport(transport);

// Configure routing
var routing = transport.Routing();
routing.RouteToEndpoint(typeof(PlaceOrder), "Sales");

// Configure persistence
endpointConfiguration.UsePersistence<LearningPersistence>();

// Configure serialization
endpointConfiguration.UseSerialization<SystemJsonSerializer>();

// Enable installers for development
endpointConfiguration.EnableInstallers();

Console.Title = "Client UI";
Console.WriteLine("Starting Client UI...");

var endpointInstance = await Endpoint.Start(endpointConfiguration);

Console.WriteLine();
Console.WriteLine("Client UI started. Press 'P' to place an order, 'Q' to quit.");
Console.WriteLine();

while (true)
{
    var key = Console.ReadKey(true);
    
    switch (key.Key)
    {
        case ConsoleKey.P:
            await PlaceOrder(endpointInstance);
            break;
        case ConsoleKey.Q:
            goto shutdown;
        default:
            Console.WriteLine("Press 'P' to place an order, 'Q' to quit.");
            break;
    }
}

shutdown:
await endpointInstance.Stop();

static async Task PlaceOrder(IEndpointInstance endpoint)
{
    var orderId = Guid.NewGuid().ToString();
    var products = new[] { "Widget", "Gadget", "Doohickey", "Thingamajig" };
    var product = products[Random.Shared.Next(products.Length)];
    var amount = Random.Shared.Next(10, 500);

    var order = new PlaceOrder
    {
        OrderId = orderId,
        Product = product,
        Amount = amount
    };

    await endpoint.Send(order);
    
    Console.WriteLine($"Order placed: {product} for ${amount} (OrderId: {orderId[..8]})");
}

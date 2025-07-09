using NServiceBus;
using NServiceBus.ServicePlatform.Connector;
using Messages;

var endpointConfiguration = new EndpointConfiguration("Shipping");

// Configure RabbitMQ transport
var transport = new RabbitMQTransport(RoutingTopology.Conventional, "host=localhost");
endpointConfiguration.UseTransport(transport);

// Configure routing - keep commands within Shipping endpoint
var routing = transport.Routing();
routing.RouteToEndpoint(typeof(ShipWithMaple), "Shipping");
routing.RouteToEndpoint(typeof(ShipWithAlpine), "Shipping");

// Configure Platform Connector for Shipping domain
var platformConnection = new ServicePlatformConnectionConfiguration
{
    ErrorQueue = "error",
    Heartbeats = new ServicePlatformHeartbeatConfiguration
    {
        Enabled = true,
        HeartbeatsQueue = "Particular.ServiceControl"
    },
    CustomChecks = new ServicePlatformCustomChecksConfiguration
    {
        Enabled = true,
        CustomChecksQueue = "Particular.ServiceControl"
    },
    MessageAudit = new ServicePlatformMessageAuditConfiguration
    {
        Enabled = true,
        AuditQueue = "shipping.audit" // Shipping-specific audit queue
    },
    SagaAudit = new ServicePlatformSagaAuditConfiguration
    {
        Enabled = true,
        SagaAuditQueue = "shipping.audit" // Shipping saga audits
    },
    Metrics = new ServicePlatformMetricsConfiguration
    {
        Enabled = true,
        MetricsQueue = "Particular.Monitoring",
        Interval = TimeSpan.FromSeconds(5),
        InstanceId = "Shipping-Instance"
    }
};

endpointConfiguration.ConnectToServicePlatform(platformConnection);

// Configure persistence
endpointConfiguration.UsePersistence<LearningPersistence>();

// Configure serialization
endpointConfiguration.UseSerialization<SystemJsonSerializer>();

// Enable installers for development
endpointConfiguration.EnableInstallers();

Console.Title = "Shipping Endpoint";
Console.WriteLine("Starting Shipping endpoint...");

var endpointInstance = await Endpoint.Start(endpointConfiguration);

Console.WriteLine("Shipping endpoint started. Press any key to stop.");
Console.ReadKey();

await endpointInstance.Stop();

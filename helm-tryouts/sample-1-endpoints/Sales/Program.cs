using NServiceBus;
using NServiceBus.ServicePlatform.Connector;
using Messages;

var endpointConfiguration = new EndpointConfiguration("Sales");

// Configure RabbitMQ transport
var transport = new RabbitMQTransport(RoutingTopology.Conventional, "host=localhost");
endpointConfiguration.UseTransport(transport);

// Configure routing
var routing = transport.Routing();
routing.RouteToEndpoint(typeof(ProcessPayment), "Billing");

// Configure Platform Connector with default audit queue
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
        Enabled = true
        // Using default audit queue (no custom AuditQueue specified)
    },
    SagaAudit = new ServicePlatformSagaAuditConfiguration
    {
        Enabled = true
        // Using default audit queue (no custom SagaAuditQueue specified)
    },
    Metrics = new ServicePlatformMetricsConfiguration
    {
        Enabled = true,
        MetricsQueue = "Particular.Monitoring",
        Interval = TimeSpan.FromSeconds(5),
        InstanceId = "Sales-Instance"
    }
};

endpointConfiguration.ConnectToServicePlatform(platformConnection);

// Configure persistence
endpointConfiguration.UsePersistence<LearningPersistence>();

// Configure serialization
endpointConfiguration.UseSerialization<SystemJsonSerializer>();

// Enable installers for development
endpointConfiguration.EnableInstallers();

Console.Title = "Sales Endpoint";
Console.WriteLine("Starting Sales endpoint...");

var endpointInstance = await Endpoint.Start(endpointConfiguration);

Console.WriteLine("Sales endpoint started. Press any key to stop.");
Console.ReadKey();

await endpointInstance.Stop();

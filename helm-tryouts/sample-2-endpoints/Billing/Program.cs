using NServiceBus;
using NServiceBus.ServicePlatform.Connector;
using Messages;

var endpointConfiguration = new EndpointConfiguration("Billing");

// Configure RabbitMQ transport
var transport = new RabbitMQTransport(RoutingTopology.Conventional, "host=localhost;username=guest;password=guest");
endpointConfiguration.UseTransport(transport);

// Configure routing
var routing = transport.Routing();
routing.RouteToEndpoint(typeof(ShipOrder), "Shipping");

// Configure Platform Connector for Billing domain
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
        AuditQueue = "billing.audit" // Billing-specific audit queue
    },
    SagaAudit = new ServicePlatformSagaAuditConfiguration
    {
        Enabled = true,
        SagaAuditQueue = "billing.audit" // Billing saga audits
    },
    Metrics = new ServicePlatformMetricsConfiguration
    {
        Enabled = true,
        MetricsQueue = "Particular.Monitoring",
        Interval = TimeSpan.FromSeconds(5),
        InstanceId = "Billing-Instance"
    }
};

endpointConfiguration.ConnectToServicePlatform(platformConnection);

// Configure persistence
endpointConfiguration.UsePersistence<LearningPersistence>();

// Configure serialization
endpointConfiguration.UseSerialization<SystemJsonSerializer>();

// Enable installers for development
endpointConfiguration.EnableInstallers();

Console.Title = "Billing Endpoint";
Console.WriteLine("Starting Billing endpoint...");

var endpointInstance = await Endpoint.Start(endpointConfiguration);

Console.WriteLine("Billing endpoint started. Press any key to stop.");
Console.ReadKey();

await endpointInstance.Stop();

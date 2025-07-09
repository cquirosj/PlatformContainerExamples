using Microsoft.Extensions.Hosting;
using NServiceBus;
using System;
using System.Threading.Tasks;
using Messages;

var endpointName = "Shipping";

Console.Title = endpointName;

var builder = Host.CreateApplicationBuilder(args);

var endpointConfiguration = new EndpointConfiguration(endpointName);
endpointConfiguration.EnableInstallers();

endpointConfiguration.UseSerialization<SystemJsonSerializer>();

var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
transport.ConnectionString("host=localhost;username=guest;password=guest");
var routing = transport.UseConventionalRoutingTopology(QueueType.Quorum).Routing();

endpointConfiguration.UsePersistence<LearningPersistence>();

routing.RouteToEndpoint(typeof(ShipOrder), "Shipping");
routing.RouteToEndpoint(typeof(ShipWithMaple), "Shipping");
routing.RouteToEndpoint(typeof(ShipWithAlpine), "Shipping");

endpointConfiguration.SendFailedMessagesTo("error");
endpointConfiguration.AuditProcessedMessagesTo("audit");

var servicePlatformConnection = ServicePlatformConnectionConfiguration.Parse(@"{
    ""Heartbeats"": {
        ""Enabled"": true,
        ""HeartbeatsQueue"": ""Particular.ServiceControl"",
        ""Frequency"": ""00:00:10"",
        ""TimeToLive"": ""00:00:40""
    },
    ""CustomChecks"": {
        ""Enabled"": true,
        ""CustomChecksQueue"": ""Particular.ServiceControl""
    },
    ""ErrorQueue"": ""error"",
    ""SagaAudit"": {
        ""Enabled"": true,
        ""SagaAuditQueue"": ""audit""
    },
    ""MessageAudit"": {
        ""Enabled"": true,
        ""AuditQueue"": ""audit""
    },
    ""Metrics"": {
        ""Enabled"": true,
        ""MetricsQueue"": ""Particular.Monitoring"",
        ""Interval"": ""00:00:01""
    }
}");

endpointConfiguration.ConnectToServicePlatform(servicePlatformConnection);

// Decrease the default delayed delivery interval so that we don't
// have to wait too long for the message to be moved to the error queue
var recoverability = endpointConfiguration.Recoverability();
recoverability.Delayed(
    delayed => { delayed.TimeIncrease(TimeSpan.FromSeconds(2)); }
);

builder.UseNServiceBus(endpointConfiguration);

await builder.Build().RunAsync();
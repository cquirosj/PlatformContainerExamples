using ClientUI;
using Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var endpointName = "ClientUI";

Console.Title = endpointName;

var builder = Host.CreateApplicationBuilder(args);

var endpointConfiguration = new EndpointConfiguration(endpointName);
endpointConfiguration.EnableInstallers();
endpointConfiguration.UseSerialization<SystemJsonSerializer>();


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

var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
transport.ConnectionString("host=localhost;username=guest;password=guest");
var routing = transport.UseConventionalRoutingTopology(QueueType.Quorum).Routing();

routing.RouteToEndpoint(typeof(PlaceOrder), "Sales");

builder.UseNServiceBus(endpointConfiguration);

builder.Services.AddHostedService<InputLoopService>();

await builder.Build().RunAsync();
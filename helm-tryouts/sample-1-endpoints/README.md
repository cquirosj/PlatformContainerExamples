# Sample 1 Endpoints - Single Audit Instance

This sample demonstrates a simple NServiceBus solution using the default audit queue configuration, designed to work with Sample 1's single audit instance setup.

## Overview

This is a retail order processing system with the following endpoints:
- **Sales**: Handles order placement and buyer's remorse policy
- **Billing**: Processes payments and coordinates with shipping
- **Shipping**: Manages shipment with external carriers
- **ClientUI**: Console application for placing orders

## Key Configuration Differences from Sample 2

### Simplified Platform Connector Configuration
All endpoints use the **default audit queue** instead of domain-specific queues:

```csharp
MessageAudit = new ServicePlatformMessageAuditConfiguration
{
    Enabled = true
    // No custom AuditQueue - uses default "audit" queue
},
SagaAudit = new ServicePlatformSagaAuditConfiguration
{
    Enabled = true
    // No custom SagaAuditQueue - uses default "audit" queue
}
```

### Single Audit Flow
- All endpoints → **default `audit` queue** → Single audit instance
- Simplified monitoring and troubleshooting
- Shared audit data storage in single RavenDB

## Prerequisites

Ensure you have the Sample 1 infrastructure running:

```bash
# Start single audit infrastructure
cd ../../docker-compose
docker compose -f compose-single-audit.yml up -d

# Deploy Sample 1 platform
cd ../helm
helm install particular-platform --create-namespace --namespace particular-platform -f ../helm-tryouts/overrides-sample-1.yaml .
## Running the Sample

### Option 1: Run All Endpoints Together
```bash
cd sample-1-endpoints
dotnet run --project RetailDemo.sln
```

### Option 2: Run Endpoints Individually
Open separate terminals for each endpoint:

```bash
# Terminal 1 - Sales
cd sample-1-endpoints
dotnet run --project Sales/Sales.csproj

# Terminal 2 - Billing  
cd sample-1-endpoints
dotnet run --project Billing/Billing.csproj

# Terminal 3 - Shipping
cd sample-1-endpoints
dotnet run --project Shipping/Shipping.csproj

# Terminal 4 - ClientUI
cd sample-1-endpoints
dotnet run --project ClientUI/ClientUI.csproj
```

## Using the Sample

1. **Start all endpoints** (using either option above)
2. **In the ClientUI terminal**, press 'P' to place orders
3. **Monitor the flow** in other terminal windows
4. **Check ServicePulse** at http://servicepulse.local to see:
   - Message throughput across all endpoints
   - All audit data in a single view
   - Error handling and retries

## Message Flow

```
ClientUI → PlaceOrder → Sales
Sales → ProcessPayment → Billing
Billing → Ship → Shipping
```

### Saga Orchestration
- **Sales**: Buyer's remorse policy (10-minute timeout)
- **Billing**: Payment processing and shipping coordination
- **Shipping**: External carrier coordination (Maple/Alpine)

## Audit Data

All endpoints send audit messages to the **default `audit` queue**, which is processed by the single audit instance and stored in the shared RavenDB database at `localhost:8080`.

## Comparison with Sample 2

| Aspect | Sample 1 (This) | Sample 2 |
|--------|------------------|----------|
| **Audit Queues** | Single `audit` queue | Domain-specific queues |
| **Configuration** | Simplified | Complex |
| **Data Separation** | None | Full domain isolation |
| **Monitoring** | Single view | Domain-separated views |
| **Best For** | Development, small prod | Enterprise, large prod |

## Troubleshooting

### Common Issues
1. **Endpoints can't connect**: Ensure RabbitMQ is running (`docker-compose ps`)
2. **No audit data**: Check that the single audit instance is running in Kubernetes
3. **ServicePulse not accessible**: Verify ingress and /etc/hosts configuration

### Useful Commands
```bash
# Check infrastructure
docker compose -f ../../docker-compose/compose-single-audit.yml ps

# Check Kubernetes
kubectl get pods -n particular-platform

# View logs
kubectl logs -n particular-platform deployment/particular-platform-audit
```

## Next Steps

- Generate traffic using ClientUI ('P' to place orders)
- Monitor message flow in ServicePulse
- Experiment with error scenarios (stop endpoints, etc.)
- Compare with Sample 2's domain-separated approach

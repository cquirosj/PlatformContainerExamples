# Sample NServiceBus Endpoints for Multi-Audit Platform Demo

This sample demonstrates a retail order processing system with three endpoints (Sales, Billing, Shipping) that connect to the Particular Service Platform with domain-specific audit instances.

## Business Scenario

Based on the NServiceBus saga tutorial, this system processes retail orders through multiple stages:

1. **Sales Endpoint**: Handles order placement and customer management
2. **Billing Endpoint**: Processes payments and invoicing  
3. **Shipping Endpoint**: Manages shipment coordination with external carriers

Each endpoint sends audit data to domain-specific audit instances to enable:
- **Business Domain Separation**: Different teams can monitor their own processes
- **Compliance Requirements**: Financial data (Billing) may need different retention policies
- **Performance Isolation**: High-volume shipping operations don't impact sales monitoring

## Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ Sales Endpoint  │    │ Billing         │    │ Shipping        │
│                 │    │ Endpoint        │    │ Endpoint        │
│ - PlaceOrder    │───▶│ - ProcessBill   │───▶│ - ShipOrder     │
│ - BuyersRemorse │    │ - SendInvoice   │    │ - Integration   │
└─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘
          │                      │                      │
          ▼                      ▼                      ▼
    sales.audit            billing.audit          shipping.audit
      queue                    queue                  queue
          │                      │                      │
          ▼                      ▼                      ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ Sales Audit     │    │ Billing Audit   │    │ Shipping Audit  │
│ Instance        │    │ Instance        │    │ Instance        │
│ (RavenDB:8081)  │    │ (RavenDB:8082)  │    │ (RavenDB:8083)  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
          │                      │                      │
          └──────────┬───────────┴──────────────────────┘
                     ▼
          ┌─────────────────┐
          │ Error Instance  │ ◄─── ServicePulse
          │ (RavenDB:8084)  │ ◄─── ServiceInsight
          └─────────────────┘
```

## Prerequisites

1. **Infrastructure**: Start the infrastructure services first:
   ```shell
   cd ../docker-compose
   docker compose -f compose-infrastructure.yml up -d
   ```

2. **Platform**: Deploy the Helm chart with multiple audit instances:
   ```shell
   cd ../helm
   helm install particular-platform-multi --create-namespace --namespace particular-platform-multi -f overrides-sample-2.yaml .
   ```

## Running the Sample

### Option 1: Run All Endpoints
```shell
dotnet run --project RetailDemo.sln
```

### Option 2: Run Individual Endpoints
```shell
# Terminal 1 - Sales
cd Sales
dotnet run

# Terminal 2 - Billing  
cd Billing
dotnet run

# Terminal 3 - Shipping
cd Shipping
dotnet run

# Terminal 4 - Client UI
cd ClientUI
dotnet run
```

## Key Features

### Platform Connector Configuration
Each endpoint uses the Platform Connector to automatically configure:
- **Error Queue**: `error` 
- **Message Auditing**: Domain-specific audit queues
- **Saga Auditing**: Included in audit data
- **Endpoint Heartbeats**: Sent to `Particular.ServiceControl`
- **Custom Checks**: Health monitoring
- **Performance Metrics**: Sent to `Particular.Monitoring`

### Domain-Specific Audit Queues
- **Sales**: `sales.audit` → Sales Audit Instance (RavenDB:8081)
- **Billing**: `billing.audit` → Billing Audit Instance (RavenDB:8082)  
- **Shipping**: `shipping.audit` → Shipping Audit Instance (RavenDB:8083)

### RabbitMQ Transport
All endpoints connect to RabbitMQ running in Docker Compose infrastructure:
- **Host**: `localhost:5672`
- **Management UI**: `http://localhost:15672` (guest/guest)

## Business Process Flow

1. **Customer places order** via ClientUI
2. **Sales endpoint** processes order placement (20s buyers remorse period)
3. **Billing endpoint** processes payment and generates invoice
4. **Shipping endpoint** coordinates with external carriers (Maple/Alpine)
5. **Order completion** with full audit trail across all domains

## Monitoring

- **ServicePulse**: `http://servicepulse-multi.local` - Unified view of all instances
- **ServiceInsight**: Connect to `http://localhost:33333/api` - Message flow visualization
- **RabbitMQ Management**: `http://localhost:15672` - Queue monitoring

## Sample Messages

The system processes several message types:
- `PlaceOrder`, `OrderPlaced` (Sales domain)
- `ProcessPayment`, `OrderBilled` (Billing domain)  
- `ShipOrder`, `ShipmentAccepted` (Shipping domain)

Each message flows through the system and is audited in the appropriate domain-specific audit instance.

## Production Considerations

1. **Retention Policies**: Billing data may need longer retention for compliance
2. **Access Control**: Different teams should only access their domain data
3. **Performance**: High shipping volume isolated from sales/billing monitoring
4. **Backup**: Each audit database backed up according to domain requirements

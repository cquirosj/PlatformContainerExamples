# Multiple Audit Instances with Separate RavenDB Infrastructure

This example demonstrates how to deploy the Particular Platform with multiple audit instances, each using a dedicated RavenDB database. This pattern is useful for:

- **Business Domain Separation**: Different teams (Sales, Billing, Shipping) with isolated audit data
- **Different Retention Policies**: Each business domain can have its own data retention requirements
- **Scalability**: Distribute audit load across multiple instances and databases
- **Performance Isolation**: Heavy audit traffic from one domain won't impact others

## Infrastructure Setup

### 1. Start the Infrastructure Services

First, start the infrastructure services (RabbitMQ + multiple RavenDB instances):

```shell
cd ../docker-compose
docker compose -f compose-infrastructure.yml up -d
```

This will start:
- **RabbitMQ**: `localhost:5672` (Management UI at `localhost:15672`)
- **RavenDB Sales**: `localhost:8081` 
- **RavenDB Billing**: `localhost:8082`
- **RavenDB Shipping**: `localhost:8083`
- **RavenDB Error**: `localhost:8084`

### 2. Verify Infrastructure is Running

```shell
# Check all containers are healthy
docker compose -f compose-infrastructure.yml ps

# Test RabbitMQ connectivity
curl -u guest:guest http://localhost:15672/api/overview

# Test RavenDB instances
curl http://localhost:8081/admin/stats  # Sales
curl http://localhost:8082/admin/stats  # Billing
curl http://localhost:8083/admin/stats  # Shipping
curl http://localhost:8084/admin/stats  # Error
```

## Helm Deployment

### 3. Deploy the Particular Platform

```shell
cd ../helm

# Add servicepulse-multi.local to your hosts file (optional)
echo "127.0.0.1 servicepulse-multi.local" | sudo tee -a /etc/hosts

# Deploy using the multi-audit configuration
helm install particular-platform-multi --create-namespace --namespace particular-platform-multi -f ../helm-tryouts/overrides-sample-2.yaml .
```

### 4. Verify Deployment

```shell
# Check all pods are running
kubectl get pods -n particular-platform-multi

# Check services
kubectl get services -n particular-platform-multi

# Check ingress
kubectl get ingress -n particular-platform-multi
```

You should see:
- **1 Error instance**: `particular-platform-multi-error`
- **3 Audit instances**: 
  - `particular-platform-multi-audit-sales`
  - `particular-platform-multi-audit-billing`
  - `particular-platform-multi-audit-shipping`
- **1 Monitor instance**: `particular-platform-multi-monitor`
- **1 ServicePulse instance**: `particular-platform-multi-pulse`

### 5. Access ServicePulse

ServicePulse will be available at: `http://servicepulse-multi.local`

The Error instance automatically aggregates data from all three audit instances through the remote instances feature.

## Sample NServiceBus Endpoints

A complete sample application demonstrating this multi-audit pattern is available in the [`sample-2-endpoints`](sample-2-endpoints/) folder. The sample includes:

### Endpoints
- **Sales Endpoint**: Handles order placement and buyers remorse policies
- **Billing Endpoint**: Processes payments and manages shipping policies  
- **Shipping Endpoint**: Coordinates shipment with external carriers (Maple/Alpine)
- **ClientUI**: Console application for placing orders

### Key Features
- **Platform Connector**: Each endpoint uses domain-specific audit queues
- **Saga Workflows**: Demonstrates business process orchestration
- **Message Flow**: Complete order lifecycle from placement to shipment
- **Error Handling**: Timeout handling and escalation scenarios

### Running the Sample
```shell
# Start infrastructure first
cd ../docker-compose
docker compose -f compose-infrastructure.yml up -d

# Deploy platform
cd ../helm  
helm install particular-platform-multi --create-namespace --namespace particular-platform-multi -f ../helm-tryouts/overrides-sample-2.yaml .

# Run sample endpoints
cd ../helm-tryouts/sample-2-endpoints
dotnet run --project RetailDemo.sln
```

See the [sample endpoints README](sample-2-endpoints/README.md) for detailed instructions.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                                Docker Host                                      │
│                                                                                 │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │   RabbitMQ      │  │ RavenDB Sales   │  │ RavenDB Billing │  │RavenDB Ship.│ │
│  │   :5672         │  │     :8081       │  │     :8082       │  │    :8083    │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  └─────────────┘ │
│                                                                                 │
│                       ┌─────────────────┐                                      │
│                       │ RavenDB Error   │                                      │
│                       │     :8084       │                                      │
│                       └─────────────────┘                                      │
└─────────────────────────────────────────────────────────────────────────────────┘
                                    │
                            host.docker.internal
                                    │
┌─────────────────────────────────────────────────────────────────────────────────┐
│                            Kubernetes Cluster                                  │
│                                                                                 │
│  ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐             │
│  │   Sales Team    │    │ Billing Team    │    │  Shipping Team  │             │
│  │   Endpoints     │    │   Endpoints     │    │   Endpoints     │             │
│  └─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘             │
│            │                      │                      │                     │
│            ▼                      ▼                      ▼                     │
│      sales.audit            billing.audit         shipping.audit              │
│        queue                     queue                  queue                  │
│            │                      │                      │                     │
│            ▼                      ▼                      ▼                     │
│  ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐             │
│  │ Sales Audit     │    │ Billing Audit   │    │ Shipping Audit  │             │
│  │ Instance        │    │ Instance        │    │ Instance        │             │
│  │ Pod :44444      │    │ Pod :44444      │    │ Pod :44444      │             │
│  │ ↓ RavenDB:8081  │    │ ↓ RavenDB:8082  │    │ ↓ RavenDB:8083  │             │
│  └─────────────────┘    └─────────────────┘    └─────────────────┘             │
│            │                      │                      │                     │
│            └──────────┬───────────┴──────────────────────┘                     │
│                       ▼                                                        │
│            ┌─────────────────┐                                                 │
│            │ Error Instance  │ ◄──── Remote Instances API Calls               │
│            │ Pod :33333      │                                                 │
│            │ ↓ RavenDB:8084  │                                                 │
│            └─────────┬───────┘                                                 │
│                      │                                                         │
│                      ▼                                                         │
│            ┌─────────────────┐       ┌─────────────────┐                      │
│            │ ServicePulse    │       │ Monitor Instance│                      │
│            │ Pod :9090       │       │ Pod :33633      │                      │
│            └─────────┬───────┘       └─────────────────┘                      │
│                      │                                                         │
│                      ▼                                                         │
│            ┌─────────────────┐                                                 │
│            │ Nginx Ingress   │                                                 │
│            │servicepulse-multi                                                 │
│            │    .local       │                                                 │
│            └─────────────────┘                                                 │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### Data Flow:
1. **Endpoints** send audit messages to domain-specific queues
2. **Audit Instances** process messages from their dedicated queues
3. **Each Audit Instance** stores data in its own RavenDB database (via host.docker.internal)
4. **Error Instance** aggregates data from all audit instances via Remote Instances API
5. **ServicePulse** connects to Error Instance for unified monitoring view
6. **Users** access ServicePulse through Nginx Ingress

## Configuration Details

### Queue Configuration
Each audit instance processes messages from dedicated queues:
- **Sales team endpoints** → `sales.audit` queue → Sales audit instance
- **Billing team endpoints** → `billing.audit` queue → Billing audit instance  
- **Shipping team endpoints** → `shipping.audit` queue → Shipping audit instance

### Database Isolation
Each instance uses a separate RavenDB database:
- **Sales audit data** → RavenDB on port 8081
- **Billing audit data** → RavenDB on port 8082
- **Shipping audit data** → RavenDB on port 8083
- **Error instance data** → RavenDB on port 8084

### Remote Instances Configuration
The Error instance is automatically configured with remote instances pointing to all audit instances:
```json
[
  { "api_uri": "http://particular-platform-multi-audit-sales:44444/api" },
  { "api_uri": "http://particular-platform-multi-audit-billing:44444/api" },
  { "api_uri": "http://particular-platform-multi-audit-shipping:44444/api" }
]
```

## Production Considerations

1. **Resource Allocation**: Each RavenDB instance should have dedicated resources
2. **Backup Strategy**: Each database needs its own backup schedule
3. **Monitoring**: Monitor each database separately for health and performance
4. **Scaling**: Audit instances can be scaled independently based on message volume
5. **Security**: Consider network policies to isolate database access

## Cleanup

```shell
# Remove Helm deployment
helm uninstall particular-platform-multi -n particular-platform-multi

# Stop infrastructure
cd ../docker-compose
docker compose -f compose-infrastructure.yml down -v

# Remove namespace
kubectl delete namespace particular-platform-multi
```

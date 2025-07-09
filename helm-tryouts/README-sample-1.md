# Single Audit Instance with Shared RavenDB Infrastructure

This example demonstrates how to deploy the Particular Platform with a single audit instance using a shared RavenDB database. This pattern is useful for:

- **Development Environments**: Quick setup for local development and testing
- **Small Production Deployments**: Simple environments with moderate message volume
- **Getting Started**: Learning how the Particular Platform works before scaling to multiple instances
- **Cost Optimization**: Single database reduces infrastructure overhead

## Infrastructure Setup

### 1. Start the Infrastructure Services

First, start the infrastructure services (RabbitMQ + single RavenDB instance):

```shell
cd ../docker-compose
docker compose -f compose-single-audit.yml up -d
```

This will start:
- **RabbitMQ**: `localhost:5672` (Management UI at `localhost:15672`)
- **RavenDB**: `localhost:8080` (shared between error and audit data)

### 2. Verify Infrastructure is Running

```shell
# Check all containers are healthy
docker compose -f compose-single-audit.yml ps

# Test RabbitMQ connectivity
curl -u guest:guest http://localhost:15672/api/overview

# Test RavenDB instance
curl http://localhost:8080/admin/stats
```

## Helm Deployment

### 3. Deploy the Particular Platform

```shell
cd ../helm

# Add servicepulse.local to your hosts file (optional)
echo "127.0.0.1 servicepulse.local" | sudo tee -a /etc/hosts

# Deploy using the single-audit configuration
helm install particular-platform --create-namespace --namespace particular-platform -f overrides-sample-1.yaml .
```

### 4. Verify Deployment

```shell
# Check all pods are running
kubectl get pods -n particular-platform

# Check services
kubectl get services -n particular-platform

# Check ingress
kubectl get ingress -n particular-platform
```

You should see:
- **1 Error instance**: `particular-platform-error`
- **1 Audit instance**: `particular-platform-audit` (using default queue `audit`)
- **1 Monitor instance**: `particular-platform-monitor`
- **1 ServicePulse instance**: `particular-platform-pulse`

### 5. Access ServicePulse

ServicePulse will be available at: `http://servicepulse.local`

## Sample NServiceBus Endpoints

A complete sample application demonstrating this single-audit pattern is available in the [`sample-1-endpoints`](sample-1-endpoints/) folder. The sample includes:

### Endpoints
- **Sales Endpoint**: Handles order placement and buyers remorse policies
- **Billing Endpoint**: Processes payments and manages shipping policies  
- **Shipping Endpoint**: Coordinates shipment with external carriers (Maple/Alpine)
- **ClientUI**: Console application for placing orders

### Key Features
- **Platform Connector**: Each endpoint uses the default audit queue
- **Saga Workflows**: Demonstrates business process orchestration
- **Message Flow**: Complete order lifecycle from placement to shipment
- **Error Handling**: Timeout handling and escalation scenarios

### Running the Sample
```shell
# Start infrastructure first
cd ../docker-compose
docker compose -f compose-single-audit.yml up -d

# Deploy platform
cd ../helm  
helm install particular-platform --create-namespace --namespace particular-platform -f ../helm-tryouts/overrides-sample-1.yaml .

# Run sample endpoints
cd ../helm-tryouts/sample-1-endpoints
dotnet run --project RetailDemo.sln
```

See the [sample endpoints README](sample-1-endpoints/README.md) for detailed instructions.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                                Docker Host                                      │
│                                                                                 │
│  ┌─────────────────┐                    ┌─────────────────┐                    │
│  │   RabbitMQ      │                    │   RavenDB       │                    │
│  │   :5672         │                    │     :8080       │                    │
│  │                 │                    │  (Shared DB)    │                    │
│  └─────────────────┘                    └─────────────────┘                    │
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
│            └──────────┬───────────┴──────────────────────┘                     │
│                       ▼                                                        │
│                 audit queue                                                    │
│                 (default)                                                      │
│                       │                                                        │
│                       ▼                                                        │
│            ┌─────────────────┐                                                 │
│            │ Audit Instance  │                                                 │
│            │ Pod :44444      │                                                 │
│            │ ↓ RavenDB:8080  │                                                 │
│            └─────────┬───────┘                                                 │
│                      │                                                         │
│                      ▼                                                         │
│            ┌─────────────────┐                                                 │
│            │ Error Instance  │                                                 │
│            │ Pod :33333      │                                                 │
│            │ ↓ RavenDB:8080  │ ◄──── Remote Instance API Call                │
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
│            │  servicepulse   │                                                 │
│            │    .local       │                                                 │
│            └─────────────────┘                                                 │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### Data Flow:
1. **All Endpoints** send audit messages to the default `audit` queue
2. **Single Audit Instance** processes messages from the audit queue
3. **Both Audit and Error Instances** store data in the same RavenDB database (via host.docker.internal)
4. **Error Instance** aggregates data from the audit instance via Remote Instance API
5. **ServicePulse** connects to Error Instance for unified monitoring view
6. **Users** access ServicePulse through Nginx Ingress

## Configuration Details

### Queue Configuration
All endpoints use the default audit queue configuration:
- **All team endpoints** → `audit` queue → Single audit instance

### Database Sharing
Both instances use the same RavenDB database:
- **Audit instance data** → RavenDB on port 8080
- **Error instance data** → RavenDB on port 8080 (same database, different collections)

### Remote Instance Configuration
The Error instance is automatically configured with the audit instance:
```json
[
  { "api_uri": "http://particular-platform-audit:44444/api" }
]
```

## Configuration Comparison

| Aspect | Single Audit | Multiple Audit |
|--------|--------------|----------------|
| **Complexity** | Simple | Complex |
| **Database Count** | 1 RavenDB | 4+ RavenDBs |
| **Queue Strategy** | Shared `audit` queue | Domain-specific queues |
| **Resource Usage** | Lower | Higher |
| **Isolation** | None | Full domain separation |
| **Scalability** | Limited | High |
| **Best For** | Dev/Test, Small Production | Large Production, Enterprise |

## When to Use Single Audit

### ✅ Good For:
- **Development environments** where simplicity is key
- **Small production systems** with moderate message volume
- **Learning and experimentation** with the Particular Platform
- **Cost-sensitive deployments** where infrastructure overhead matters
- **Teams that don't need data isolation** between business domains

### ❌ Consider Multiple Audit When:
- **High message volume** that could overwhelm a single instance
- **Business domain separation** is required for compliance or organizational reasons
- **Different retention policies** are needed for different types of data
- **Independent scaling** of audit processing is needed
- **Performance isolation** between different business areas is important

## Production Considerations

1. **Database Sizing**: Ensure RavenDB has sufficient storage for combined audit and error data
2. **Backup Strategy**: Single database means single backup/restore process
3. **Monitoring**: Monitor the shared database for performance and capacity
4. **Scaling**: Consider migrating to multiple audit instances as volume grows
5. **Security**: All audit data is in one database - ensure appropriate access controls

## Migration Path

This single audit setup provides an easy migration path to multiple audit instances:

1. **Start Simple**: Begin with single audit for development and initial production
2. **Monitor Growth**: Track message volume and database size
3. **Plan Separation**: Identify business domains that would benefit from separation
4. **Gradual Migration**: Move to multiple audit instances when needed

## Cleanup

```shell
# Remove Helm deployment
helm uninstall particular-platform -n particular-platform

# Stop infrastructure
cd ../docker-compose
docker compose -f compose-single-audit.yml down -v

# Remove namespace
kubectl delete namespace particular-platform
```

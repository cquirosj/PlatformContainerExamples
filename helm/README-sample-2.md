# Multiple Audit Instances with Separate RavenDB Infrastructure

This example demonstrates how to deploy the Particular Platform with multiple audit instances, each using a dedicated RavenDB database. This pattern is useful for:

- **Business Domain Separation**: Different teams (Sales, Marketing, Support) with isolated audit data
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
- **RavenDB Marketing**: `localhost:8082`
- **RavenDB Support**: `localhost:8083`
- **RavenDB Error**: `localhost:8084`

### 2. Verify Infrastructure is Running

```shell
# Check all containers are healthy
docker compose -f compose-infrastructure.yml ps

# Test RabbitMQ connectivity
curl -u guest:guest http://localhost:15672/api/overview

# Test RavenDB instances
curl http://localhost:8081/admin/stats  # Sales
curl http://localhost:8082/admin/stats  # Marketing
curl http://localhost:8083/admin/stats  # Support
curl http://localhost:8084/admin/stats  # Error
```

## Helm Deployment

### 3. Deploy the Particular Platform

```shell
cd ../helm

# Add servicepulse-multi.local to your hosts file (optional)
echo "127.0.0.1 servicepulse-multi.local" | sudo tee -a /etc/hosts

# Deploy using the multi-audit configuration
helm install particular-platform-multi --create-namespace --namespace particular-platform-multi -f overrides-sample-2.yaml .
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
  - `particular-platform-multi-audit-marketing`
  - `particular-platform-multi-audit-support`
- **1 Monitor instance**: `particular-platform-multi-monitor`
- **1 ServicePulse instance**: `particular-platform-multi-pulse`

### 5. Access ServicePulse

ServicePulse will be available at: `http://servicepulse-multi.local`

The Error instance automatically aggregates data from all three audit instances through the remote instances feature.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                                Docker Host                                      │
│                                                                                 │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │   RabbitMQ      │  │ RavenDB Sales   │  │RavenDB Marketing│  │RavenDB Supp.│ │
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
│  │   Sales Team    │    │ Marketing Team  │    │  Support Team   │             │
│  │   Endpoints     │    │   Endpoints     │    │   Endpoints     │             │
│  └─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘             │
│            │                      │                      │                     │
│            ▼                      ▼                      ▼                     │
│      sales.audit            marketing.audit        support.audit               │
│        queue                     queue                 queue                   │
│            │                      │                      │                     │
│            ▼                      ▼                      ▼                     │
│  ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐             │
│  │ Sales Audit     │    │Marketing Audit  │    │Support Audit    │             │
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
- **Marketing team endpoints** → `marketing.audit` queue → Marketing audit instance  
- **Support team endpoints** → `support.audit` queue → Support audit instance

### Database Isolation
Each instance uses a separate RavenDB database:
- **Sales audit data** → RavenDB on port 8081
- **Marketing audit data** → RavenDB on port 8082
- **Support audit data** → RavenDB on port 8083
- **Error instance data** → RavenDB on port 8084

### Remote Instances Configuration
The Error instance is automatically configured with remote instances pointing to all audit instances:
```json
[
  { "api_uri": "http://particular-platform-multi-audit-sales:44444/api" },
  { "api_uri": "http://particular-platform-multi-audit-marketing:44444/api" },
  { "api_uri": "http://particular-platform-multi-audit-support:44444/api" }
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

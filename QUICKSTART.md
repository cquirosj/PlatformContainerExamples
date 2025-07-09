# Quick Start Guide

This guide provides step-by-step instructions for running the Particular Service Platform samples.

## Prerequisites

- [Docker](https://docs.docker.com/get-docker/) and [Docker Compose](https://docs.docker.com/compose/install/)
- [Kubernetes](https://kubernetes.io/docs/tasks/tools/) (Docker Desktop with Kubernetes enabled)
- [Helm](https://helm.sh/docs/intro/install/)
- [kubectl](https://kubernetes.io/docs/tasks/tools/install-kubectl/)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for running sample endpoints)

## Sample 1: Single Audit Instance

This sample demonstrates a simple setup with one audit instance using a single RavenDB for both error and audit data.

### 1. Start Infrastructure

```bash
# Navigate to the docker-compose directory
cd docker-compose

# Start RabbitMQ and RavenDB
docker compose -f compose-single-audit.yml up -d

# Verify services are running
docker compose -f compose-single-audit.yml ps
```

### 2. Install Particular Platform in Kubernetes

```bash
# Navigate to the helm directory
cd ../helm

# Install the Helm chart with sample 1 configuration
helm install particular-platform --create-namespace --namespace particular-platform -f overrides-sample-1.yaml .

# Wait for pods to be ready
kubectl wait --for=condition=ready pod --all -n particular-platform --timeout=300s
```

### 3. Configure Access

Add the following line to your `/etc/hosts` file:
```
127.0.0.1 servicepulse.local
```

### 4. Validate Setup

```bash
# Navigate back to project root
cd ..

# Run validation script
./validate-samples.sh sample1
```

### 5. Access Services

- **ServicePulse**: http://servicepulse.local
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **RavenDB Studio**: http://localhost:8080

## Sample 2: Multiple Audit Instances

This sample demonstrates an enterprise setup with separate audit instances for different business domains (Sales, Billing, Shipping), each with its own RavenDB.

### 1. Start Infrastructure

```bash
# Navigate to the docker-compose directory
cd docker-compose

# Start RabbitMQ and multiple RavenDB instances
docker compose -f compose-infrastructure.yml up -d

# Verify services are running
docker compose -f compose-infrastructure.yml ps
```

### 2. Install Particular Platform in Kubernetes

```bash
# Navigate to the helm directory
cd ../helm

# Install the Helm chart with sample 2 configuration
helm install particular-platform --create-namespace --namespace particular-platform -f overrides-sample-2.yaml .

# Wait for pods to be ready
kubectl wait --for=condition=ready pod --all -n particular-platform --timeout=300s
```

### 3. Configure Access

Add the following line to your `/etc/hosts` file:
```
127.0.0.1 servicepulse.local
```

### 4. Validate Setup

```bash
# Navigate back to project root
cd ..

# Run validation script
./validate-samples.sh sample2
```

### 5. Access Services

- **ServicePulse**: http://servicepulse.local
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **RavenDB Studios**: 
  - Error: http://localhost:8080
  - Sales: http://localhost:8081
  - Billing: http://localhost:8082
  - Shipping: http://localhost:8083

## Running Sample NServiceBus Endpoints

To see the platform in action, run the sample NServiceBus endpoints:

```bash
# Navigate to sample endpoints
cd sample-endpoints

# Build the solution
dotnet build

# Run endpoints in separate terminals
dotnet run --project Sales/Sales.csproj
dotnet run --project Billing/Billing.csproj  
dotnet run --project Shipping/Shipping.csproj
dotnet run --project ClientUI/ClientUI.csproj
```

The endpoints will automatically:
- Connect to RabbitMQ running in Docker
- Send audit messages to their respective audit queues (sample 2) or default queue (sample 1)
- Enable monitoring through the Platform Connector

## Generating Test Data

With all endpoints running, use the ClientUI to generate messages:

1. Access the ClientUI console
2. Press 'P' to place orders
3. Press 'S' to ship orders
4. Check ServicePulse to see throughput and audit data

## Troubleshooting

### Common Issues

1. **Services not starting**: Check Docker resources and ensure ports are not in use
2. **Kubernetes pods failing**: Check logs with `kubectl logs -n particular-platform <pod-name>`
3. **ServicePulse not accessible**: Verify ingress configuration and /etc/hosts entry
4. **Sample endpoints can't connect**: Ensure infrastructure is running and accessible

### Useful Commands

```bash
# Check infrastructure status
docker compose -f docker-compose/compose-single-audit.yml ps
docker compose -f docker-compose/compose-infrastructure.yml ps

# Check Kubernetes status
kubectl get pods -n particular-platform
kubectl get services -n particular-platform
kubectl get ingress -n particular-platform

# View logs
kubectl logs -n particular-platform deployment/servicecontrol-error
kubectl logs -n particular-platform deployment/servicecontrol-audit
kubectl logs -n particular-platform deployment/servicepulse

# Clean up
helm uninstall particular-platform -n particular-platform
kubectl delete namespace particular-platform
docker compose -f docker-compose/compose-single-audit.yml down
docker compose -f docker-compose/compose-infrastructure.yml down
```

## Next Steps

- Read the detailed documentation:
  - [Single Audit Instance (Sample 1)](helm/README-sample-1.md)
  - [Multiple Audit Instances (Sample 2)](helm/README-sample-2.md)
- Explore the NServiceBus Platform Connector configuration in the sample endpoints
- Adapt the configurations for your specific environment and requirements
- Review the [Particular Platform documentation](https://docs.particular.net/platform/) for production deployment considerations

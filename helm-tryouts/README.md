# Helm Tryouts - Sample Configurations

This folder contains sample configurations and examples showing how to use the Particular Platform Helm chart located in the `../helm/` directory.

## Contents

### Sample Configurations
- **`overrides-sample-1.yaml`**: Single audit instance configuration
- **`overrides-sample-2.yaml`**: Multiple audit instances configuration (Sales, Billing, Shipping)

### Documentation
- **`README-sample-1.md`**: Detailed guide for single audit instance setup
- **`README-sample-2.md`**: Detailed guide for multiple audit instances setup

### Sample Applications
- **`sample-1-endpoints/`**: Simple NServiceBus solution for single audit scenario
  - All endpoints use default audit queue
  - Simplified Platform Connector configuration
  - Shared audit data in single RavenDB
- **`sample-2-endpoints/`**: Complex NServiceBus solution for multi-audit scenario  
  - Domain-specific audit queues (sales.audit, billing.audit, shipping.audit)
  - Advanced Platform Connector configuration
  - Separate audit data storage per domain

## Quick Start

### Sample 1: Single Audit Instance
```bash
# Start infrastructure
cd ../docker-compose
docker compose -f compose-single-audit.yml up -d

# Deploy platform
cd ../helm
helm install particular-platform --create-namespace --namespace particular-platform -f ../helm-tryouts/overrides-sample-1.yaml .
```

### Sample 2: Multiple Audit Instances
```bash
# Start infrastructure
cd ../docker-compose
docker compose -f compose-infrastructure.yml up -d

# Deploy platform
cd ../helm
helm install particular-platform --create-namespace --namespace particular-platform -f ../helm-tryouts/overrides-sample-2.yaml .
```

## Documentation

For detailed setup instructions and architecture explanations:
- **Single Audit**: [README-sample-1.md](README-sample-1.md)
- **Multiple Audit**: [README-sample-2.md](README-sample-2.md)

## Dependencies

These samples require:
- Infrastructure services from `../docker-compose/`
- The Helm chart from `../helm/`
- Kubernetes cluster (Docker Desktop with Kubernetes enabled)
- nginx ingress controller

## Sample Endpoints

### Sample 1 Endpoints (`sample-1-endpoints/`)
Simple configuration for single audit scenario:
- All endpoints use default audit queue
- Simplified Platform Connector setup
- Shared audit data storage

See [sample-1-endpoints/README.md](sample-1-endpoints/README.md) for details.

### Sample 2 Endpoints (`sample-2-endpoints/`)
Advanced configuration for multi-audit scenario:
- Domain-specific audit queues
- Complex Platform Connector configuration  
- Separated audit data per business domain

See [sample-2-endpoints/README.md](sample-2-endpoints/README.md) for details.

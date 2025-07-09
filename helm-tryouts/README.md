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
- **`sample-endpoints/`**: Complete NServiceBus solution demonstrating both scenarios
  - Sales, Billing, Shipping, and ClientUI endpoints
  - Platform Connector integration
  - Domain-specific audit queues

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

The `sample-endpoints/` folder contains a complete working example:
- NServiceBus endpoints using Platform Connector
- Business logic for order processing workflow
- Error handling and saga patterns
- Configuration for both single and multi-audit scenarios

See [sample-endpoints/README.md](sample-endpoints/README.md) for details.

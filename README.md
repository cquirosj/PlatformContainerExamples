# Platform Container Examples

This repository contains examples of deploying the [Particular Service Platform](https://docs.particular.net/platform/) tools (notably [ServiceControl](https://docs.particular.net/servicecontrol/) and [ServicePulse](https://docs.particular.net/servicepulse/)) using containers. These examples can be used as starting points for deployment scripts, or as a tool to learn how the different pieces work together, but should not be used as-is in production environments.

## 🚀 Quick Start

For step-by-step instructions to get started quickly, see [QUICKSTART.md](QUICKSTART.md).

## Examples

- [Deploying to Azure Container Apps using Bicep](/azure-container-apps/)
- [Running locally with Docker Compose](/docker-compose/)
- [Deploying to a Kubernetes cluster using helm](/helm/)

## Sample Configurations

This repository includes two complete sample configurations demonstrating different deployment scenarios:

### Sample 1: Single Audit Instance
Simple setup with one audit instance using a single RavenDB for both error and audit data. Ideal for development, testing, or small production environments.
- **Documentation**: [helm/README-sample-1.md](helm/README-sample-1.md)

### Sample 2: Multiple Audit Instances  
Enterprise setup with separate audit instances for different business domains (Sales, Billing, Shipping), each with its own RavenDB. Ideal for large production environments with domain separation requirements.
- **Documentation**: [helm/README-sample-2.md](helm/README-sample-2.md)

Each sample includes:
- Dedicated Docker Compose infrastructure files
- Helm override configurations
- Sample NServiceBus endpoints with Platform Connector integration
- Complete documentation and validation scripts

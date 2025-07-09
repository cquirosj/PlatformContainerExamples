# Platform Container Examples

## Prerequisites

- [Docker Desktop](https://docs.docker.com/get-docker/) with Kubernetes enabled
- [Helm](https://helm.sh/docs/intro/install/)
- [kubectl](https://kubernetes.io/docs/tasks/tools/install-kubectl/)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for running sample endpoints)
- **nginx Ingress Controller** for Kubernetes (required for accessing ServicePulse)

### Installing nginx Ingress Controller

The samples use nginx ingress to expose ServicePulse externally. Install it in your Docker Desktop Kubernetes cluster:

```bash
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.11.0/deploy/static/provider/cloud/deploy.yaml
```

Verify the installation:
```bash
kubectl get pods -n ingress-nginx
kubectl get services -n ingress-nginx
```

## 🚀 Quick Start

For step-by-step instructions to get started quickly, see [QUICKSTART.md](QUICKSTART.md).

## Examples

- [Deploying to Azure Container Apps using Bicep](/azure-container-apps/)
- [Running locally with Docker Compose](/docker-compose/)
- [Deploying to a Kubernetes cluster using helm](/helm/)
- [Sample configurations and tryouts](/helm-tryouts/)

## Sample Configurations

This repository includes two complete sample configurations demonstrating different deployment scenarios:

### Sample 1: Single Audit Instance
Simple setup with one audit instance using a single RavenDB for both error and audit data. Ideal for development, testing, or small production environments.
- **Documentation**: [helm-tryouts/README-sample-1.md](helm-tryouts/README-sample-1.md)

### Sample 2: Multiple Audit Instances  
Enterprise setup with separate audit instances for different business domains (Sales, Billing, Shipping), each with its own RavenDB. Ideal for large production environments with domain separation requirements.
- **Documentation**: [helm-tryouts/README-sample-2.md](helm-tryouts/README-sample-2.md)

Each sample includes:
- Dedicated Docker Compose infrastructure files (in `/docker-compose/`)
- Helm override configurations (in `/helm-tryouts/`)
- Sample NServiceBus endpoints with Platform Connector integration (in `/helm-tryouts/sample-endpoints/`)
- Complete documentation and validation scripts

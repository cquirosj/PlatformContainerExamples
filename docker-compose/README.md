# Running with Docker Compose

This directory contains Docker Compose files for different deployment scenarios of the [Particular Service Platform](https://docs.particular.net/platform/) tools.

## Available Configurations

### 1. Standard Demo (`compose.yml`)
Complete demonstration setup including ServiceControl, ServicePulse, and supporting infrastructure.

**Services included:**
- ServiceControl (Error, Audit, Monitoring instances)
- ServicePulse UI
- RavenDB database
- RabbitMQ message broker

### 2. Single Audit Infrastructure (`compose-single-audit.yml`)
Infrastructure-only setup for the single audit instance scenario (used with Helm sample 1).

**Services included:**
- RabbitMQ message broker
- Single RavenDB instance (shared between error and audit)

### 3. Multi-Audit Infrastructure (`compose-infrastructure.yml`)
Infrastructure-only setup for the multiple audit instances scenario (used with Helm sample 2).

**Services included:**
- RabbitMQ message broker
- Four RavenDB instances (one for error, three for audit instances: Sales, Billing, Shipping)

## Usage

### Standard Demo
```shell
docker compose pull
docker compose up -d
```

### Infrastructure for Helm Samples
```shell
# For single audit scenario (Helm sample 1)
docker compose -f compose-single-audit.yml up -d

# For multi-audit scenario (Helm sample 2)  
docker compose -f compose-infrastructure.yml up -d
```

Running ServiceControl and ServicePulse locally in containers provides a way to use and test Service Platform features during local development on any platform, without needing to install Windows services.

## Access Points

Once the standard demo is running:

* [ServicePulse](https://docs.particular.net/servicepulse/) can be accessed at http://localhost:9090
* [ServiceInsight](https://docs.particular.net/serviceinsight/) can be used with a connection URL of http://localhost:33333/api

For infrastructure-only setups:
* RabbitMQ Management UI: http://localhost:15672 (guest/guest)
* RavenDB Studio: http://localhost:8080 (single-audit) or http://localhost:8080-8083 (multi-audit)

## Implementation details

### Standard Demo (compose.yml)
* The ports for all services are exposed to localhost:
  * `33333`: ServiceControl API
  * `44444`: Audit API
  * `33633`: Monitoring API
  * `8080`: Database backend
  * `9090` ServicePulse UI
* One instance of the [`servicecontrol-ravendb` container](https://docs.particular.net/servicecontrol/ravendb/containers) is used for both the [`servicecontrol`](https://docs.particular.net/servicecontrol/servicecontrol-instances/deployment/containers) and [`servicecontrol-audit`](https://docs.particular.net/servicecontrol/audit-instances/deployment/containers) containers.
  * _A single database container should not be shared between multiple ServiceControl instances in production scenarios._

### Infrastructure-only Setups
* **Single Audit** (`compose-single-audit.yml`):
  * RabbitMQ on port 5672 (AMQP) and 15672 (Management UI)
  * Single RavenDB on port 8080
  
* **Multi-Audit** (`compose-infrastructure.yml`):
  * RabbitMQ on port 5672 (AMQP) and 15672 (Management UI)
  * Four RavenDB instances on ports 8080-8083:
    * 8080: Error instance database
    * 8081: Sales audit instance database
    * 8082: Billing audit instance database
    * 8083: Shipping audit instance database

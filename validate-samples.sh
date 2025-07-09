#!/bin/bash
set -e

echo "=== Particular Service Platform - Sample Validation ==="
echo ""

# Function to check if a service is responding
check_service() {
    local url=$1
    local service_name=$2
    local max_attempts=30
    local attempt=1
    
    echo "Checking $service_name at $url..."
    
    while [ $attempt -le $max_attempts ]; do
        if curl -s -f "$url" > /dev/null 2>&1; then
            echo "✅ $service_name is responding"
            return 0
        fi
        
        echo "⏳ Attempt $attempt/$max_attempts - $service_name not ready yet, waiting..."
        sleep 5
        attempt=$((attempt + 1))
    done
    
    echo "❌ $service_name failed to start within expected time"
    return 1
}

# Function to validate infrastructure
validate_infrastructure() {
    local scenario=$1
    echo ""
    echo "=== Validating Infrastructure for $scenario ==="
    
    # Check RabbitMQ
    check_service "http://localhost:15672" "RabbitMQ Management"
    
    # Check RavenDB instances based on scenario
    if [ "$scenario" = "Single Audit" ]; then
        check_service "http://localhost:8080/admin/stats" "RavenDB (Single)"
    else
        check_service "http://localhost:8080/admin/stats" "RavenDB Error"
        check_service "http://localhost:8081/admin/stats" "RavenDB Sales"
        check_service "http://localhost:8082/admin/stats" "RavenDB Billing"
        check_service "http://localhost:8083/admin/stats" "RavenDB Shipping"
    fi
}

# Function to validate Kubernetes services
validate_kubernetes() {
    echo ""
    echo "=== Validating Kubernetes Services ==="
    
    # Check if kubectl is available
    if ! command -v kubectl &> /dev/null; then
        echo "❌ kubectl not found. Please install kubectl to validate Kubernetes services."
        return 1
    fi
    
    # Check if namespace exists
    if ! kubectl get namespace particular-platform &> /dev/null; then
        echo "❌ particular-platform namespace not found. Please install the Helm chart first."
        return 1
    fi
    
    # Check pods status
    echo "Checking pod status..."
    kubectl get pods -n particular-platform
    
    # Check if all pods are running
    local not_running=$(kubectl get pods -n particular-platform --no-headers | grep -v Running | wc -l)
    if [ "$not_running" -gt 0 ]; then
        echo "⚠️  Some pods are not in Running state. Check the output above."
    else
        echo "✅ All pods are running"
    fi
    
    # Check services
    echo ""
    echo "Service endpoints:"
    kubectl get services -n particular-platform
}

# Main validation logic
if [ $# -eq 0 ]; then
    echo "Usage: $0 [sample1|sample2]"
    echo ""
    echo "sample1: Validate single audit instance scenario"
    echo "sample2: Validate multi-audit instances scenario"
    echo ""
    echo "Examples:"
    echo "  $0 sample1    # Validate sample 1 (single audit)"
    echo "  $0 sample2    # Validate sample 2 (multi-audit)"
    exit 1
fi

case $1 in
    sample1)
        echo "Validating Sample 1: Single Audit Instance"
        echo "Infrastructure file: docker-compose/compose-single-audit.yml"
        echo "Helm overrides: helm-tryouts/overrides-sample-1.yaml"
        validate_infrastructure "Single Audit"
        validate_kubernetes
        ;;
    sample2)
        echo "Validating Sample 2: Multiple Audit Instances"
        echo "Infrastructure file: docker-compose/compose-infrastructure.yml"
        echo "Helm overrides: helm-tryouts/overrides-sample-2.yaml"
        validate_infrastructure "Multi-Audit"
        validate_kubernetes
        ;;
    *)
        echo "❌ Invalid sample: $1"
        echo "Use 'sample1' or 'sample2'"
        exit 1
        ;;
esac

echo ""
echo "=== Validation Complete ==="
echo ""
echo "Next steps:"
echo "1. Access ServicePulse at http://servicepulse.local (add to /etc/hosts: 127.0.0.1 servicepulse.local)"
echo "2. Run the sample NServiceBus endpoints in helm-tryouts/sample-endpoints/ directory"
echo "3. Generate some traffic to see monitoring data"

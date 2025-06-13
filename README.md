# Projeto-AS-2 - Editorial Workflow System for Piranha CMS

## Course Information
**Course:** Arquiteturas de Software (Software Architectures)  
**Project:** Second Project - Editorial Workflow Implementation  
**Team Members:** André Oliveira, Alexandre Cotorobai, Hugo Correia, Joaquim Rosa

## Project Description

This project extends Piranha CMS with a comprehensive editorial workflow system that provides content governance, audit trails, and real-time notifications. The implementation follows enterprise software patterns and event-driven architecture to deliver a scalable solution for organizations requiring structured content approval processes.

### Key Features Implemented

#### 1. **Editorial Workflow Module**
- **Configurable Workflows**: Define custom approval chains with multiple states (draft, review, approved, published)
- **Role-Based Permissions**: Control state transitions based on user roles
- **State Machine Pattern**: Ensures content follows predefined approval paths
- **Workflow Templates**: Reusable workflow definitions for different content types

#### 2. **Audit Trail System**
- **Complete State History**: Tracks all workflow transitions with timestamps
- **Reviewer Information**: Records who approved/rejected content and why
- **Event-Driven Architecture**: Uses RabbitMQ for reliable event processing
- **Compliance Ready**: Provides full audit trail for regulatory requirements

#### 3. **Real-Time Notifications**
- **State Change Alerts**: Notifies relevant users when content moves between states
- **RabbitMQ Integration**: Asynchronous message processing for scalability
- **Configurable Recipients**: Route notifications based on roles and workflow states

#### 4. **Observability & Monitoring**
- **OpenTelemetry Integration**: Full distributed tracing support
- **Prometheus Metrics**: Monitor workflow performance and bottlenecks
- **Jaeger Tracing**: Visualize request flows through the system
- **Grafana Dashboards**: Real-time monitoring of system health

## How to Run with Docker Compose

### Prerequisites
- Docker and Docker Compose installed
- At least 4GB of available RAM
- Ports 3000, 5672, 8080, 9090, 15672, and 16686 available

### Quick Start

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Projeto-AS-2/piranha.core/examples/MvcWeb
   ```

2. **Start all services besides mvcweb-app**
   ```bash
   docker-compose up --build
   ```
3. **Start the main application**
   ```bash
   docker-compose up --build mvcweb-app
   ```

4. **Access the applications**
   - **Piranha CMS**: http://localhost:8080
     - Default admin credentials: admin / password
   - **RabbitMQ Management**: http://localhost:15672
     - Credentials: admin / admin
   - **Jaeger UI** (Tracing): http://localhost:16686
   - **Prometheus** (Metrics): http://localhost:9090
   - **Grafana** (Dashboards): http://localhost:3000
     - Credentials: admin / admin

5. **Run load tests** (optional)
   ```bash
   docker-compose --profile loadtest run k6
   ```

### Docker Services Overview

The docker-compose setup includes:
- **mvcweb-app**: The main Piranha CMS application with workflow extensions
- **rabbitmq**: Message broker for event-driven communication
- **otel-collector**: Collects and routes telemetry data
- **jaeger**: Distributed tracing visualization
- **prometheus**: Time-series metrics database
- **grafana**: Metrics visualization and dashboards
- **k6**: Load testing tool (optional, with loadtest profile)

### Stopping the Application

```bash
docker-compose down
```

To also remove volumes (database and uploads):
```bash
docker-compose down -v
```

## Architecture Highlights

- **Event-Driven Design**: Loose coupling between workflow, audit, and notification modules
- **Microservices Ready**: Each module can be deployed independently
- **Observable by Default**: Full telemetry instrumentation for production readiness
- **Scalable**: Horizontal scaling supported through RabbitMQ and stateless services
- **Extensible**: Clean interfaces allow custom workflow implementations

## Development Notes

The project demonstrates advanced software architecture concepts including:
- Domain-Driven Design (DDD) principles
- Event Sourcing patterns for audit trails
- CQRS implementation for read/write separation
- Distributed systems best practices
- Container orchestration with Docker Compose

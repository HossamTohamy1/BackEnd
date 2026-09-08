# Infrastructure Layer

This layer handles all communication with external systems, databases, and real-time hubs.

## Rules
- **Dependencies**: Depends ONLY on `Application` and `Shared`.
- **Data Access**: Implements `ApplicationDbContext` which acts as the `IApplicationDbContext`. 
- **NO HTTP Handling**: This layer must not contain Controllers, Minimal APIs, or any logic that directly handles HTTP requests.
- **Implementations only**: Services here are implementations of interfaces defined in the `Application` layer.

## Usage
- Entity Framework Core Configurations belong in `Persistence/Configurations/`.
- External service integrations (e.g., Email Service, Payment Gateway) belong here.
- Authentication configurations (Smart Scheme) are initialized here.

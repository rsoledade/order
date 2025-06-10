# Order Microservice

This is a microservice for order processing using .NET Core, PostgreSQL, and following the DDD with Clean Architecture approach.

## Architecture

The solution follows Domain-Driven Design (DDD) principles and Clean Architecture with the following layers:

- **Domain Layer**: Contains entities, value objects, domain services, and interfaces.
- **Application Layer**: Contains use cases implemented via CQRS with MediatR.
- **Infrastructure Layer**: Contains implementations of repositories, database context, and messaging.
- **API Layer**: Contains controllers and configuration for the REST API.

## Technologies

- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core with PostgreSQL
- MediatR for CQRS
- FluentValidation for validation
- AutoMapper for object mapping
- Swagger for API documentation
- Docker for containerization

## How to Run

### Prerequisites

- .NET 8.0 SDK or later
- Docker and Docker Compose

### Running with Docker

1. Start the PostgreSQL database:
   ```
   docker compose -f docker-compose-db.yml up -d
   ```

2. Build and run the application:
   ```
   docker compose up -d
   ```

### Running Locally

1. Start PostgreSQL using Docker:
   ```
   docker compose -f docker-compose-db.yml up -d
   ```

2. Apply database migrations:
   ```
   cd src/Order.Infrastructure
   dotnet ef database update --startup-project ../Order.API
   ```

3. Run the API:
   ```
   cd ../Order.API
   dotnet run
   ```

4. Access the Swagger UI:
   ```
   https://localhost:5001/
   ```

## API Endpoints

### Order Controller

- `GET /api/orders`: Get all orders or filter by OrderId or ExternalId
- `POST /api/orders`: Register a new order

### Health Controller

- `GET /api/health`: Check the health status of the service

## Domain Model

### Key Entities

- **Order**: The main aggregate root that represents a customer order
- **Product**: Represents items in an order

### Value Objects

- **OrderId**: Unique identifier for orders
- **Money**: Represents monetary values with proper encapsulation

### Enums

- **OrderStatus**: Represents the different states of an order (Pending, Processing, Completed, Error, Duplicate)

## Implementation Details

### CQRS Pattern

The application uses the CQRS pattern for separation of read and write operations:

- **Commands**: RegisterOrder
- **Queries**: GetOrders

### Event Publishing

When an order is successfully processed, it publishes an OrderProcessedEvent that downstream systems can consume.

### Error Handling

The application includes a global exception handling middleware that provides consistent error responses.

## Project Structure

```
Order/
│
├── src/
│   ├── Order.API/                      # Presentation layer
│   │   ├── Controllers/
│   │   ├── Middlewares/
│   │   ├── Extensions/
│   │   ├── Filters/
│   │   └── Program.cs
│
│   ├── Order.Application/              # Application layer
│   │   ├── Commands/
│   │   │   └── RegisterOrder/
│   │   ├── Queries/
│   │   │   └── GetOrders/
│   │   ├── DTOs/
│   │   ├── Interfaces/
│   │   └── Events/
│
│   ├── Order.Domain/                   # Domain layer
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Enums/
│   │   ├── Services/
│   │   └── Interfaces/
│
│   ├── Order.Infrastructure/           # Infrastructure layer
│   │   ├── Data/
│   │   │   ├── Context/
│   │   │   └── Repositories/
│   │   ├── Messaging/
│   │   ├── Mappings/
│   │   └── IoC/
│
│   └── Order.Tests/                    # Tests
│       ├── Domain/
│       ├── Application/
│       └── Infrastructure/
│
├── docker-compose.yml
└── Order.sln
```
# Order Microservice

# Important for Migrations

To run Entity Framework Core commands (such as `dotnet ef migrations add` or `dotnet ef database update`), execute them from the root of the project (where the `src` folder is) or from the `src/Order.API` directory.

The `appsettings.json` file with the connection string must be in `src/Order.API`.

Example command from the project root:

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
      docker compose -f docker-compose-db.yml up -d
   
2. Build and run the application:
      docker compose up -d
   
### Running Locally

1. Start PostgreSQL using Docker:
      docker compose -f docker-compose-db.yml up -d
   
2. Apply database migrations:
      cd src/Order.Infrastructure
      dotnet ef database update --startup-project ../Order.API
   
3. Run the API:
      cd ../Order.API
      dotnet run
   
4. Access the Swagger UI:
      https://localhost:5001/
   
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

# Load Testing - Order API

## How to run the load test with k6

1. Install [k6](https://k6.io/docs/getting-started/installation/).
2. Make sure the API is running and accessible (adjust the BASE_URL variable if necessary).
3. Run the load test:

- The script simulates multiple orders per second, varying the data to avoid duplicates.
- Adjust the `vus` and `duration` parameters in the script to simulate the desired volume (e.g., 200k orders/day ≈ 2.3 req/s).
- Monitor throughput, latency, and error rate at the end of the test.

## Recommended Parameters

- To simulate 200,000 orders in 24h: about 2.3 requests/second.
- Example configuration:
  - `vus: 10`
  - `duration: "6h"`
  - Or adjust the `sleep()` in the script to control the rate.

---

## Automated Load Test with NBomber

1. Make sure the API is running locally (`http://localhost:5000`).
2. Remove or comment the `[Fact(Skip = ...)]` attribute from the `RegisterOrder_LoadTest` method in `src/Order.LoadTests/OrderLoadTests.cs` so the test will run.
3. In the terminal, navigate to the project root and run:

   dotnet test src/Order.LoadTests/Order.LoadTests.csproj --filter FullyQualifiedName~RegisterOrder_LoadTest

4. The test will simulate multiple POST requests to `/api/orders/register-order` and report throughput, latency, and error rate.

5. Adjust the `InjectPerSec(rate: 2, ...)` parameter in the test to simulate the desired volume (e.g., 2-3 req/s for 150-200k orders/day).

**Prerequisites:**
- .NET 8 SDK installed
- API and database running locally
- NBomber packages installed (already referenced in the Order.LoadTests project)

---
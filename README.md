# Distributed Data Access Lab
## Containerized E-Commerce Microservices Backend
**Course:** CSCI 6844 - Programming for the Internet   
**Last Updated:** April 2026

---

## Overview

A containerized distributed backend for a simplified e-commerce platform built with ASP.NET Core 10, Entity Framework Core, Docker, Docker Compose, and RabbitMQ. The system follows the **Database-per-Service** pattern where each microservice owns its SQLite database. Communication between services happens through REST APIs (synchronous) and RabbitMQ (asynchronous event publishing).

**Key Features:**
- 4 independent microservices with isolated data stores
- API Gateway with Ocelot for unified routing and Swagger aggregation
- Synchronous validation via HttpClient between services
- Asynchronous event processing via RabbitMQ
- Complete Swagger/OpenAPI documentation per service
- Fully containerized with Docker and Docker Compose
- Async/await throughout for non-blocking I/O
- Entity Framework Core with SQLite per service

---

## Architecture

```
                         Client (Browser / Swagger)
                                   |
                                   | HTTP
                                   v
                         +-----------------+
                         | API Gateway     |
                         | Port: 5000      |
                         | Ocelot Routing  |
                         +-----------------+
                      |       |       |       |
              +-------+       |       |       +-------+
              |               |       |               |
              v               v       v               v
    +--------------+  +-----------+  +-----------+  +--------------------+
    | Order        |  | Customer  |  | Product   |  | Notification       |
    | Service      |  | Service   |  | Service   |  | Service            |
    | Port: 5002   |  | Port: 5001|  | Port: 5003|  | Port: 5004         |
    | orders.db    |  | customers | | products  |  | notifications.db   |
    +--------------+  | .db       |  | .db       |  +--------------------+
          |           +-----------+  +-----------+           ^
          |                 ^              ^                 |
          |                 |              |                 |
          +-----REST API Calls (sync)------+                 |
          |                                                  |
          |          RabbitMQ                                |
          | (OrderCreated event)                             |
          +--------------------------------------------------+
              
Data Communication:
- OrderService → HTTP GET → CustomerService (validate customer)
- OrderService → HTTP GET → ProductService (validate product)
- OrderService → Publish event → RabbitMQ → NotificationService (async)
- Each service has its own SQLite database (NO direct DB access between services)
```

---

## Services Overview

| Service | Port | Database | Description |
|---------|------|----------|-------------|
| **API Gateway** | 5000 | — | Ocelot-based unified routing, Swagger aggregation |
| **CustomerService** | 5001 | customers.db | CRUD operations for customer records |
| **OrderService** | 5002 | orders.db | Create orders with validation, publish events |
| **ProductService** | 5003 | products.db | Product catalog and inventory management |
| **NotificationService** | 5004 | notifications.db | Listen for OrderCreated events from RabbitMQ |
| **RabbitMQ** | 5672 / 15672 | — | Message broker for async communication, Admin UI |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (includes Docker and Docker Compose)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/) (optional, for development)
- Git for cloning the repository

---

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/chandra-lab/DistributedDataAccessLab.git
cd DistributedDataAccessLab
```

### 2. Option A: Run with Docker Compose (Recommended)

```bash
docker compose up --build
```

**What this does:**
- Builds Docker images for all 4 services + API Gateway
- Starts RabbitMQ container
- Creates and initializes SQLite databases
- Launches all containers on their respective ports
- First run takes 2-3 minutes to download base images

### 2. Option B: Run Services Locally (Development)

**Prerequisites:** .NET 10 SDK installed locally

```bash
# Terminal 1: Start RabbitMQ using Docker
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management

# Terminal 2: CustomerService
cd CustomerService/CustomerService.Api
dotnet run

# Terminal 3: ProductService
cd ProductService/ProductService.Api
dotnet run

# Terminal 4: OrderService
cd OrderService/OrderService.Api
dotnet run

# Terminal 5: NotificationService
cd NotificationService/NotificationService.Api
dotnet run

# Terminal 6: API Gateway
cd ApiGateway
dotnet run
```

### 3. Verify Services Are Running

```bash
docker ps
```

You should see 5 containers running:
- customerservice
- orderservice
- productservice
- notificationservice
- rabbitmq

### 4. Access the APIs

**Individual Service Swagger UI:**

| Service | URL |
|---------|-----|
| CustomerService | http://localhost:5001/swagger |
| OrderService | http://localhost:5002/swagger |
| ProductService | http://localhost:5003/swagger |
| NotificationService | http://localhost:5004/swagger |

**API Gateway Unified Documentation:**

| Component | URL |
|-----------|-----|
| API Gateway Swagger | http://localhost:5000/swagger |
| RabbitMQ Management | http://localhost:15672 |
| RabbitMQ Credentials | guest / guest |

---

## Project Structure

```
DistributedDataAccessLab/
│
├── .gitignore                      # Git ignore file
├── docker-compose.yml              # Multi-container orchestration
├── DistributedDataAccessLab.sln    # Visual Studio solution file
├── README.md                        # This file
│
├── ApiGateway/                     # Unified API Gateway (Port 5000)
│   ├── Properties/                 # Build properties
│   │   └── launchSettings.json
│   ├── ApiGateway.csproj          # Project file
│   ├── ApiGateway.http            # HTTP test file
│   ├── Dockerfile                 # Docker configuration
│   ├── Program.cs                 # Main entry point with Ocelot setup
│   ├── ocelot.json                # Route configuration
│   ├── appsettings.json           # Configuration
│   └── appsettings.Development.json
│
├── CustomerService/               # Customer Management (Port 5001)
│   └── CustomerService.Api/
│       ├── Properties/
│       │   └── launchSettings.json
│       ├── Controllers/
│       │   └── CustomersController.cs      # CRUD endpoints for customers
│       ├── Data/
│       │   └── CustomerDbContext.cs        # EF Core DbContext
│       ├── Models/
│       │   └── Customer.cs                 # Customer entity
│       ├── DTOs/                           # Data Transfer Objects
│       │   ├── CreateCustomerDto.cs
│       │   └── UpdateCustomerDto.cs
│       ├── Migrations/                     # EF Core migrations
│       │   └── [Migration files]
│       ├── CustomerService.Api.csproj     # Project file
│       ├── CustomerService.Api.http       # HTTP test file
│       ├── Dockerfile                     # Docker configuration
│       ├── Program.cs                     # Startup and DI configuration
│       ├── appsettings.json              # Configuration
│       └── appsettings.Development.json
│
├── OrderService/                  # Order Management (Port 5002)
│   └── OrderService.Api/
│       ├── Properties/
│       │   └── launchSettings.json
│       ├── Controllers/
│       │   └── OrdersController.cs        # CRUD endpoints for orders
│       ├── Data/
│       │   └── OrdersDbContext.cs         # EF Core DbContext
│       ├── Models/
│       │   └── Order.cs                   # Order entity
│       ├── DTOs/
│       │   ├── CreateOrderDto.cs
│       │   └── UpdateOrderDto.cs
│       ├── Services/                      # Business logic services
│       │   ├── ICustomerClient.cs         # Interface for customer validation
│       │   ├── CustomerClient.cs          # HTTP client for CustomerService
│       │   ├── IProductClient.cs          # Interface for product validation
│       │   ├── ProductClient.cs           # HTTP client for ProductService
│       │   ├── IEventPublisher.cs         # Interface for event publishing
│       │   └── RabbitMqPublisher.cs       # RabbitMQ event publisher
│       ├── Migrations/                    # EF Core migrations
│       │   └── [Migration files]
│       ├── OrderService.Api.csproj       # Project file
│       ├── OrderService.Api.http         # HTTP test file
│       ├── Dockerfile                    # Docker configuration
│       ├── Program.cs                    # Startup and DI configuration
│       ├── appsettings.json             # Configuration
│       └── appsettings.Development.json
│
├── ProductService/                # Product Management (Port 5003)
│   └── ProductService.Api/
│       ├── Properties/
│       │   └── launchSettings.json
│       ├── Controllers/
│       │   └── ProductsController.cs      # CRUD endpoints for products
│       ├── Data/
│       │   └── ProductDbContext.cs        # EF Core DbContext
│       ├── Models/
│       │   └── Product.cs                 # Product entity
│       ├── DTOs/
│       │   ├── CreateProductDto.cs
│       │   └── UpdateProductDto.cs
│       ├── Migrations/                    # EF Core migrations
│       │   └── [Migration files]
│       ├── ProductService.Api.csproj     # Project file
│       ├── Dockerfile                    # Docker configuration
│       ├── Program.cs                    # Startup and DI configuration
│       └── appsettings.json             # Configuration
│
└── NotificationService/           # Notification Handler (Port 5004)
    └── NotificationService.Api/
        ├── Properties/
        │   └── launchSettings.json
        ├── Controllers/
        │   └── NotificationsController.cs # CRUD endpoints for notifications
        ├── Data/
        │   └── NotificationDbContext.cs   # EF Core DbContext
        ├── Models/
        │   └── Notification.cs            # Notification entity
        ├── DTOs/
        │   ├── CreateNotificationDto.cs
        │   └── NotificationDto.cs
        ├── Services/                      # Business logic services
        │   ├── OrderCreatedConsumer.cs    # RabbitMQ event consumer (BackgroundService)
        │   └── OrderCancelledConsumer.cs  # Future: Order cancellation handler
        ├── Migrations/                    # EF Core migrations
        │   └── [Migration files]
        ├── NotificationService.Api.csproj # Project file
        ├── Dockerfile                     # Docker configuration
        ├── Program.cs                     # Startup and DI configuration
        └── appsettings.json              # Configuration
```

---

## Key Design Decisions

### 1. Database-per-Service Pattern
Each microservice maintains its own **isolated SQLite database** with a dedicated DbContext. This ensures:
- **Data Autonomy:** No service can directly access another's data
- **Independent Scaling:** Each database can be optimized separately
- **Schema Evolution:** Services can evolve their schemas independently
- **Failure Isolation:** A database failure in one service doesn't cascade

**Cross-Service Data Access:**
- **Synchronous:** REST API calls with HttpClient (for real-time validation)
- **Asynchronous:** RabbitMQ events (for eventual consistency)

### 2. Synchronous Communication (REST APIs)

**When?** OrderService needs to validate that a customer and product exist before creating an order.

**How?**
```csharp
// OrdersController validates before saving
var customer = await _customerClient.GetCustomerAsync(request.CustomerId);
var product = await _productClient.GetProductAsync(request.ProductId);

if (customer == null || product == null)
    return BadRequest("Invalid customer or product");

// Only then save the order
await _context.Orders.AddAsync(order);
await _context.SaveChangesAsync();
```

**Advantages:**
- Immediate feedback (fail fast)
- Data consistency guarantee
- Simple to test and debug

**Drawbacks:**
- Creates coupling between services
- Single point of failure (if CustomerService is down, orders can't be created)

### 3. Asynchronous Communication (RabbitMQ)

**When?** OrderService needs to notify NotificationService about new orders without waiting for a response.

**How?**
```csharp
// After order is saved, publish event
await _eventPublisher.PublishOrderCreatedAsync(order);
```

**NotificationService subscribes:**
```csharp
// BackgroundService continuously listens
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    var consumer = new EventingBasicConsumer(_channel);
    consumer.Received += async (model, ea) =>
    {
        var notification = JsonSerializer.Deserialize<Notification>(body);
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    };
}
```

**Advantages:**
- Loose coupling (services don't depend on each other's availability)
- High throughput (publish-and-forget)
- Scales well (multiple subscribers possible)
- Automatic retry via RabbitMQ

**Drawbacks:**
- Eventual consistency (slight delay before notification appears)
- More complex error handling
- Requires message broker infrastructure

### 4. Entity Framework Core with Async/Await

**All database operations use async methods:**
```csharp
// ✅ GOOD: Non-blocking
var customers = await _context.Customers.ToListAsync();

// ❌ BAD: Blocks thread
var customers = _context.Customers.ToList();
```

**Benefits:**
- Non-blocking I/O (threads don't sit idle during database calls)
- Better scalability under load
- Prevents thread pool starvation
- Follows ASP.NET Core best practices

### 5. API Gateway with Ocelot

The API Gateway aggregates all service routes into a single entry point:

**Benefits:**
- **Unified API:** Clients only need to know one URL (http://localhost:5000)
- **Centralized Routing:** Easy to add, remove, or modify service routes
- **Cross-Cutting Concerns:** Authentication, rate limiting, caching can be added here
- **Service Discovery:** Routes can be updated without client code changes
- **Swagger Aggregation:** MMLib.SwaggerForOcelot combines all service documentation

**ocelot.json Routes:**
```json
{
  "Routes": [
    {
      "SwaggerKey": "customerservice",
      "UpstreamPathTemplate": "/api/customers/{everything}",
      "DownstreamHostAndPorts": [{ "Host": "customerservice", "Port": 8080 }]
    },
    // ... similar for other services
  ]
}
```

---

## Testing the Full Flow

Follow this sequence to test end-to-end functionality:

### Step 1: Create a Customer
**Endpoint:** `POST http://localhost:5001/api/customers`

**Request Body:**
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "555-1234"
}
```

**Expected Response:**
```json
{
  "id": 1,
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "555-1234"
}
```

**Verification:** Customer ID is returned (usually 1 for first customer)

---

### Step 2: Create a Product
**Endpoint:** `POST http://localhost:5003/api/products`

**Request Body:**
```json
{
  "name": "Gaming Laptop",
  "description": "High-performance laptop for gaming and development",
  "price": 999.99,
  "stock": 50
}
```

**Expected Response:**
```json
{
  "id": 1,
  "name": "Gaming Laptop",
  "description": "High-performance laptop for gaming and development",
  "price": 999.99,
  "stock": 50
}
```

**Verification:** Product ID is returned (usually 1 for first product)

---

### Step 3: Create an Order (Validates References)
**Endpoint:** `POST http://localhost:5002/api/orders`

**Request Body:**
```json
{
  "customerId": 1,
  "productId": 1,
  "quantity": 2,
  "totalPrice": 1999.98
}
```

**Expected Response:**
```json
{
  "id": 1,
  "customerId": 1,
  "productId": 1,
  "quantity": 2,
  "totalPrice": 1999.98,
  "orderDate": "2026-04-17T10:30:00Z"
}
```

**What Happens Behind the Scenes:**
1. OrderService calls `GET http://customerservice:8080/api/customers/1` → Validates customer exists
2. OrderService calls `GET http://productservice:8080/api/products/1` → Validates product exists
3. OrderService saves order to orders.db
4. OrderService publishes `OrderCreated` event to RabbitMQ
5. NotificationService (listening on RabbitMQ) receives event automatically
6. NotificationService saves a notification record to notifications.db

---

### Step 4: Verify Notification Was Created (Async Processing)
**Endpoint:** `GET http://localhost:5004/api/notifications`

**Expected Response:**
```json
[
  {
    "id": 1,
    "orderId": 1,
    "customerId": 1,
    "message": "Order #1 has been created. Thank you for your purchase!",
    "createdAt": "2026-04-17T10:30:02Z"
  }
]
```

**Key Point:** The notification appears automatically without any manual intervention. This confirms RabbitMQ event processing is working.

---

### Step 5: Test Validation Failures

**Try creating an order with invalid customer:**
```json
{
  "customerId": 999,
  "productId": 1,
  "quantity": 1,
  "totalPrice": 999.99
}
```

**Expected Response:** `400 Bad Request`
**Message:** "Customer not found" or similar validation error

**What Happens:** OrderService's HttpClient validation fails before the order is even saved to the database. This prevents orphaned orders.

---

### Step 6: Check RabbitMQ Dashboard (Optional)
**URL:** http://localhost:15672
**Credentials:** guest / guest

**What to Look For:**
- Queues section should show `order_created` queue
- Message count shows how many events have been processed
- Confirms event-driven communication is working

---

## API Reference

### API Gateway (Port 5000)

**Swagger UI:** http://localhost:5000/swagger

All routes are aggregated here. See individual services below.

---

### CustomerService (Port 5001)

**Base URL:** `http://localhost:5001/api`

| Method | Endpoint | Description | Request Body |
|--------|----------|-------------|--------------|
| GET | `/customers` | Get all customers | — |
| GET | `/customers/{id}` | Get customer by ID | — |
| POST | `/customers` | Create a new customer | `{ "name": "...", "email": "...", "phone": "..." }` |
| PUT | `/customers/{id}` | Update a customer | `{ "name": "...", "email": "...", "phone": "..." }` |
| DELETE | `/customers/{id}` | Delete a customer | — |

**Example:**
```bash
curl -X GET http://localhost:5001/api/customers/1
```

---

### ProductService (Port 5003)

**Base URL:** `http://localhost:5003/api`

| Method | Endpoint | Description | Request Body |
|--------|----------|-------------|--------------|
| GET | `/products` | Get all products | — |
| GET | `/products/{id}` | Get product by ID | — |
| POST | `/products` | Create a new product | `{ "name": "...", "description": "...", "price": 0.00, "stock": 0 }` |
| PUT | `/products/{id}` | Update a product | `{ "name": "...", "description": "...", "price": 0.00, "stock": 0 }` |
| DELETE | `/products/{id}` | Delete a product | — |

**Example:**
```bash
curl -X POST http://localhost:5003/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Laptop","description":"Gaming laptop","price":999.99,"stock":50}'
```

---

### OrderService (Port 5002)

**Base URL:** `http://localhost:5002/api`

| Method | Endpoint | Description | Request Body |
|--------|----------|-------------|--------------|
| GET | `/orders` | Get all orders | — |
| GET | `/orders/{id}` | Get order by ID | — |
| POST | `/orders` | Create a new order (validates customer & product) | `{ "customerId": 0, "productId": 0, "quantity": 0, "totalPrice": 0.00 }` |
| DELETE | `/orders/{id}` | Delete an order | — |

**Important:** Creating an order will:
1. Validate customer exists (synchronous REST call)
2. Validate product exists (synchronous REST call)
3. Save order to database
4. Publish `OrderCreated` event to RabbitMQ
5. Return 400 Bad Request if validation fails

**Example:**
```bash
curl -X POST http://localhost:5002/api/orders \
  -H "Content-Type: application/json" \
  -d '{"customerId":1,"productId":1,"quantity":2,"totalPrice":1999.98}'
```

---

### NotificationService (Port 5004)

**Base URL:** `http://localhost:5004/api`

| Method | Endpoint | Description | Request Body |
|--------|----------|-------------|--------------|
| GET | `/notifications` | Get all notifications | — |
| GET | `/notifications/{id}` | Get notification by ID | — |
| POST | `/notifications` | Create a notification manually | `{ "orderId": 0, "customerId": 0, "message": "..." }` |
| DELETE | `/notifications/{id}` | Delete a notification | — |

**Note:** Most notifications are created automatically by the `OrderCreatedConsumer` background service when RabbitMQ events arrive.

**Example:**
```bash
curl -X GET http://localhost:5004/api/notifications
```

---

## Docker Commands

### Start & Stop

```bash
# Start all services with rebuild (first time or after code changes)
docker compose up --build

# Start without rebuilding
docker compose up

# Start in background (detached mode)
docker compose up -d

# Stop all containers
docker compose down

# Stop and remove all volumes (fresh database, careful!)
docker compose down -v

# Stop and remove volumes with images
docker compose down -v --rmi all
```

### Inspect & Debug

```bash
# View running containers
docker ps

# View all containers (including stopped)
docker ps -a

# View logs for a specific service
docker logs customerservice
docker logs orderservice
docker logs notificationservice
docker logs rabbitmq
docker logs apigateway

# Stream logs in real-time
docker logs -f orderservice

# View logs for all services
docker compose logs -f

# Execute command in running container
docker exec -it orderservice /bin/sh

# Inspect container details
docker inspect customerservice

# View resource usage
docker stats
```

### Clean Up

```bash
# Remove stopped containers
docker container prune

# Remove unused images
docker image prune

# Remove unused volumes
docker volume prune

# Full cleanup (careful!)
docker system prune -a
```

---

## Technologies Used

| Technology | Version | Purpose |
|------------|---------|---------|
| **ASP.NET Core** | 10.0 | Web API framework and runtime |
| **Entity Framework Core** | 10.0 | ORM for database access and migrations |
| **SQLite** | (via EF Core) | Per-service lightweight relational database |
| **RabbitMQ** | 6.8.1 | Asynchronous message broker |
| **Ocelot** | Latest | API Gateway and routing |
| **MMLib.SwaggerForOcelot** | Latest | Swagger aggregation for API Gateway |
| **Docker** | Latest | Container runtime and deployment |
| **Docker Compose** | Latest | Multi-container orchestration |
| **Swagger / OpenAPI** | 3.0 | API documentation and testing UI |
| **C#** | 12.0 | Primary programming language |
| **HTTP/REST** | — | Service-to-service communication |

---

## Common Issues & Troubleshooting

### Services Won't Start

**Issue:** Containers keep restarting or exiting immediately

**Solution:**
```bash
# Check logs
docker compose logs orderservice

# Rebuild images
docker compose down -v
docker compose up --build
```

---

### Port Already in Use

**Issue:** Error "Bind for 0.0.0.0:5001 failed"

**Solution:**
```bash
# Find which process is using the port
lsof -i :5001  # macOS/Linux
netstat -ano | findstr :5001  # Windows

# Kill the process or change port in docker-compose.yml
```

---

### RabbitMQ Connection Failed

**Issue:** "Failed to connect to RabbitMQ broker"

**Solution:**
```bash
# Ensure RabbitMQ container is running
docker ps | grep rabbitmq

# Check RabbitMQ logs
docker logs rabbitmq

# Restart RabbitMQ
docker restart rabbitmq
```

---

### Database Not Initialized

**Issue:** "Database table 'Customers' doesn't exist"

**Solution:**
```bash
# EF Core creates tables automatically on first run
# If not working, manually delete containers and rebuild

docker compose down -v
docker compose up --build
```

---

### Notification Not Appearing

**Issue:** Order created but no notification shows up

**Possible Causes:**
1. RabbitMQ not running → Check `docker logs rabbitmq`
2. NotificationService not running → Check `docker logs notificationservice`
3. Queue not subscribed → Check OrderCreatedConsumer in NotificationService

**Solution:**
```bash
# Check all services are running
docker ps

# Restart NotificationService
docker restart notificationservice

# Check logs
docker logs notificationservice -f
```

---

## Development Tips

### Opening in Visual Studio

1. Open `DistributedDataAccessLab.sln` in Visual Studio 2022
2. All 5 projects (CustomerService, OrderService, ProductService, NotificationService, ApiGateway) should load
3. Set startup projects (right-click solution → Configure Startup Projects → Multiple startup projects)
4. Select all 5 projects and set action to "Start"
5. Press F5 or Ctrl+F5 to start all services

### Adding New Endpoints

1. Add method to controller (e.g., `OrdersController.cs`)
2. Add corresponding repository method if needed
3. Services are registered in `Program.cs` with dependency injection
4. Swagger docs auto-generate from attributes

### Modifying Database Schema

1. Add property to model class (e.g., `Order.cs`)
2. Entity Framework Core automatically applies migrations on startup
3. Check database file in `Data/` folder (SQLite database)

### Running Tests Locally

```bash
# From service directory
cd OrderService/OrderService.Api
dotnet run
```

---

## Contributing

To contribute to this project:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test locally with Docker Compose
5. Submit a pull request

---

## License

This project is provided for educational purposes as part of CSCI 6844 - Programming for the Internet.

---

## Support

For issues or questions:
1. Check the Troubleshooting section above
2. Review service logs with `docker logs [service-name]`
3. Verify all containers are running with `docker ps`
4. Open an issue on GitHub with detailed error messages

---

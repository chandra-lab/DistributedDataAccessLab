# Distributed Data Access Lab
## Containerized E-Commerce Microservices Backend
**Course:** CSCI 6844 - Programming for the Internet  

---

## Overview

A containerized distributed backend for a simplified e-commerce platform built with ASP.NET Core 8, Entity Framework Core, Docker, Docker Compose, and RabbitMQ. The system follows the Database-per-Service pattern with four independent microservices, each owning its own database and exposing a REST API.

---

## Architecture

```
Client (Browser / Swagger)
        |
        | HTTP POST /api/orders
        v
  +----------------+         HTTP GET        +------------------+
  |  OrderService  | ----------------------> | CustomerService  |
  |   Port: 5002   |                         |   Port: 5001     |
  |  orders.db     |                         | customers.db     |
  +----------------+                         +------------------+
        |
        | HTTP GET        +------------------+
        +---------------> |  ProductService  |
        |                 |   Port: 5003     |
        |                 | products.db      |
        |                 +------------------+
        |
        | Publish: OrderCreated event
        v
  +----------------+
  |   RabbitMQ     |
  | Port: 5672     |
  | UI:   15672    |
  +----------------+
        |
        | Subscribe: order_created queue
        v
  +----------------------+
  |  NotificationService |
  |     Port: 5004       |
  |  notifications.db    |
  +----------------------+
```

---

## Services

| Service | Port | Database | Description |
|---|---|---|---|
| CustomerService | 5001 | customers.db | Manage customer records |
| OrderService | 5002 | orders.db | Create and track orders |
| ProductService | 5003 | products.db | Manage product catalog and stock |
| NotificationService | 5004 | notifications.db | Store order notifications |
| RabbitMQ | 5672 / 15672 | — | Message broker for async events |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/chandra-lab/DistributedDataAccessLab.git
cd DistributedDataAccessLab
```

### 2. Start all services

```bash
docker compose up --build
```

This will build all 4 service images, start RabbitMQ, and launch everything. First run takes a few minutes to download base images.

### 3. Access the APIs

| Service | Swagger URL |
|---|---|
| CustomerService | http://localhost:5001/swagger |
| OrderService | http://localhost:5002/swagger |
| ProductService | http://localhost:5003/swagger |
| NotificationService | http://localhost:5004/swagger |
| RabbitMQ Dashboard | http://localhost:15672 (guest / guest) |

---

## Testing the Full Flow

Follow this sequence to test everything end to end:

**Step 1 - Create a Customer** via `POST /api/customers` on port 5001:
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "555-1234"
}
```

**Step 2 - Create a Product** via `POST /api/products` on port 5003:
```json
{
  "name": "Laptop",
  "description": "Gaming laptop",
  "price": 999.99,
  "stock": 50
}
```

**Step 3 - Create an Order** via `POST /api/orders` on port 5002:
```json
{
  "customerId": 1,
  "productId": 1,
  "quantity": 2,
  "totalPrice": 1999.98
}
```

**Step 4 - Verify notification was created automatically** via `GET /api/notifications` on port 5004. A notification record should appear without any manual action, confirming the RabbitMQ event was received.

**Step 5 - Test validation** - Try creating an order with `customerId: 999`. You should get a `400 Bad Request` confirming synchronous HttpClient validation is working.

---

## Project Structure

```
DistributedDataAccessLab/
|
|-- docker-compose.yml
|-- DistributedDataAccessLab.sln
|-- README.md
|
|-- CustomerService/
|   └── CustomerService.Api/
|       |-- Controllers/       # CustomersController.cs
|       |-- Data/              # CustomerDbContext.cs
|       |-- Models/            # Customer.cs
|       |-- Program.cs
|       └── Dockerfile
|
|-- OrderService/
|   └── OrderService.Api/
|       |-- Controllers/       # OrdersController.cs
|       |-- Data/              # OrdersDbContext.cs
|       |-- Models/            # Order.cs
|       |-- Services/          # CustomerClient, ProductClient, RabbitMqPublisher
|       |-- Program.cs
|       └── Dockerfile
|
|-- ProductService/
|   └── ProductService.Api/
|       |-- Controllers/       # ProductsController.cs
|       |-- Data/              # ProductDbContext.cs
|       |-- Models/            # Product.cs
|       |-- Program.cs
|       └── Dockerfile
|
|-- NotificationService/
|   └── NotificationService.Api/
|       |-- Controllers/       # NotificationsController.cs
|       |-- Data/              # NotificationDbContext.cs
|       |-- Models/            # Notification.cs
|       |-- Services/          # OrderCreatedConsumer.cs
|       |-- Program.cs
|       └── Dockerfile
```

---

## Key Design Decisions

### Database-per-Service
Each service has its own isolated SQLite database and DbContext. No service accesses another service's database directly. Cross-service data needs are met through REST API calls or RabbitMQ events only.

### Synchronous Communication
OrderService uses typed HttpClient wrappers to validate that a customer and product exist before persisting an order. If either check fails, the request is rejected with `400 Bad Request`.

### Asynchronous Communication
After saving an order, OrderService publishes an `OrderCreated` event to RabbitMQ. NotificationService subscribes to this queue as a `BackgroundService` and automatically saves a notification record. OrderService has no direct dependency on NotificationService.

### EF Core Async
All database operations use async EF Core methods (`ToListAsync`, `FindAsync`, `AddAsync`, `SaveChangesAsync`) to keep the web server non-blocking under load.

---

## API Reference

### CustomerService (port 5001)

| Method | Endpoint | Description |
|---|---|---|
| GET | /api/customers | Get all customers |
| GET | /api/customers/{id} | Get customer by ID |
| POST | /api/customers | Create a customer |
| PUT | /api/customers/{id} | Update a customer |
| DELETE | /api/customers/{id} | Delete a customer |

### ProductService (port 5003)

| Method | Endpoint | Description |
|---|---|---|
| GET | /api/products | Get all products |
| GET | /api/products/{id} | Get product by ID |
| POST | /api/products | Create a product |
| PUT | /api/products/{id} | Update a product |
| DELETE | /api/products/{id} | Delete a product |

### OrderService (port 5002)

| Method | Endpoint | Description |
|---|---|---|
| GET | /api/orders | Get all orders |
| GET | /api/orders/{id} | Get order by ID |
| POST | /api/orders | Create an order (validates customer and product) |
| DELETE | /api/orders/{id} | Delete an order |

### NotificationService (port 5004)

| Method | Endpoint | Description |
|---|---|---|
| GET | /api/notifications | Get all notifications |
| GET | /api/notifications/{id} | Get notification by ID |
| POST | /api/notifications | Create a notification manually |
| DELETE | /api/notifications/{id} | Delete a notification |

---

## Docker Commands

```bash
# Start everything (first time or after code changes)
docker compose up --build

# Start without rebuilding
docker compose up

# Stop all containers
docker compose down

# Stop and remove volumes (fresh database)
docker compose down -v

# View logs for a specific service
docker logs orderservice
docker logs notificationservice

# View all running containers
docker ps
```

---

## Technologies Used

| Technology | Version | Purpose |
|---|---|---|
| ASP.NET Core | 10.0 | Web API framework |
| Entity Framework Core | 10.0 | ORM and database access |
| SQLite | 10.0 | Per-service database |
| RabbitMQ | 6.8.1 | Async message broker |
| Docker | - | Service containerization |
| Docker Compose | - | Multi-container orchestration |
| Swagger / OpenAPI | - | API documentation and testing |

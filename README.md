# Distributed Data Access Lab
## Full-Stack Containerized E-Commerce Microservices System
**Course:** CSCI 6844 - Programming for the Internet  
**Last Updated:** April 2026

---

## Overview

A full-stack containerized e-commerce platform built with **ASP.NET Core 10**, **Blazor WebAssembly**, **Entity Framework Core**, **Docker**, **Docker Compose**, and **RabbitMQ**.

The system follows the **Database-per-Service** pattern where each microservice owns its own SQLite database. A **Blazor WebAssembly** frontend communicates exclusively through an **Ocelot API Gateway**. Service-to-service communication happens through REST APIs (synchronous validation) and RabbitMQ (asynchronous events).

**Key Features:**
- Blazor WebAssembly frontend with smart/dumb component architecture
- 4 independent microservices with isolated data stores
- API Gateway with Ocelot for unified routing and Swagger aggregation
- CORS configured to support Blazor WASM cross-origin requests
- nginx reverse proxy in the frontend container (no hardcoded URLs — works in Codespaces)
- Synchronous inter-service validation via HttpClient
- Asynchronous event processing via RabbitMQ
- Fully containerized — single `docker compose up --build` starts everything

---

## Architecture

```
           Browser (GitHub Codespaces / localhost)
                            |
              HTTP  (port 5005 external / 80 internal)
                            |
           +----------------+----------------+
           |   Blazor WebAssembly Frontend   |
           |   (nginx · Docker container)    |
           |  Pages: Dashboard, Products,    |
           |         Customers, Orders       |
           |  Components: ProductCard,       |
           |    CustomerForm, OrderForm      |
           +----------------+----------------+
                            |
             /api/* proxied by nginx internally
                            |
              HTTP  (port 5000 external / 8080 internal)
                            |
           +----------------+----------------+
           |       API Gateway (Ocelot)      |
           |  Routes /api/* to services      |
           |  CORS: AllowAnyOrigin           |
           +---+-------+-------+--------+---+
               |       |       |        |
           /api/    /api/   /api/   /api/
          products customers orders notifications
               |       |       |        |
         +-----+  +----+  +---+--+  +--+-----------+
         |        |        |          |
         v        v        v          v
    +---------+ +------+ +------+ +------------------+
    | Product | |Cust. | |Order | | Notification     |
    | Service | |Svc   | |Svc   | | Service          |
    | :5003   | |:5001 | |:5002 | | :5004            |
    +---------+ +------+ +--+---+ +------------------+
                              |             ^
                  publish     |  subscribe  |
                              v             |
                       +------+-------------+---+
                       |         RabbitMQ        |
                       |   OrderCreated event    |
                       |   port 5672 / 15672     |
                       +-------------------------+
```

---

## Services Overview

| Service | Port | Description |
|---------|------|-------------|
| **Blazor Frontend** | 5005 | WebAssembly SPA served by nginx; communicates only via API Gateway |
| **API Gateway** | 5000 | Ocelot routing, CORS, Swagger aggregation |
| **CustomerService** | 5001 | CRUD for customer records |
| **OrderService** | 5002 | Create orders with validation; publishes RabbitMQ events |
| **ProductService** | 5003 | Product catalog and inventory management |
| **NotificationService** | 5004 | Subscribes to RabbitMQ OrderCreated events |
| **RabbitMQ** | 5672 / 15672 | Message broker + Management UI |

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

| URL | What it opens |
|-----|---------------|
| `http://localhost:5005` | Blazor frontend |
| `http://localhost:5000/swagger` | API Gateway Swagger UI |
| `http://localhost:15672` | RabbitMQ Management (guest / guest) |

> **GitHub Codespaces:** nginx proxies `/api/` calls internally to the API Gateway container. Just open the Codespaces URL for port 5005 — no configuration needed.

---

## Project Structure

```
DistributedDataAccessLab/
│
├── docker-compose.yml
├── DistributedDataAccessLab.sln
├── README.md
│
├── BlazorFrontend/                      # Blazor WebAssembly frontend (port 5005)
│   ├── App.razor
│   ├── _Imports.razor
│   ├── Program.cs                       # HttpClient DI — uses own origin via nginx proxy
│   ├── Dockerfile                       # Build: .NET SDK → Publish; Serve: nginx
│   ├── nginx.conf                       # Serves WASM + proxies /api/ → apigateway:8080
│   │
│   ├── Components/                      # DUMB — UI only, no API calls
│   │   ├── ProductCard.razor            # Displays one product via [Parameter]
│   │   ├── CustomerForm.razor           # Add form; fires EventCallback<CreateCustomerRequest>
│   │   └── OrderForm.razor             # Order form with dropdowns; fires EventCallback<CreateOrderRequest>
│   │
│   ├── Pages/                           # SMART — own state, call services, feed components
│   │   ├── Index.razor                  # Home (/)
│   │   ├── Dashboard.razor             # Dashboard (/dashboard)
│   │   ├── Products.razor              # (/products) — renders ProductCard grid
│   │   ├── Customers.razor             # (/customers) — uses CustomerForm
│   │   └── Orders.razor               # (/orders) — uses OrderForm
│   │
│   ├── Services/                        # HTTP wrappers — injected into Pages only
│   │   ├── ProductService.cs
│   │   ├── CustomerService.cs
│   │   └── OrderService.cs
│   │
│   ├── Models/AppModels.cs             # Shared DTOs
│   ├── Layout/MainLayout.razor         # Navbar + footer
│   └── wwwroot/
│       ├── index.html
│       ├── appsettings.json            # Empty — URL resolved via nginx proxy at runtime
│       └── css/app.css
│
├── ApiGateway/
│   ├── Program.cs                       # CORS (AllowAnyOrigin), Ocelot, aggregate endpoint
│   ├── ocelot.json                      # Base-path + {everything} routes per service
│   └── Dockerfile
│
├── CustomerService/CustomerService.Api/
├── OrderService/OrderService.Api/
├── ProductService/ProductService.Api/
└── NotificationService/NotificationService.Api/
```

---

## Frontend Component Architecture

### Dumb Components (`Components/`) — UI only

```razor
@* ProductCard.razor — renders what it receives, no API calls *@
[Parameter, EditorRequired]
public Product Product { get; set; } = new();
```

```razor
@* CustomerForm.razor — fires back to parent, never calls API *@
[Parameter] public EventCallback<CreateCustomerRequest> OnSubmit { get; set; }
[Parameter] public bool IsSaving { get; set; }
```

### Smart Pages (`Pages/`) — orchestration layer

```razor
@* Customers.razor — calls API, passes result to CustomerForm *@
<CustomerForm OnSubmit="HandleAddCustomer" IsSaving="isSaving" />

@code {
    private async Task HandleAddCustomer(CreateCustomerRequest req)
    {
        await CustomerSvc.CreateAsync(req);  // API call lives in the PAGE, not the component
        await LoadCustomers();
    }
}
```

---

## End-to-End Order Flow

1. User opens `/orders` → `Orders.razor` loads customers, products, and orders via `Task.WhenAll`
2. User clicks **+ Create Order** → `OrderForm` renders with dropdown data via `[Parameter]`
3. Selecting a product auto-calculates total price
4. User clicks **Place Order** → `OrderForm` fires `EventCallback` → `Orders.razor` calls `OrderService.CreateAsync()`
5. Request: Browser → nginx → apigateway:8080 → orderservice:8080
6. `OrdersController` validates customer + product via HTTP, saves order, publishes `OrderCreated` to RabbitMQ
7. `NotificationService` consumes the event asynchronously
8. UI refreshes with success message

---

## API Quick Reference (via Gateway at port 5000)

```bash
# Products
GET    /api/products
POST   /api/products        {"name":"...","description":"...","price":0.00,"stock":0}

# Customers
GET    /api/customers
POST   /api/customers       {"name":"...","email":"...","phone":"..."}

# Orders
GET    /api/orders
POST   /api/orders          {"customerId":1,"productId":1,"quantity":1,"totalPrice":0.00}

# Notifications (auto-created by RabbitMQ consumer)
GET    /api/notifications
```

---

## Common Issues

| Issue | Fix |
|-------|-----|
| Frontend blank / loading forever | Check port 5005 is forwarded in Codespaces; check browser console |
| API calls 404 | Check ocelot.json has base-path routes (not just `{everything}`) |
| Order 400 Bad Request | Customer/product ID doesn't exist — create them first |
| RabbitMQ not ready | Wait 10–15s after startup for the health check to pass |

---

## Technologies

| Technology | Purpose |
|------------|---------|
| **Blazor WebAssembly (.NET 10)** | SPA frontend |
| **nginx** | Serve WASM files + proxy `/api/` to gateway |
| **ASP.NET Core 10** | Backend microservices |
| **Ocelot** | API Gateway routing |
| **Entity Framework Core + SQLite** | Per-service database |
| **RabbitMQ** | Async event messaging |
| **Docker + Docker Compose** | Containerization |
| **Bootstrap 5** | Frontend styling |

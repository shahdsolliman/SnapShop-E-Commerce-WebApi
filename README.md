# 🛒 SnapShop - E-Commerce Web API & Admin Dashboard

[![.NET](https://img.shields.io/badge/.NET-6.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-blue.svg)](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![Redis](https://img.shields.io/badge/Redis-Caching-DC382D?logo=redis&logoColor=white)](https://redis.io/)
[![Stripe](https://img.shields.io/badge/Stripe-Payments-008CDD?logo=stripe&logoColor=white)](https://stripe.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

A scalable, secure, and production-ready E-Commerce RESTful Web API built with **ASP.NET Core 6** following **Clean Architecture (Onion Architecture)** and Domain-Driven Design principles.

Includes a complete **Admin Dashboard** for shop owners to upload daily product images, manage inventory, track orders, and process payments securely via Stripe.

---

## 🏛️ Clean Architecture Structure

```
SnapShop-E-Commerce-WebApi/
├── SnapShop.Core/               # Domain Layer (Enterprise Business Rules)
│   ├── Entities/                # Domain models (Product, Order, Basket, Identity)
│   ├── Repositories.Contract/   # Repository & Unit of Work interfaces
│   ├── Services.Contract/       # Business service contracts (Order, Payment, Token)
│   └── Specifications/          # Specification Pattern for dynamic querying
│       ├── ProductSpecs/        # Product filtering, sorting, pagination
│       └── OrderSpecs/          # Order retrieval & payment intent specs
│
├── SnapShop.Application/        # Application Layer (Application Business Rules)
│   ├── OrderService.cs          # Order lifecycle & delivery calculations
│   ├── PaymentService.cs        # Stripe PaymentIntent creation & webhooks
│   └── TokenService.cs          # JWT token generation with Role claims
│
├── SnapShop.Infrastructure/     # Infrastructure Layer (Data & External Services)
│   ├── Data/                    # EF Core StoreContext, Fluent API configurations
│   ├── Identity/                # AppIdentityDbContext, migrations & automatic seeding
│   ├── GenericRepository.cs     # Specification-ready Generic Repository
│   ├── BasketRepository.cs      # Distributed caching via Redis
│   └── UnitOfWork.cs            # Unit of Work transaction management
│
└── SnapShop.API/                # Presentation Layer (REST API & Admin Dashboard)
    ├── Controllers/             # Products, Orders, Basket, Accounts, Payments
    ├── Dtos/                    # Strongly-typed Request & Response DTOs
    ├── Errors/                  # Standardized API response & exception handling
    ├── Extensions/              # Modular Service & Middleware registrations
    ├── Helpers/                 # AutoMapper profiles, pagination, resolvers
    ├── MiddleWares/             # Global Exception handling middleware
    └── wwwroot/
        ├── admin/               # Arabic RTL Glassmorphism Admin Dashboard
        │   ├── index.html       # Products management & daily image upload
        │   ├── style.css        # Premium dark mode glassmorphism UI
        │   └── app.js           # JWT authentication & multipart upload
        └── uploads/             # Static file storage for product images
```

---

## ✨ Key Features

- **🛡️ Identity & Role-Based Authorization (RBAC)**:
  - Custom JWT tokens with embedded claims and role verification (`Admin`, `Customer`).
  - Automatic migration and role seeding (`admin@snapshop.com` / `Admin$123`).
  - Role-protected administrative endpoints.

- **🖼️ Product Management & Media Upload**:
  - Multipart form upload with image validation (format, max size, GUID randomization).
  - Search, sort, brand/category filtering with pagination via the **Specification Pattern**.
  - Admin endpoints to create and remove products along with their physical media.

- **⚡ High-Performance Redis Caching**:
  - In-memory shopping basket storage backed by StackExchange.Redis.
  - TTL-based cache expiration for temporary shopper baskets.

- **💳 Stripe Payment Gateway**:
  - PaymentIntent generation and webhook processing for order status updates.

- **📊 Modern Admin Dashboard**:
  - Clean Arabic RTL UI with live stats, instant image preview, and toast notifications.
  - Zero framework dependencies (Vanilla HTML5 / CSS3 / ES6+ JavaScript).

- **🩺 Production Readiness**:
  - Structured logging with **Serilog**.
  - Health check endpoint at `/health`.
  - Swagger / OpenAPI documentation with JWT Bearer authentication support.
- **🧪 Automated Unit Testing**:
  - Comprehensive unit test suite built with **xUnit**, **Moq**, and **FluentAssertions**.
  - 100% passing tests covering Order creation, Specification queries, and Domain entities.
  - Run with a single command: `dotnet test`.

---

## 🚀 Getting Started

### Prerequisites
- [.NET 6.0 SDK or later](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server)
- [Redis](https://redis.io/) (or run via Docker)

### Option 1: Running Locally

1. **Clone the repository**:
   ```bash
   git clone https://github.com/your-username/SnapShop-E-Commerce-WebApi.git
   cd SnapShop-E-Commerce-WebApi
   ```

2. **Configure `appsettings.json`**:
   Update `SnapShop.API/appsettings.json` with your connection strings:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=SnapShopDb;Trusted_Connection=True;MultipleActiveResultSets=true",
       "IdentityConnection": "Server=localhost;Database=SnapShopIdentityDb;Trusted_Connection=True;MultipleActiveResultSets=true",
       "Redis": "localhost:6379"
     },
     "JWT": {
       "key": "your-super-secret-key-at-least-32-characters-long",
       "validIssuer": "https://localhost:7205",
       "validAudience": "MySecuredAPIUsers",
       "DurationInDays": "30"
     }
   }
   ```

3. **Build & Run**:
   ```bash
   dotnet restore
   dotnet build
   dotnet run --project SnapShop.API
   ```

4. **Access the Application**:
   - **Swagger UI**: `https://localhost:7205/index.html`
   - **Admin Dashboard**: `https://localhost:7205/admin/index.html`
   - **Health Checks**: `https://localhost:7205/health`

---

### Option 2: Running with Docker Compose

Run the API, SQL Server 2022, and Redis with a single command:

```bash
docker compose up -d
```

---

## 🔐 Default Admin Credentials

Upon initial launch, database migrations run automatically and seed the default administrator account:

| Role | Email | Password |
| :--- | :--- | :--- |
| **Admin** | `admin@snapshop.com` | `Admin$123` |
| **Customer** | `demo@snapshop.com` | `Pa$$w0rd` |

---

## 📖 API Documentation

Detailed endpoint specifications, request payloads, and status codes are documented in [Endpoints.md](Endpoints.md).

---

## 📄 License
This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.
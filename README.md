# 🛒 SnapShop - E-Commerce Web API & Admin Dashboard

[![.NET](https://img.shields.io/badge/.NET-6.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![CI Pipeline](https://github.com/shahdsolliman/SnapShop-E-Commerce-WebApi/actions/workflows/ci.yml/badge.svg)](https://github.com/shahdsolliman/SnapShop-E-Commerce-WebApi/actions/workflows/ci.yml)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-blue.svg)](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![Redis](https://img.shields.io/badge/Redis-Caching-DC382D?logo=redis&logoColor=white)](https://redis.io/)
[![Stripe](https://img.shields.io/badge/Stripe-Payments-008CDD?logo=stripe&logoColor=white)](https://stripe.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

A scalable, secure, and production-ready E-Commerce RESTful Web API built with **ASP.NET Core 6** following **Clean Architecture (Onion Architecture)**, Domain-Driven Design principles, and enterprise security standards.

Includes a complete **Arabic RTL Glassmorphism Admin Dashboard** for shop owners to manage products, upload high-resolution media, track customer orders in real-time, and process payments securely via **Stripe**.

---

## 🏛️ Clean Architecture Structure

```
SnapShop-E-Commerce-WebApi/
├── SnapShop.Core/               # Domain Layer (Enterprise Business Rules)
│   ├── Entities/                # Product, Order, CustomerBasket, Identity (AppUser)
│   ├── Repositories.Contract/   # IGenericRepository, IBasketRepository, IUnitOfWork
│   ├── Services.Contract/       # IOrderService, IPaymentService, ITokenService
│   └── Specifications/          # Dynamic filtering, sorting, pagination specifications
│
├── SnapShop.Application/        # Application Layer (Application Business Rules)
│   ├── OrderService.cs          # Order calculations, lifecycle & delivery methods
│   ├── PaymentService.cs        # Stripe PaymentIntent creation & webhooks
│   └── TokenService.cs          # JWT token generation with role claims & UTC expiry
│
├── SnapShop.Infrastructure/     # Infrastructure Layer (Data & External Services)
│   ├── Data/                    # StoreContext (Products, Brands, Categories, Orders)
│   ├── Identity/                # AppIdentityDbContext (ASP.NET Core Identity & Roles)
│   ├── GenericRepository.cs     # Specification-ready Generic Repository
│   ├── BasketRepository.cs      # Distributed caching via Redis
│   └── UnitOfWork.cs            # Unit of Work transaction management
│
└── SnapShop.API/                # Presentation Layer (REST API & UI)
    ├── Controllers/             # Products, Orders, Basket, Accounts, Payments
    ├── Dtos/                    # Strongly-typed Request & Response DTOs
    ├── Errors/                  # Standardized API response & exception handling
    ├── Extensions/              # Modular Service & Middleware registrations
    ├── Helpers/                 # AutoMapper profiles, pagination, resolvers
    ├── MiddleWares/             # Global Exception handling middleware
    └── wwwroot/
        ├── admin/               # Arabic RTL Glassmorphism Admin Dashboard
        │   ├── index.html       # Products management & daily image upload
        │   ├── style.css        # Premium dark glassmorphism UI
        │   └── app.js           # JWT authentication & multipart upload
        └── uploads/             # Static file storage for product images
```

---

## ✨ Key Features

- **🛡️ Identity & Role-Based Authorization (RBAC)**:
  - Custom JWT tokens with embedded claims and role verification (`Admin`, `Customer`).
  - Configurable Admin credentials with fallback demo accounts for evaluation.
  - Strict password policy enforced via ASP.NET Core Identity.
  - Role-protected administrative endpoints.

- **🖼️ Product Management & Media Upload**:
  - Multipart form upload with image validation (format, max size, GUID randomization).
  - Search, sort, brand/category filtering with pagination via the **Specification Pattern**.
  - Admin endpoints to create and remove products along with physical media cleanup.

- **⚡ High-Performance Redis Caching**:
  - In-memory shopping basket storage backed by StackExchange.Redis.
  - Automatic reconnection (`AbortOnConnectFail = false`) for high availability.
  - TTL-based cache expiration for temporary shopper baskets.

- **💳 Stripe Payment Gateway**:
  - Secure PaymentIntent generation and webhook processing with cryptographic signature verification.

- **📊 Modern Admin Dashboard**:
  - Clean Arabic RTL UI with live stats, instant image preview, and toast notifications.
  - Zero framework dependencies (Vanilla HTML5 / CSS3 / ES6+ JavaScript).

- **🩺 Production Readiness**:
  - Structured logging with **Serilog**.
  - Health check endpoint at `/health`.
  - Swagger / OpenAPI documentation with JWT Bearer authentication support.
  - Debug endpoints automatically excluded in Release builds (`#if DEBUG`).

- **🧪 Automated Unit Testing**:
  - Comprehensive unit test suite built with **xUnit**, **Moq**, and **FluentAssertions**.
  - 100% passing tests covering Order creation, Specification queries, and Domain entities.
  - Automated CI execution via **GitHub Actions**.

---

## 🗄️ Database Architecture

SnapShop utilizes two independent EF Core DbContexts:

| DbContext | Primary Tables | Connection String Key |
|:----------|:---------------|:----------------------|
| **`StoreContext`** | `Products`, `ProductBrands`, `ProductCategories`, `DeliveryMethods`, `Orders`, `OrderItems` | `DefaultConnection` |
| **`AppIdentityDbContext`** | `AspNetUsers`, `AspNetRoles`, `AspNetUserClaims`, `AspNetUserRoles` | `IdentityConnection` |

> [!NOTE]
> **Single Database Support (e.g., MonsterASP.NET)**:
> In environments where only one database is available, `IdentityConnection` automatically falls back to `DefaultConnection`. Tables do not conflict, allowing both contexts to run safely on a single database.

---

## 🚀 Getting Started

### Prerequisites
- [.NET 6.0 SDK or later](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, or Docker)
- [Redis](https://redis.io/) (or Docker)

---

### Option 1: Running Locally with .NET CLI

1. **Clone the repository**:
   ```bash
   git clone https://github.com/shahdsolliman/SnapShop-E-Commerce-WebApi.git
   cd SnapShop-E-Commerce-WebApi
   ```

2. **Configure User Secrets (Recommended for Local Dev)**:
   ```bash
   cd SnapShop.API
   dotnet user-secrets init
   dotnet user-secrets set "JWT:key" "YourSuperSecretKeyAtLeast32CharactersLong!"
   dotnet user-secrets set "StripeSettings:SecretKey" "sk_test_your_stripe_key"
   dotnet user-secrets set "StripeSettings:WebhookSecret" "whsec_your_webhook_secret"
   ```

3. **Build & Run**:
   ```bash
   dotnet restore ../SnapShop.Solution.sln
   dotnet build ../SnapShop.Solution.sln
   dotnet run
   ```

4. **Access the Application**:
   - **Swagger UI**: `https://localhost:7205/index.html`
   - **Admin Dashboard**: `https://localhost:7205/admin/index.html`
   - **Health Checks**: `https://localhost:7205/health`

---

### Option 2: Running with Docker Compose

1. Copy `.env.example` to `.env`:
   ```bash
   cp .env.example .env
   ```
2. Edit `.env` with your secure passwords.
3. Start all services (SQL Server 2022, Redis 7, and Web API):
   ```bash
   docker compose up -d
   ```

---

## 🔐 Seed Accounts & Security Configuration

### Local Development Seed (Development Only)
In development mode (`ASPNETCORE_ENVIRONMENT=Development`), initial launch seeds demo accounts for testing:

| Role | Email | Password | Scope |
|:-----|:------|:---------|:------|
| **Admin** | `admin@snapshop.com` | `Admin$123` | Local Development Only |
| **Customer** | `demo@snapshop.com` | `Pa$$w0rd` | Local Development Only |

> [!CAUTION]
> **Production Security Rules**:
> - Demo accounts are **never seeded** in production (`IsProduction() == true`).
> - Production requires explicit admin credentials via `AdminUser__Email` and `AdminUser__Password`. The application enforces **Fail-Fast** and will refuse to start if these are missing.
> - Password policy enforces minimum 8 characters, uppercase, lowercase, digit, and special characters.
> - **Brute-Force Protection**: Account lockout triggers for 15 minutes after 5 consecutive failed attempts.

---

## ☁️ Deployment Guide (MonsterASP.NET / IIS)

1. **Database Setup**:
   - Create a single SQL Server database in your hosting control panel (e.g. `db80430`).
   - Copy the SQL Connection String.

2. **Environment Variables**:
   Configure the following in the host portal or `web.config`:
   - `ConnectionStrings__DefaultConnection`: `<your-db-connection-string>`
   - `JWT__key`: `<at-least-32-byte-secret-key>`
   - `JWT__validIssuer`: `https://your-domain.com`
   - `JWT__validAudience`: `MySecuredAPIUsers`
   - `FrontBaseUrl`: `https://your-frontend-domain.com`
   - `AdminUser__Email`: `<your-production-admin-email>`
   - `AdminUser__Password`: `<your-production-admin-password>`
   - `StripeSettings__SecretKey`: `<your-stripe-secret-key>`
   - `StripeSettings__WebhookSecret`: `<your-stripe-webhook-secret>`

3. **Publish & Deploy**:
   ```bash
   dotnet publish SnapShop.API/SnapShop.API.csproj -c Release -o ./publish
   ```
   Upload the contents of `./publish` to your hosting root directory. Migrations and seed data execute automatically upon startup.

---

## 🧪 Testing

Run the automated test suite with:

```bash
dotnet test SnapShop.Solution.sln --configuration Release
```

All 17 tests validate domain entity rules, dynamic specifications, and order lifecycle operations.

---

## 📖 API Documentation

Detailed endpoint specifications, request payloads, and status codes are documented in [Endpoints.md](Endpoints.md).

---

## 📄 License
This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

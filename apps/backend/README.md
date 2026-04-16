# LushThreads Backend - E-Commerce API & Web Application

A comprehensive **ASP.NET Core backend** for the LushThreads e-commerce platform, featuring a RESTful API, MVC web application, JWT authentication, Stripe payment integration, and clean architecture design.

---

## Live Demo

🌐 **Web Application & API**: [https://lushthreads.runasp.net](https://lushthreads.runasp.net)  
📚 **Swagger API Documentation**: [https://lushthreads.runasp.net/swagger](https://lushthreads.runasp.net/swagger)

---

## Project Architecture

The backend follows **Clean Architecture** principles with clear separation of concerns across multiple layers:

```
LushThreads.Backend/
├── LushThreads.Domain/          # Domain Layer (Entities, ViewModels, Constants)
├── LushThreads.Infrastructure/  # Infrastructure Layer (DbContext, Repositories, Migrations)
├── LushThreads.Application/     # Application Layer (Services, DTOs, Business Logic)
├── LushThreads.Api/             # API & Presentation Layer (Controllers, Areas, Program.cs)
└── LushThreads.Web/             # MVC Web Application (Razor Views, Tailwind CSS)
```

### Layer Responsibilities

| Layer | Project | Responsibilities |
|-------|---------|-----------------|
| **Domain** | `LushThreads.Domain` | Core business entities, view models, constants |
| **Infrastructure** | `LushThreads.Infrastructure` | Data access, EF Core DbContext, repositories, migrations, persistence |
| **Application** | `LushThreads.Application` | Business logic, services, DTOs, mapping profiles, email templates |
| **API/Presentation** | `LushThreads.Api` | REST API controllers, MVC controllers, authentication, Swagger setup |
| **Web UI** | `LushThreads.Web` | Razor views, Tailwind CSS, Bootstrap, user interface |

---

## Tech Stack

| Category | Technologies |
|----------|-------------|
| **Framework** | ASP.NET Core 8.0 (MVC + Web API) |
| **Language** | C# |
| **ORM** | Entity Framework Core (Code-First) |
| **Database** | SQL Server |
| **Authentication** | ASP.NET Identity, JWT Bearer Tokens, OAuth (Google, Facebook) |
| **Authorization** | Role-based (Admin, User), Custom Policies |
| **Payment Gateway** | Stripe |
| **Frontend (Web)** | Razor Views, Tailwind CSS, Bootstrap 5 |
| **API Documentation** | Swagger/OpenAPI |
| **Architecture Patterns** | Clean Architecture, Repository Pattern, Dependency Injection |
| **Security** | HTTPS, HSTS, Password Hashing, Email Confirmation |

---

## Key Features

### 🔐 Authentication & Authorization
- ASP.NET Identity with email confirmation
- JWT Bearer token authentication for API
- Role-based authorization (Admin, User)
- Custom authorization policies (`AdminOnly`, `UserOnly`)
- External login providers (Google, Facebook)
- Session management with distributed memory cache

### 🛍️ E-Commerce Functionality
- Product catalog with filtering by brand and category
- Shopping cart with quantity management
- Order processing and tracking
- Stripe payment integration (checkout, receipts)
- Stock management
- Order history and details

### 👤 User Management
- User registration, login, logout
- Profile management and settings
- Admin panel for user management
- Security activity tracking
- Device tracking for sessions

### 📊 Admin Dashboard
- Product management (CRUD operations)
- Category and brand management
- Order management and analytics
- User management
- Admin activity logging
- Analytics dashboard (orders, products, users)

### 📧 Communication
- Email sender service
- Email template system
- Order confirmation emails
- Stripe receipt emails

### 🔒 Security Features
- HTTPS enforcement
- HSTS headers
- Password hashing via ASP.NET Identity
- Email verification
- Admin activity logging
- Security activity tracking
- Device fingerprinting

---

## API Endpoints

The backend exposes RESTful API endpoints organized by areas:

### Customer Area
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/Customer/Auth/*` | POST | Authentication (Login, Register, Refresh Token) |
| `/api/Customer/Setting/*` | GET/PUT | User profile and settings management |

### Admin Area
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/Admin/Product/*` | CRUD | Product management |
| `/api/Admin/Category/*` | CRUD | Category management |
| `/api/Admin/Brand/*` | CRUD | Brand management |
| `/api/Admin/Order/*` | GET/PUT | Order management |
| `/api/Admin/User/*` | CRUD | User management |
| `/api/Admin/AdminActivity/*` | GET | Admin activity logs |

### Public API
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/Home/*` | GET | Home page data, featured products |
| `/api/Cart/*` | CRUD | Shopping cart operations |
| `/api/Order/*` | POST | Order creation and retrieval |

> 🔍 **Interactive API Documentation**: Visit [https://lushthreads.runasp.net/swagger](https://lushthreads.runasp.net/swagger) to explore all endpoints with live testing capabilities.

---

## Database Schema

### Core Entities
- **ApplicationUser**: Extended Identity user with profile information
- **Product**: Product details with pricing, stock, images
- **Category**: Product categorization
- **Brand**: Brand information
- **CartItem**: Shopping cart items
- **OrderHeader**: Order summary and status
- **OrderDetail**: Individual order line items
- **PaymentMethod**: Payment information
- **Stocks**: Inventory tracking
- **UserDevice**: Device tracking for sessions
- **AdminActivity**: Admin action logging
- **SecurityActivity**: Security event tracking

---

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 / VS Code / Rider
- SQL Server (LocalDB, Express, or full version)
- Stripe account (for payment integration)
- Google/Facebook developer accounts (optional, for OAuth)

### Installation Steps

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd apps/backend
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore LushThreads.sln
   ```

3. **Configure connection string**
   
   Update `appsettings.json` in `LushThreads.Api`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LushThreads;Trusted_Connection=True;"
     }
   }
   ```

4. **Configure Stripe keys**
   
   Add your Stripe keys to `appsettings.json`:
   ```json
   {
     "Stripe": {
       "PublishableKey": "pk_test_...",
       "SecretKey": "sk_test_..."
     }
   }
   ```

5. **Configure JWT Settings**
   
   Add JWT configuration to `appsettings.json`:
   ```json
   {
     "Jwt": {
       "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong",
       "Issuer": "LushThreads",
       "Audience": "LushThreadsUsers"
     }
   }
   ```

6. **Apply database migrations**
   ```bash
   dotnet ef database update --project LushThreads.Infrastructure --startup-project LushThreads.Api
   ```

7. **Run the application**
   ```bash
   dotnet run --project LushThreads.Api
   ```

8. **Access the application**
   - Web Application: `https://localhost:7000`
   - Swagger API: `https://localhost:7000/swagger`

---

## Configuration Files

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "Jwt": {
    "Key": "...",
    "Issuer": "...",
    "Audience": "..."
  },
  "Stripe": {
    "PublishableKey": "...",
    "SecretKey": "..."
  },
  "Facebook": {
    "ClientId": "...",
    "ClientSecret": "..."
  },
  "Google": {
    "ClientId": "...",
    "ClientSecret": "..."
  },
  "EmailSettings": {
    "SmtpServer": "...",
    "Port": 587,
    "SenderEmail": "...",
    "Password": "..."
  }
}
```

---

## Services Overview

The Application layer contains the following core services:

| Service | Responsibility |
|---------|---------------|
| `AccountService` | User authentication, registration, token management |
| `ProductService` | Product CRUD, filtering, search |
| `CartService` | Shopping cart operations |
| `OrderService` | Order creation, processing, tracking |
| `UserService` | User management, profile updates |
| `ProfileService` | User profile and settings |
| `CategoryService` | Category management |
| `BrandService` | Brand management |
| `AdminActivityService` | Admin action logging |
| `OrderAnalyticsService` | Order statistics and reporting |
| `ProductAnalyticsService` | Product performance metrics |
| `UserAnalyticsService` | User behavior analytics |
| `EmailTemplateService` | Dynamic email generation |
| `TokenService` | JWT token generation and validation |
| `DeviceTrackingService` | Session and device management |
| `IpLocationService` | IP geolocation tracking |

---

## Development Workflow

### Adding a New Feature
1. Create/update entities in `LushThreads.Domain`
2. Add DTOs in `LushThreads.Application/DTOs`
3. Implement service logic in `LushThreads.Application/Services`
4. Define interface in `LushThreads.Application/ServiceInterfaces`
5. Create API controller in `LushThreads.Api/Areas`
6. Add migrations in `LushThreads.Infrastructure/Migrations`
7. Test via Swagger UI

### Database Migrations
```bash
# Add a new migration
dotnet ef migrations add MigrationName --project LushThreads.Infrastructure --startup-project LushThreads.Api

# Update database
dotnet ef database update --project LushThreads.Infrastructure --startup-project LushThreads.Api
```

---

## Testing

### Manual Testing
- Use Swagger UI at `/swagger` for API testing
- Navigate through the web application for UI testing
- Test payment flow with Stripe test cards

### Recommended Test Cards (Stripe)
- Success: `4242 4242 4242 4242`
- Decline: `4000 0000 0000 0002`

---

## Deployment

The application is deployed to ASP.NET hosting:

- **URL**: https://lushthreads.runasp.net
- **Environment**: Production
- **Database**: SQL Server
- **SSL**: Enabled with HTTPS redirection

---

## Security Considerations

- ✅ All sensitive data encrypted at rest and in transit
- ✅ Passwords hashed using ASP.NET Identity
- ✅ JWT tokens with expiration and refresh mechanism
- ✅ Role-based access control for all endpoints
- ✅ CORS policy configured for allowed origins
- ✅ HSTS enabled in production
- ✅ Input validation on all forms and API endpoints
- ✅ SQL injection prevention via Entity Framework parameterization

---

## Project Structure Details

### LushThreads.Domain
```
Domain/
├── Entites/           # Core business entities
│   ├── ApplicationUser.cs
│   ├── Product.cs
│   ├── OrderHeader.cs
│   └── ...
├── ViewModels/        # View-specific models
└── Constants/         # Application constants
```

### LushThreads.Infrastructure
```
Infrastructure/
├── Data/
│   ├── ApplicationDbContext.cs
│   └── DbInitializer.cs
├── Persistence/       # Repository implementations
├── Configurations/    # DI configurations
└── Migrations/        # EF Core migrations
```

### LushThreads.Application
```
Application/
├── Services/          # Business logic implementation
├── ServiceInterfaces/ # Service contracts
├── DTOs/             # Data transfer objects
├── Mapping/          # AutoMapper profiles
└── Configurations/   # DI configurations
```

### LushThreads.Api
```
Api/
├── Areas/
│   ├── Admin/        # Admin controllers
│   └── Customer/     # Customer controllers
├── Controllers/      # Public API controllers
├── Program.cs        # Application entry point
└── appsettings.json  # Configuration
```

---

## Contact & Support

For questions or issues related to the backend:
- Check the Swagger documentation at `/swagger`
- Review the code comments in service implementations
- Check application logs for error details

---

## License

This project is licensed under the MIT License.

---

## Acknowledgments

Built with ❤️ using ASP.NET Core, Entity Framework, and modern web technologies.

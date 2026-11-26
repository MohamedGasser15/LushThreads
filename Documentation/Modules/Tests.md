# Tests Reference (LushThreads.Tests)

---

## Overview

### Purpose
Reference for the automated test suite (`LushThreads.Tests`), covering the 5 source projects (`Domain`, `Application` (all 17 services), `Infrastructure` (repositories & `CatalogSeeder`), `Web` (controllers), and `Seeder`) inside the 6-project `LushThreads.sln`.

### Business Objective
Provide a fast (<1s), self-contained regression gate verifying business logic, Clean Architecture boundaries, EF Core repository behavior, and MVC controller routing without requiring a live SQL Server instance, Stripe API, or SMTP server.

---

## Project Setup (`LushThreads.Tests.csproj`)

| Aspect | Value |
|---|---|
| Target Framework | `net8.0` |
| Test Framework | **xUnit** (`[Fact]` × 55) |
| Mocking | **Moq `4.21.0`** |
| In-Memory Database | **Microsoft.EntityFrameworkCore.InMemory `8.0.14`** |
| Project References | `Domain`, `Application`, `Infrastructure`, `Web`, `Seeder` |

**Run Commands:**
```bash
# Run all 55 tests
dotnet test LushThreads.Tests/LushThreads.Tests.csproj -c Release

# Run with XPlat Code Coverage
dotnet test LushThreads.Tests/LushThreads.Tests.csproj -c Release --collect:"XPlat Code Coverage"
```

---

## Coverage Map (23 Files, 55 Tests)

### 1. Domain Layer (`DomainTests.cs` — 4 tests)
- `Product.IsNewArrival` default 30-day window & custom `NewArrivalDurationDays` override.
- `CartItem.Total` computed subtotal (`Quantity * Product.Product_Price`).
- `SD` role and order status constants.

### 2. Application Layer — All 17 Services (42 tests across 17 files)

| Test File | Tests | Verified Behaviors |
|---|---|---|
| `AccountServiceTests.cs` | 3 | Login missing-user failure, `RegisterUserAsync` (`UserName = Email`, allows duplicate `Name`), external login `remoteError` handling |
| `AdminActivityServiceTests.cs` | 2 | `LogActivityAsync` persistence, `DeleteOldActivitiesAsync` cutoff deletion count |
| `BrandServiceTests.cs` | 3 | `GetAllBrandsAsync`, `GetBrandByIdAsync` null check, `CreateBrandAsync` + admin activity logging |
| `CartServiceTests.cs` | 5 | `AddToCartAsync` insufficient stock failure & valid insert, `DecreaseQuantityAsync` deletion at qty=1, `UpdateSizeAsync` invalid size guard, `ClearCartAsync` |
| `CategoryServiceTests.cs` | 3 | `GetAllCategoriesAsync`, `GetCategoryByIdAsync`, `GetMainCategoriesAsync` top-level filter |
| `DeviceTrackingServiceTests.cs` | 2 | `GetClientIpAddress` (`X-Forwarded-For` priority), `TrackUserDeviceAsync` new device + security log |
| `EmailTemplateServiceTests.cs` | 2 | `GenerateEmailConfirmationEmail` & `GetOrderShippedEmail` HTML content |
| `HomeServiceTests.cs` | 2 | `GetHomeViewModelAsync` featured + new arrivals mapping, `GetShopProductsAsync` pagination |
| `IpLocationServiceTests.cs` | 2 | Localhost fallback & mocked HTTP JSON parsing (`Cairo, Egypt`) |
| `OrderAnalyticsServiceTests.cs` | 2 | `GetOrderStatusData` bucket mapping, `GetRevenueTrendData` label/revenue projection |
| `OrderServiceTests.cs` | 4 | `GetAllOrdersAsync`, `ApproveOrderAsync` status/payment update + audit log, `ShipOrderAsync` invalid state exception, `DeliverOrderAsync` |
| `ProductAnalyticsServiceTests.cs` | 1 | `GetProductAnalytics` key metrics, low-stock count & top-selling product |
| `ProductServiceTests.cs` | 2 | `ToggleFeaturedAsync`, `GetProductByIdAsync` null check |
| `ProfileServiceTests.cs` | 3 | `IsValidEmail`, `VerifyEmailCodeAsync`, `VerifyAndEnable2FAAsync` expiry check |
| `SettingServiceTests.cs` | 2 | `ChangeLanguageAsync`, `ChangeCurrencyAsync` missing user failure |
| `UserAnalyticsServiceTests.cs` | 1 | `GetUserGrowthData` timeline alignment |
| `UserServiceTests.cs` | 2 | `GetAllUsersAsync` single-query role mapping (`GetUserRolesMapAsync`), `LockUnlockUserAsync` |

### 3. Infrastructure Layer (`InfrastructureTests.cs` — 3 tests, EF Core InMemory)
- `BrandRepository_CrudOperationsWork`: Full Create/Get/Update/Delete cycle against `ApplicationDbContext`.
- `RoleRepository_GetUserRolesMapAsync_ReturnsExpectedRoles`: Verifies single-query `UserRoles ⋈ Roles` join.
- `CatalogSeeder_SeedsCatalogFromJson_WhenEmpty`: Verifies `catalog.json` deserialization & 2-pass category seeding into EF InMemory.

### 4. Presentation / Web Controllers (`AccountControllerTests.cs` + `WebControllerTests.cs` — 6 tests)
- `AccountControllerTests`: Admin login redirects to `/Admin/Analytics`, regular user redirects to `/`, `ExternalLoginCallback` with `isPopup: true` returns `text/html` `postMessage` script.
- `WebControllerTests`: `HomeController.About`, `HomeController.Shop` (`pageSize = 18`), `BrandController.Index`.

### 5. Seeder Layer (`SeederTests.cs` — 1 test)
- `SeedOptions_FromUserCount_ComputesProportionalTargets` (verifies `2000 → 300/1500/1600/700/800`).

### 6. Shared Helpers (`TestHelpers.cs`)
- `CreateUserManagerMock()`, `CreateRoleManagerMock()`, `CreateSignInManagerMock(UserManager)`.

---

## Verified Test & Coverage Results

- **Latest Run:** `Passed: 55, Failed: 0, Skipped: 0, Total: 55` (Duration: **806 ms**).
- **XPlat Code Coverage (`coverlet`):**
  - `LushThreads.Domain`: **52.6%**
  - `LushThreads.Application`: **26.7%**
  - `LushThreads.Web`: **4.6%** (includes Razor `.cshtml` compiled lines)
  - `LushThreads.Seeder`: **3.2%**
  - `LushThreads.Infrastructure`: **1.9%** (includes generated EF Core migration designer files)

---

## Technical Notes

1. **No Live Dependencies:** Tests mock `UserManager`, `SignInManager`, `RoleManager`, `IHttpClientFactory`, `IEmailSender`, and repositories; EF Core InMemory tests `ApplicationDbContext` queries without touching SQL Server.
2. **InMemory Semantics:** EF Core InMemory does not enforce SQL Server foreign-key constraints or `IDENTITY_INSERT` rules, so SQL-specific behavior should still be verified manually against SQL Server.

---

## Change Log

**Current functionality (verified):** 55 passing tests across 23 files covering all 5 source projects (`Domain`, `Application`, `Infrastructure`, `Web`, `Seeder`) in `LushThreads.sln`.

# Repositories & Data Access Reference (`LushThreads.Infrastructure`)

---

## Overview

### Purpose
Complete reference of the persistence, database seeding, SMTP email engine, and repository pattern implementation in `LushThreads.Infrastructure` — covering `ApplicationDbContext`, generic `Repository<T>`, all 13 specialized repositories, `EmailSender`, `DbInitializer`, `CatalogSeeder`, `StripeSettings`, and `InfrastructureContainer`.

### Architecture Boundaries
- Depends on `LushThreads.Domain` and `LushThreads.Application`.
- Registered via `AddInfrastructureServices(IConfiguration)` (`InfrastructureContainer.cs:26-69`).
- Target engine: **SQL Server** via `Microsoft.EntityFrameworkCore.SqlServer 10.0.15` (and `InMemory 10.0.15` in `LushThreads.Tests`).

---

## Folder Structure

```
LushThreads.Infrastructure/
├── LushThreads.Infrastructure.csproj        # EF Core 8, Identity, MailKit 4.16.0
├── Configurations/
│   ├── InfrastructureContainer.cs           # 71 lines — DbContext, Identity, Stripe, 14 repos, EmailSender
│   └── StripeSettings.cs                    # 14 lines — SecretKey / PublishableKey options class
├── Data/
│   ├── ApplicationDbContext.cs              # 118 lines — 12 DbSets + CartItem FKs + initial HasData seed
│   ├── CatalogSeeder.cs                     # 262 lines — Seeds real catalog from SeedData/catalog.json
│   ├── DbInitializer.cs                     # 175 lines — Roles, default Admin, CatalogSeeder trigger
│   └── SeedData/catalog.json                # Embedded real clothing catalog payload
├── Migrations/                              # EF Core code-first migration history
├── Persistence/Repository/
│   ├── Repository.cs                        # 438 lines — Generic async CRUD + include/tracking/cancellation
│   ├── AdminActivityRepository.cs           #  22 lines
│   ├── BrandRepository.cs                   #  15 lines
│   ├── CartItemRepository.cs                #  27 lines
│   ├── CategoryRepository.cs                #  34 lines
│   ├── OrderDetailRepository.cs             # 162 lines — Sales/top-product analytics aggregations
│   ├── OrderRepository.cs                   # 284 lines — Stock adjustments + 6 analytics projections
│   ├── PaymentMethodRepository.cs           #  27 lines
│   ├── ProductRepository.cs                 #  40 lines — GetTotalProductsCountAsync
│   ├── RoleRepository.cs                    #  79 lines — Batch GetUserRolesMapAsync
│   ├── SecurityActivityRepository.cs        #  27 lines
│   ├── StockRepository.cs                   #  83 lines — Inventory valuation & low/out-of-stock queries
│   ├── UserDeviceRepository.cs              #  42 lines — GetByUserAndTokenAsync
│   └── UserRepository.cs                    #  81 lines — Active/locked/date-range user analytics queries
└── Services/
    └── EmailSender.cs                       #  56 lines — MailKit SMTP client (GoogleSMTP config)
```

---

## 1. `ApplicationDbContext` (`Data/ApplicationDbContext.cs:12-118`)

Inherits `IdentityDbContext` (`:12`) and exposes **12 DbSets** (`:17-29`):
`ApplicationUsers`, `Products`, `Categories`, `Brands`, `UserDevices`, `SecurityActivities`, `CartItems`, `OrderHeaders`, `OrderDetails`, `PaymentMethods`, `Stocks`, `AdminActivities`.

### Fluent API Configuration (`OnModelCreating`, `:34-116`)
- **`CartItem → Product` (`:37-41`):** Explicit FK on `ProductId` with `DeleteBehavior.Cascade`.
- **`CartItem → ApplicationUser` (`:43-46`):** Explicit FK on `UserId` with inverse `u.CartItems`.
- **EF Core `HasData` Snapshot (`:47-115`):**
  - 3 Categories (`1`: `T-Shirts`, `2`: `Pantalon`, `3`: `Shorts`).
  - 3 Brands (`1`: `Nike`, `2`: `Adidas`, `3`: `Puma`).
  - 3 starter Products (`1`: `T-Shirt Nike` \$100, `2`: `Pantalon Adidas` \$200, `3`: `Shorts Puma` \$150).

---

## 2. Generic Repository (`Persistence/Repository/Repository.cs`, 438 lines)

Implements `IRepository<T>` where `T : class` (`:15`) using `ApplicationDbContext` and optional `ILogger<Repository<T>>` (two constructors `:30` and `:37`).

| Method | Line | Parameters | Behavior |
|---|---|---|---|
| `GetAllAsync` | `:54` | `filter?`, `includeProperties?`, `orderBy?`, `isTracking = true`, `cancellationToken` | Applies `AsNoTracking()` when `!isTracking`, eager-loads comma-separated `includeProperties`, filters, orders, and awaits `ToListAsync`. |
| `GetAsync` | `:135` | `filter?`, `includeProperties?`, `isTracking = true`, `cancellationToken` | Single-record variant using `FirstOrDefaultAsync`. |
| `AnyAsync` | `:201` | `predicate`, `cancellationToken` | Async existence check (`AnyAsync`). |
| `CreateAsync` | `:244` | `T entity`, `cancellationToken` | Calls `_dbSet.AddAsync` then `SaveAsync`. |
| `UpdateAsync` | `:275` | `T entity`, `cancellationToken` | Attaches if detached (`EntityState.Modified`) and calls `SaveAsync`. |
| `DeleteAsync` | `:316` | `T entity`, `cancellationToken` | Attaches if detached, calls `_dbSet.Remove`, and awaits `SaveAsync`. |
| `DeleteRangeAsync` | `:355` | `IEnumerable<T> entities`, `cancellationToken` | Calls `_dbSet.RemoveRange` and awaits `SaveAsync`. |
| `SaveAsync` | `:401` | `cancellationToken` | Persists changes via `_db.SaveChangesAsync`. |

*Note: Because every `CreateAsync`/`UpdateAsync`/`DeleteAsync` calls `SaveAsync` immediately, operations are auto-committed per repository call rather than batched via an explicit Unit of Work.*

---

## 3. Specialized Repositories (13 Repositories)

### Thin Entity Repositories (inherit `Repository<T>` with no extra queries)
- `AdminActivityRepository` (`22 lines`, `:12-22`) → `IAdminActivityRepository`
- `BrandRepository` (`15 lines`, `:8-15`) → `IBrandRepository`
- `CartItemRepository` (`27 lines`, `:17-27`) → `ICartItemRepository`
- `CategoryRepository` (`34 lines`, `:12-34`) → `ICategoryRepository`
- `PaymentMethodRepository` (`27 lines`, `:17-27`) → `IPaymentMethodRepository`
- `SecurityActivityRepository` (`27 lines`, `:17-27`) → `ISecurityActivityRepository`

### Query-Extended Repositories

#### `OrderRepository` (`284 lines`, `:17-284`)
| Method | Line | Purpose |
|---|---|---|
| `GetOrderWithUserAsync(int orderId)` | `:37` | Loads `OrderHeader` with `ApplicationUser`. |
| `GetAllOrdersWithUserAsync()` | `:44` | Lists all `OrderHeader` rows with `ApplicationUser`. |
| `GetOrderDetailsAsync(int orderId)` | `:52` | Fetches `OrderDetail` rows for an order. |
| `GetOrderDetailsWithProductAsync(int orderId)` | `:59` | Fetches `OrderDetail` rows with `Product` included. |
| `UpdateStockForOrderDetailsAsync(details, bool increase)` | `:67` | Adjusts `Stock.Quantity` per `(ProductId, Size)` when orders are approved or cancelled. |
| `GetRevenueTrendAsync(start, end, filter, interval)` | `:95` | Aggregates `Revenue`, `Subtotal`, `ShippingFees`, `Taxes`, `OrderCount` by `day`/`week`/`month`. |
| `GetOrderStatusDistributionAsync(start, end, filter)` | `:135` | Groups orders by `OrderStatus` (`Count` + `Revenue`). |
| `GetPaymentStatusDistributionAsync(start, end, filter)` | `:151` | Groups orders by `PaymentStatus` (`Count`). |
| `GetOrdersForMetricsAsync(currStart, currEnd, prevStart, prevEnd, filter)` | `:167` | Fetches current and comparison period order slices. |
| `GetNewCustomerCountAsync(start, end, filter)` | `:184` | Counts distinct first-time buyers within the date window. |
| `GetSalesByDayOfWeekAsync(start, end, filter)` | `:202` | Groups revenue and order volume by `DayOfWeek`. |
| `GetSalesByHourAsync(start, end, filter)` | `:220` | Groups revenue and order volume across hours `0–23`. |

#### `OrderDetailRepository` (`162 lines`, `:18-162`)
| Method | Line | Purpose |
|---|---|---|
| `GetTopProductAsync(DateTime cutoffDate)` | `:38` | Returns `(ProductName, SalesCount, Revenue)?` for top seller since `cutoffDate`. |
| `GetTopProductsAsync(cutoffDate, count)` | `:59` | Product analytics top-N list (`TopProductResult`). |
| `GetSalesByCategoryAsync(DateTime cutoffDate)` | `:76` | Product analytics category breakdown (`CategorySalesResult`). |
| `GetTopProductsAsync(start, end, topCount, filter)` | `:99` | Filtered order analytics top-N list (`TopProductItem`). |
| `GetSalesByCategoryAsync(start, end, filter)` | `:132` | Filtered order analytics category breakdown (`CategorySalesItem`). |

#### `ProductRepository` (`40 lines`, `:12-40`) & `StockRepository` (`83 lines`, `:14-83`)
- `ProductRepository.GetTotalProductsCountAsync()` (`:35`): Fast `CountAsync()` on `Products`.
- `StockRepository.GetInventoryValueAsync()` (`:37`): Computes sum of `Quantity * Product.Product_Price`.
- `StockRepository.GetLowStockCountAsync(int threshold)` (`:42`) & `(int lowThreshold, int outOfStockThreshold)` (`:46`).
- `StockRepository.GetLowStockItemsAsync(int threshold)` (`:50`): Projects `LowStockItem` rows with product, category, size, and current quantity.
- `StockRepository.GetInStockCountAsync(int lowStockThreshold)` (`:73`) & `GetOutOfStockCountAsync(int outOfStockThreshold)` (`:78`).

#### `UserRepository` (`81 lines`), `RoleRepository` (`79 lines`), `UserDeviceRepository` (`42 lines`)
- `UserRepository`: `GetTotalUsersCountAsync()` (`:42`), `GetNewUsersCountAsync(start, end)` (`:48`), `GetActiveUsersCountAsync()` (`:56`), `GetLockedUsersCountAsync()` (`:64`), `GetUsersByDateRangeAsync(start, end)` (`:72`).
- `RoleRepository` *(standalone class — does not inherit `Repository<T>`)*:
  - `GetAllRoleNamesAsync()` (`:43`): Reads `Roles.Select(r => r.Name)`.
  - `GetUserRolesMapAsync()` (`:60`): Single LINQ join across `UserRoles ⋈ Roles` returning `Dictionary<string, string>` to eliminate N+1 role queries on `User/Index`.
- `UserDeviceRepository.GetByUserAndTokenAsync(userId, deviceToken)` (`:34`): Matches a returning user's cookie device fingerprint.

---

## 4. Startup Seeding & Infrastructure Services

### `DbInitializer` (`Data/DbInitializer.cs:13-175`)
Invoked inside a DI scope in `Program.cs:63-66` on every startup:
1. Checks and creates `SD.User` (`"User"`) and `SD.Admin` (`"Admin"`) roles if missing.
2. Creates default admin user (`admin@example.com` / `Admin1234@`) and assigns `SD.Admin` role if absent.
3. Calls `CatalogSeeder.SeedCatalogAsync(context, logger)` (`CatalogSeeder.cs:60`).

### `CatalogSeeder` (`Data/CatalogSeeder.cs:11-262`)
- Reads `SeedData/catalog.json` via nested classes `CatalogSeedPayload`, `BrandSeed`, `CategorySeed`, `ProductSeed`, `StockSeed` (`:13-59`).
- Uses a two-pass category insert (top-level parents first, then child categories resolving `ParentCategoryId`) so self-referencing FK constraints hold even without `IDENTITY_INSERT`.
- Safe to call repeatedly — only populates empty catalog tables unless missing.

### `EmailSender` (`Services/EmailSender.cs:9-56`)
Implements `Microsoft.AspNetCore.Identity.UI.Services.IEmailSender`:
- Reads `GoogleSMTP:Username` and `GoogleSMTP:Password` from `IConfiguration`.
- Connects asynchronously to Gmail SMTP via **MailKit (`4.16.0`)** with HTML body (`MimeMessage` + `BodyBuilder`).

### `StripeSettings` (`Configurations/StripeSettings.cs:9-14`) & `InfrastructureContainer` (`:18-71`)
- `StripeSettings` binds configuration section `"Stripe"`.
- Identity policy (`InfrastructureContainer.cs:38-47`): `RequiredLength = 8`, `RequireDigit = true`, `RequireLowercase = false`, `RequireUppercase = false`, `RequireNonAlphanumeric = false`, `RequireUniqueEmail = true`.

---

## Hidden Behaviors & Technical Notes

1. **Auto-Save Per Call:** `Repository<T>.CreateAsync`/`UpdateAsync`/`DeleteAsync` immediately invoke `SaveAsync()`, preventing multi-table rollback unless wrapped in an explicit `IDbContextTransaction`.
2. **`RoleRepository` Exception:** `RoleRepository` is the only repository implementing a custom interface (`IRoleRepository`) directly without extending `Repository<T>`.
3. **Dual Seed Paths:** `ApplicationDbContext.OnModelCreating` holds `HasData` for 3 sample products/brands/categories, whereas `CatalogSeeder` seeds the production JSON catalog (`SeedData/catalog.json`) cleanly at runtime.

---

## Change Log

**Current functionality (verified):** Clean separation of EF Core, MailKit SMTP, Stripe options, and 13 repositories verified via `InfrastructureTests.cs` against EF Core InMemory.

# LushThreads Documentation — Modules

Per-controller, layer-by-layer, source-verified documentation for the **LushThreads** solution (`LushThreads.sln`, 6 projects). Every claim in this suite was verified directly against source files (`file.cs:line` citations), Razor views, JavaScript/CSS assets, and executed tests — no assumptions, no pattern-guessing.

---

## Structure

```
Documentation/
├── assets/
│   └── lushthreads_readme_banner.jpg       # Repository showcase banner
└── Modules/
    ├── README.md                           # This master index
    ├── MVC/
    │   ├── Customer/                       # 6 controllers — Areas/Customer/Controllers/
    │   │   ├── AccountController.md        # 469 lines — Auth, 2FA, OTP, Popup OAuth
    │   │   ├── BaseController.md           #  93 lines — Email check, DeviceToken, IP helpers
    │   │   ├── CartController.md           # 269 lines — Cart CRUD, totals, Stripe/COD checkout
    │   │   ├── HomeController.md           # 295 lines — Home, 18/page Shop, Quick View, Search, Error
    │   │   ├── ProfileController.md        # 545 lines — Profile, Security, Devices, Orders, Cancel
    │   │   └── SettingController.md        # 260 lines — Preferences & dual shipping addresses
    │   ├── Admin/                          # 7 controllers — Areas/Admin/Controllers/
    │   │   ├── ActivityController.md       #  61 lines — Admin audit feed
    │   │   ├── AnalyticsController.md      # 101 lines — Landing hub, User/Order/Product dashboards + stubs
    │   │   ├── BrandController.md          # 140 lines — Brand CRUD
    │   │   ├── CategoryController.md       # 144 lines — Self-referencing Category CRUD
    │   │   ├── OrderController.md          # 247 lines — Order lifecycle, tracking & Stripe refunds
    │   │   ├── ProductController.md        # 167 lines — Product CRUD, cropped upload & Stocks matrix
    │   │   └── UserController.md           # 150 lines — User list, role edit, delete & 10-day lockout
    │   ├── DomainEntities.md               # 12 persisted entities + ErrorViewModel + SD constants
    │   ├── Repositories.md                 # ApplicationDbContext, Repository<T>, 13 repos, DbInitializer, CatalogSeeder
    │   ├── ApplicationServices.md          # 17 services, 14 repo interfaces, ServiceContainer DI
    │   ├── Common.md                       # All 42 ViewModels, 5 JS helpers, 3 CSS files, Area layouts
    │   └── Program.md                      # Program.cs, 3 DI containers, middleware order, user-secrets schema
    ├── Seeder.md                           # LushThreads.Seeder — SeedEngine, 5 seeders, proportional targets
    └── Tests.md                            # LushThreads.Tests — 55 [Fact] tests across 23 files + coverage
```

**Total: 21 module documentation files + 1 banner asset.**

---

## MVC — Customer Area (6 Controllers · 1,931 Source Lines)

| Controller | Actions | Base Class | Auth Guard | Anti-Forgery Coverage | Module Doc |
|---|---:|---|---|---|---|
| `AccountController` | 17 | `BaseController` | `[AllowAnonymous]` | ✅ 100% of POSTs (+ `"auth"` RL on 5 endpoints) | [`MVC/Customer/AccountController.md`](./MVC/Customer/AccountController.md) |
| `BaseController` | 0 (3 helpers) | `Controller` | — | — | [`MVC/Customer/BaseController.md`](./MVC/Customer/BaseController.md) |
| `CartController` | 11 | `BaseController` | `[Authorize]` | ❌ 0 / 8 POSTs (`SummaryPOST`, cart/payment AJAX) | [`MVC/Customer/CartController.md`](./MVC/Customer/CartController.md) |
| `HomeController` | 12 | `Controller` | Public `GET` | — (all read-only `GET`) | [`MVC/Customer/HomeController.md`](./MVC/Customer/HomeController.md) |
| `ProfileController` | 21 | `BaseController` | `[Authorize]` | ⚠️ 6 / 14 POSTs (`Update*`, `Verify2FA*`, `Resend2FA*` unprotected) | [`MVC/Customer/ProfileController.md`](./MVC/Customer/ProfileController.md) |
| `SettingController` | 12 | `BaseController` | `[Authorize]` | ⚠️ 4 / 8 POSTs (`Change*` unprotected; `Address` POSTs accept `userId`) | [`MVC/Customer/SettingController.md`](./MVC/Customer/SettingController.md) |

---

## MVC — Admin Area (7 Controllers · 1,010 Source Lines)

| Controller | Actions | Auth Guard | Anti-Forgery / Mutation Status | Highlights & Findings | Module Doc |
|---|---:|---|---|---|---|
| `ActivityController` | 1 | `[Authorize(Roles = SD.Admin)]` | Read-only `GET` | Chronological `AdminActivity` viewer | [`MVC/Admin/ActivityController.md`](./MVC/Admin/ActivityController.md) |
| `AnalyticsController` | 9 | `[Authorize(Roles = SD.Admin)]` | Read-only `GET` | Admin landing target; 3 live dashboards + 3 static stubs | [`MVC/Admin/AnalyticsController.md`](./MVC/Admin/AnalyticsController.md) |
| `BrandController` | 4 | `[Authorize(Roles = SD.Admin)]` | ⚠️ `Upsert` POST ✅ / `Delete` is `GET` ❌ | Brand CRUD with `AdminActivity` logging | [`MVC/Admin/BrandController.md`](./MVC/Admin/BrandController.md) |
| `CategoryController` | 4 | `[Authorize(Roles = SD.Admin)]` | ⚠️ `Upsert` POST ✅ / `Delete` is `GET` ❌ | Missing `ModelState.IsValid` & `ViewBag` repopulation in `catch` | [`MVC/Admin/CategoryController.md`](./MVC/Admin/CategoryController.md) |
| `OrderController` | 10 | `[Authorize(Roles = SD.Admin)]` | ⚠️ 6 / 8 POSTs ✅ (`UpdateCustomerInfo`, `UpdateShippingInfo` ❌) | Fulfillment state machine + Stripe refunds + MailKit alerts | [`MVC/Admin/OrderController.md`](./MVC/Admin/OrderController.md) |
| `ProductController` | 5 | `[Authorize(Roles = SD.Admin)]` | ⚠️ `Upsert`, `MakeFeatured` ✅ / `Delete` is `GET` ❌ | Base64 cropped / `IFormFile` upload + `Stocks` matrix | [`MVC/Admin/ProductController.md`](./MVC/Admin/ProductController.md) |
| `UserController` | 5 | `[Authorize(Roles = SD.Admin)]` | ⚠️ `Edit`, `Delete` ✅ / `LockUnlock` is `GET` ❌ | Batch `GetUserRolesMapAsync`, delete devices, 10-day lockout | [`MVC/Admin/UserController.md`](./MVC/Admin/UserController.md) |

---

## Clean Architecture Layers, Tooling & Quality (7 Files)

| Document | Scope Covered | Module Doc |
|---|---|---|
| Domain Model | All **12 EF Core entities**, `ErrorViewModel`, `SD` constants, ER diagram, data annotations & typos (`Entites`, `PaymentMehtod`) | [`MVC/DomainEntities.md`](./MVC/DomainEntities.md) |
| Persistence & Seed | `ApplicationDbContext`, generic `Repository<T>`, **13 repositories**, `RoleRepository.GetUserRolesMapAsync`, `DbInitializer`, `CatalogSeeder`, MailKit `EmailSender` | [`MVC/Repositories.md`](./MVC/Repositories.md) |
| Business Logic | **17 Application services**, method-by-method signatures & `:line` table, `ServiceContainer` DI | [`MVC/ApplicationServices.md`](./MVC/ApplicationServices.md) |
| UI & Contracts | All **42 ViewModels**, 5 JS files (`external-login-popup.js`, `login.js`, etc.), 3 CSS stylesheets, layouts, `.shop-grid` | [`MVC/Common.md`](./MVC/Common.md) |
| Host & Pipeline | `Program.cs`, 3 DI containers, middleware order, security headers, hardened sessions, `"auth"` rate limiter, `user-secrets` | [`MVC/Program.md`](./MVC/Program.md) |
| CLI Seeder | `LushThreads.Seeder`: `SeedEngine`, `--users`/`--products`/`--orders`, proportional formulas, 12-table pipeline | [`Seeder.md`](./Seeder.md) |
| Test Suite | `LushThreads.Tests`: **55 `[Fact]` tests** across 23 files (`xUnit`, `Moq`, `EF Core InMemory`), XPlat coverage metrics | [`Tests.md`](./Tests.md) |

---

## Cross-Cutting Findings (Verified Against Source)

### 1. Security & Anti-Forgery Gaps
1. **Unprotected State-Changing POST Endpoints (Missing `[ValidateAntiForgeryToken]`):**
   - `CartController`: `SummaryPOST`, `AddToCart`, `RemoveFromCart`, `IncreaseQuantity`, `DecreaseQuantity`, `ClearCart`, `UpdateSize`, `ChangePayment` (`CartController.cs:102-238`).
   - `OrderController`: `UpdateCustomerInfo` (`:59-60`), `UpdateShippingInfo` (`:82-83`).
   - `ProfileController`: `UpdateName`, `UpdateEmail`, `UpdatePhone`, `UpdateAddress`, `UpdatePostalCode`, `UpdateCountry`, `Verify2FACode`, `Resend2FACode` (`ProfileController.cs:77-403`).
   - `SettingController`: `ChangeLanguage`, `ChangeCurrency`, `ChangePayment`, `ChangePreferredCarriers` (`SettingController.cs:60-128`).
2. **HTTP `GET` State Mutations in Admin Controllers:**
   - `BrandController.Delete(int id)` (`:119`), `CategoryController.Delete(int id)` (`:123`), `ProductController.Delete(int id)` (`:120`), and `UserController.LockUnlock(string userId)` (`:117`) are decorated as `GET` actions rather than `[HttpPost] [ValidateAntiForgeryToken]`.
3. **IDOR Parameter Trust in `SettingController`:**
   - `UpdatePrimaryAddress(string userId, ...)`, `UpdateSecondaryAddress`, `SetPrimaryAddress`, and `DeleteSecondaryAddress` (`SettingController.cs:151-217`) bind `userId` from form parameters instead of ignoring caller input and enforcing `_userManager.GetUserId(User)`.
4. **Weak OTP RNG in `AccountService`:**
   - `AccountService.GenerateRandomCode()` (`AccountService.cs:76`) uses `System.Random` (`new Random().Next(100000, 999999)`), unlike `ProfileService.GenerateSecureCode()` (`ProfileService.cs:483`).

### 2. Known Bugs & Stubbed Views
- **Dead Redirect on `ProfileController.Index` (`ProfileController.cs:64-69`):** Redirects to `"Edit"`, an action that does not exist on `ProfileController`.
- **Missing Validation in Admin `Upsert` POSTs:**
  - `CategoryController.Upsert` POST (`:85-116`) skips `ModelState.IsValid` and omits repopulating `ViewBag.MainCategories` inside its `catch` block.
  - `ProductController.Upsert` POST (`:74-115`) skips `ModelState.IsValid` prior to calling `UpsertProductAsync`.
- **Three Static Analytics Dashboards (`AnalyticsController.cs:84-99`):** `MarketingDashboard`, `FinancialDashboard`, and `ReportsDashboard` return static Razor views without backend queries.
- **View Casing Mismatch on Case-Sensitive Hosts:** Storefront view file `Areas/Customer/Views/Home/shop.cshtml` is lowercase while `HomeController.Shop` uses PascalCase (`HomeController.cs:92`).

### 3. Naming, Architecture & Audit Notes
- **Namespace/Directory Typo:** `LushThreads.Domain/Entites/` instead of `Entities`.
- **Property Typos:** `ApplicationUser.PaymentMehtod` (`ApplicationUser.cs:30`), lowercase `Product.brand_Id` (`Product.cs:60`), lowercase `OrderDetail.price` (`OrderDetail.cs:27`).
- **Audit Log Cross-Pollution:** `SettingService.UpdateUserPropertyAsync` (`SettingService.cs:48`) writes customer preference updates to `AdminActivity` rather than `SecurityActivity`.
- **Unused Import:** `SettingController.cs:3` retains an unused `using LushThreads.Infrastructure.Data;`.
- **Duplicated Helpers:** `CartController.CalculateOrderTotals` (`:258`) duplicates `CartService.CalculateOrderTotals` (`:59`); `BaseController.IsValidEmail` (`:29`) duplicates `ProfileService.IsValidEmail` (`:469`).

---

## Documentation Conventions

- One Markdown file per controller (`<ControllerName>.md`) split by Area (`MVC/Customer/` and `MVC/Admin/`).
- Standard controller section order: **Overview → Module Architecture → Folder Structure → Database Design → Internal Workflows (Mermaid) → Data Flow Analysis → Controllers & Endpoints → Frontend Integration → Business Rules → Security Analysis → Hidden Behaviors & Technical Notes → Configuration → Change Log**.
- Every method citation, endpoint route, ViewModel field, and security observation is backed by its verified file path and line number.

# Application Services Reference (`LushThreads.Application`)

---

## Overview

### Purpose
Complete reference of the business-logic layer (`LushThreads.Application`), covering dependency registration (`ServiceContainer.cs`), Clean Architecture boundaries, and all **17 service classes** with exact method names, verified `:line` citations, parameter contracts, and runtime behaviors.

### Architecture Boundaries
- References **`LushThreads.Domain` only** (zero reference to `LushThreads.Infrastructure`).
- Uses `<FrameworkReference Include="Microsoft.AspNetCore.App" />` and `Stripe.net 48.0.0`.
- Exposes **14 repository interfaces** (`LushThreads.Application/Interfaces/`) and **17 service contracts** (`ServiceInterfaces/`) consumed by MVC controllers and implemented in `Services/`.

---

## Folder Structure

```
LushThreads.Application/
├── LushThreads.Application.csproj
├── Configurations/
│   └── ServiceContainer.cs          # 36 lines — IMemoryCache, HttpClient, 17 scoped services
├── Interfaces/                      # 14 repository contracts (IRepository<T> + 13 specialized)
├── ServiceInterfaces/               # 17 service contracts (IAccountService .. IUserService)
├── Services/                        # 17 service implementations (verified below)
└── ViewModels/                      # 42 ViewModels across 8 feature subfolders
```

---

## Dependency Injection (`Configurations/ServiceContainer.cs`, `:9-34`)

| Registration | Line | Lifetime | Target |
|---|---|---|---|
| `AddMemoryCache()` | `:12` | Singleton | `IMemoryCache` |
| `AddHttpClient()` | `:13` | Transient/Factory | `IHttpClientFactory` (used by `IpLocationService`) |
| 17 Business Services | `:16-32` | Scoped | All `I*Service → *Service` pairs |

*(Note: `IEmailSender` is registered separately in `InfrastructureContainer.cs:67` as `Transient` `EmailSender`.)*

---

## 1. Authentication, Security & Devices

### `AccountService` (`Services/AccountService.cs`, 512 lines)

| Method | Line | Signature / Behavior |
|---|---|---|
| `ValidateLoginAsync` | `:96` | `(email, password)` → Validates existence, email verification/password via `UserManager`, checks lockout & `TwoFactorEnabled`. |
| `InitiateTwoFactorLoginAsync` | `:112` | Generates 6-digit OTP (`:76`), saves hashed/claim values via `AddOrUpdateClaimAsync` (`:83`), returns code for email dispatch. |
| `VerifyTwoFactorCodeAsync` | `:129` | Compares code via constant-time `SecureEquals` (`:63`), enforces claim expiry, executes `SignInManager.SignInAsync`, calls `TrackUserDeviceAsync`. |
| `RegisterUserAsync` | `:160` | Creates `ApplicationUser` with `UserName = model.Email` (allows duplicate `Name` values), assigns `SD.User`, generates 6-digit confirmation code. |
| `ConfirmEmailAsync` | `:198` | Validates stored OTP claim (`:63`) and marks `EmailConfirmed = true`. |
| `ResendEmailVerificationCodeAsync` | `:224` | Rotates verification OTP for unconfirmed accounts. |
| `InitiateForgotPasswordAsync` | `:252` | Generates 6-digit reset OTP + Identity password reset token. |
| `VerifyResetCodeAsync` | `:278` | Validates password reset OTP stored on user claims. |
| `ResetPasswordAsync` | `:295` | Resets password via Identity token (`ResetPasswordAsync`). |
| `ResendPasswordResetCodeAsync` | `:333` | Generates fresh 6-digit reset OTP. |
| `HandleExternalLoginCallbackAsync` | `:360` | Reads `ExternalLoginInfo` from Google/Facebook. If missing user, auto-creates `ApplicationUser` from provider email & name (`EmailConfirmed = true`), links external login, signs in directly (no extra username prompt). |
| `TrackUserDeviceAsync` | `:446` | Resolves IP (`:78`) and hostname (`:81`) and records login device context. |
| `LogoutAsync` | `:485` | Calls `SignInManager.SignOutAsync()`. |

*Security Note:* `GenerateRandomCode()` (`:76`) uses `new Random().Next(100000, 999999)` whereas `ProfileService.GenerateSecureCode()` (`:483`) uses cryptographic RNG.

### `DeviceTrackingService` (`Services/DeviceTrackingService.cs`, 186 lines) & `IpLocationService` (`47 lines`)

| Method | Line | Purpose |
|---|---|---|
| `DeviceTrackingService.TrackUserDeviceAsync` | `:44` | Checks `UserDeviceRepository.GetByUserAndTokenAsync`. Updates `LastLoginDate` or inserts new `UserDevice`, resolves location via `IIpLocationService`, writes `SecurityActivity` log, returns cookie token. |
| `DeviceTrackingService.GetClientIpAddress` | `:111` | Checks `X-Forwarded-For` header first, falls back to `HttpContext.Connection.RemoteIpAddress`. |
| User-Agent Parsers | `:143-175` | `GetOSFromUserAgent` (`:143`), `GetBrowserFromUserAgent` (`:153`), `GetFriendlyDeviceName` (`:163`), `GetDeviceType` (`:175`). |
| `IpLocationService.GetLocationFromIpAsync` | `:24` | Returns `"Local Network"` for `127.0.0.1` / `::1`; otherwise queries external IP API via `IHttpClientFactory`. |

### `ProfileService` (`Services/ProfileService.cs`, 493 lines)

| Group | Methods & Lines |
|---|---|
| Profile Reads & Edits | `GetProfileAsync` (`:69`), `UpdateNameAsync` (`:90`), `UpdateEmailAsync` (`:105` — sets `EmailConfirmed = false`), `UpdatePhoneAsync` (`:128`), `UpdateAddressAsync` (`:143`), `UpdatePostalCodeAsync` (`:158`), `UpdateCountryAsync` (`:173`). |
| Security & Verification | `GetSecurityViewModelAsync` (`:192`), `GenerateEmailVerificationCodeAsync` (`:223`), `VerifyEmailCodeAsync` (`:239`), `ChangePasswordAsync` (`:248`), `Initiate2FASetupAsync` (`:277`), `VerifyAndEnable2FAAsync` (`:296` — checks expiry), `Disable2FAAsync` (`:308`). |
| Devices | `GetUserDevicesAsync` (`:326`), `RemoveDeviceAsync` (`:337` — skips current `deviceToken`), `RemoveInactiveDevicesAsync` (`:354`). |
| Orders & Utilities | `GetUserOrdersAsync` (`:380` — paginated `(Orders, TotalCount)`), `CancelOrderAsync` (`:413`), `IsValidEmail` (`:469`), `GenerateSecureCode` (`:483`). |

### `SettingService` (`Services/SettingService.cs`, 278 lines)
- Central private helper `UpdateUserPropertyAsync` (`:48`) updates `ApplicationUser`, saves via `UserManager.UpdateAsync`, and logs an `AdminActivity` entry *(even though triggered by Customer users)*.
- Preference methods: `GetUserByIdAsync` (`:79`), `ChangeLanguageAsync` (`:86`), `ChangeCurrencyAsync` (`:106`), `ChangePaymentMethodAsync` (`:126` — sets `user.PaymentMehtod`), `ChangePreferredCarriersAsync` (`:146`).
- Address management: `UpdatePrimaryAddressAsync` (`:166`), `UpdateSecondaryAddressAsync` (`:190`), `SetPrimaryAddressAsync` (`:210` — swaps `StreetAddress` and `StreetAddress2`), `DeleteSecondaryAddressAsync` (`:247`).

### `EmailTemplateService` (`Services/EmailTemplateService.cs`, 598 lines)
Stateless HTML email builder with 8 public templates:
`GetOrderDeliveredEmail` (`:13`), `GetOrderShippedEmail` (`:56`), `GetOrderInProcessEmail` (`:104`), `GetOrderCancelledEmail` (`:152`), `GenerateEmailConfirmationEmail` (`:200`), `Generate2FASetupEmail` (`:239`), `GeneratePasswordChangeEmail` (`:278` — includes IP, device name, timestamp, reset link), `Generate2FACodeEmail` (`:475`).

---

## 2. Storefront, Cart & Order Management

### `HomeService` (`Services/HomeService.cs`, 488 lines)

| Method | Line | Verified Behavior |
|---|---|---|
| `GetHomeViewModelAsync(categoryId?)` | `:102` | Builds `FeaturedProducts`, `NewArrivals` (`OrderByDescending(p => p.Product_Id).Take(8)`), and optional filtered collection. |
| `GetHomeProductsJsonAsync(categoryId?)` | `:153` | Returns projected anonymous product payload (`:77`) for AJAX category switching on Home. |
| `GetShopProductsAsync(categoryId?, page, pageSize)` | `:196` | Applies category + subcategory tree filter (`GetSubCategoryIdsAsync` `:47`), paginates `ShopProductViewModel`, returns `(Products, TotalPages, TotalProducts)`. |
| `GetProductDetailsAsync(productId)` | `:262` | Loads `Product` with `Category`, `Brand`, and `Stocks`. |
| `GetProductDetailsJsonAsync(productId)` | `:272` | JSON payload for product Quick View modal. |
| `GetProductSizesAsync(productId)` | `:305` | Lists available `Size` strings where `Quantity > 0`. |
| `GetProductsByCategoryAsync` / `ByChildCategoryAsync` / `ByBrandAsync` | `:325`, `:365`, `:419` | URL slug matching for `ShopByCategory`, `ShopByChildCategory`, and `ShopByBrand`. |
| `SearchProductsAsync(searchTerm)` | `:472` | Case-insensitive product search by name/description/brand/category. |

### `CartService` (`Services/CartService.cs`, 516 lines)

| Method | Line | Verified Behavior |
|---|---|---|
| `CalculateOrderTotals` | `:59` | Private tuple helper: `ShippingFee = subtotal > 100 ? 0 : 10`, `Tax = subtotal * 0.08`, `Total = subtotal + shipping + tax`. |
| `ValidateStockAsync` | `:69` | Checks `StockRepository` for `Product_Id`, `Size`, and `Quantity >= requested`. |
| `GetCartItemsAsync` / `GetCartCountAsync` | `:79`, `:88` | Loads user cart with `Product` included; returns distinct/summed item count. |
| `AddToCartAsync` | `:94` | Validates stock, merges quantity if `(ProductId, Size)` already in cart or creates new `CartItem`, returns `(success, message, cartCount)`. |
| `RemoveFromCartAsync` / `IncreaseQuantityAsync` / `DecreaseQuantityAsync` / `ClearCartAsync` | `:141-178` | Mutates user cart items; `DecreaseQuantityAsync` deletes the row when `Quantity == 1`. |
| `UpdateSizeAsync(cartItemId, userId, newSize)` | `:186` | Validates stock for `newSize` and merges rows if target size already exists in the user's cart. |
| `GetCartSummaryAsync(userId)` | `:235` | Builds `CartViewModel` prefilled with user shipping fields and calculated totals. |
| `ProcessOrderAsync` | `:265` | Creates `OrderHeader` + `OrderDetail` lines; for **COD** clears cart & completes (`orderId`, confirmation URL); for **Stripe** (`CreditCard`) creates `Stripe.Checkout.SessionService` session (`PaymentMethodTypes: card`) and returns Stripe URL. |
| `ConfirmOrderAsync(orderId, sessionId, userId)` | `:395` | Verifies Stripe session `payment_status == "paid"`, sets `StatusApproved` + `PaymentStatusApproved`, extracts `PaymentMethod` last4/brand/expiry and saves to `PaymentMethodRepository`, decreases stock, and clears user cart. |

### `OrderService` (`Services/OrderService.cs`, 290 lines)
- Reads: `GetAllOrdersAsync` (`:63`), `GetOrderDetailsAsync` (`:69`).
- Edits + audit log: `UpdateCustomerInfoAsync` (`:86`), `UpdateShippingInfoAsync` (`:101`), `UpdateTrackingInfoAsync` (`:117`).
- Lifecycle + MailKit notifications:
  - `ApproveOrderAsync` (`:132`): Decreases stock (`UpdateStockForOrderDetailsAsync(..., false)`), sets approved/processing status, emails `GetOrderInProcessEmail`.
  - `CancelOrderAsync` (`:163`): Issues Stripe `RefundService` refund if `PaymentIntentId` is present, restores stock (`increase: true`), sets `StatusCancelled`/`StatusRefunded`, emails `GetOrderCancelledEmail`.
  - `ShipOrderAsync` (`:217`): Enforces valid transition, sets `StatusShipped` + `ShippingDate`, emails `GetOrderShippedEmail`.
  - `DeliverOrderAsync` (`:242`): Sets `StatusDelivered`, emails `GetOrderDeliveredEmail`.
  - `RemoveCanceledOrRefundedOrderAsync` (`:266`): Deletes cancelled/refunded order record cleanly.

---

## 3. Admin CRUD, User Administration & Analytics

| Service | File (Lines) | Methods (`:line`) |
|---|---|---|
| `BrandService` | `173` | `GetAllBrandsAsync` (`:48`), `GetBrandByIdAsync` (`:57`), `CreateBrandAsync` (`:69`), `UpdateBrandAsync` (`:98`), `DeleteBrandAsync` (`:135`) — all mutations log `AdminActivity`. |
| `CategoryService` | `184` | `GetAllCategoriesAsync` (`:49`), `GetCategoryByIdAsync` (`:58`), `GetMainCategoriesAsync(excludeId?)` (`:70`), `CreateCategoryAsync` (`:80`), `UpdateCategoryAsync` (`:109`), `DeleteCategoryAsync` (`:146`). |
| `ProductService` | `416` | `GetAllProductsAsync` (`:181`), `GetProductByIdAsync` (`:193`), `GetProductViewModelForUpsertAsync` (`:206`), `UpsertProductAsync` (`:240` — handles `IFormFile` upload & base64 cropped image via `SaveImageAsync` `:79` + multi-size `Stock` upsert), `DeleteProductAsync` (`:340`), `ToggleFeaturedAsync` (`:383`), plus private `BuildCategoryListAsync` (`:144`) & `FixLegacyProductsAsync` (`:161`). |
| `UserService` | `280` | `GetAllUsersAsync` (`:83` — uses `RoleRepository.GetUserRolesMapAsync()` in `PopulateUserRolesAsync` `:55`), `GetUserForEditAsync` (`:93`), `UpdateUserAsync` (`:117` — swaps Identity role + edits properties), `DeleteUserAsync` (`:175` — cleans up `UserDevices` first), `LockUnlockUserAsync` (`:222` — toggles 10-day `LockoutEnd`). |
| `AdminActivityService` | `145` | `GetAllActivitiesAsync` (`:46`), `GetActivitiesByUserIdAsync` (`:58`), `GetActivitiesByDateRangeAsync` (`:71`), `LogActivityAsync` (`:88`), `DeleteOldActivitiesAsync` (`:115`). |
| `UserAnalyticsService` | `287` | `GetUserAnalytics(days)` (`:50`), `GetUserGrowthData(start, end, days)` (`:69`), `GetRolesDistributionData` (`:149`), `GetAccountStatusData` (`:230`), `GetRegistrationSourcesData` (`:246`), `GetRecentActivityData` (`:262`). |
| `OrderAnalyticsService` | `340` | `GetDashboardData(filter)` (`:50`), `GetRevenueTrendData` (`:80`), `GetOrderStatusData` (`:91`), `GetTopProductsData` (`:97`), plus 8 private aggregation methods (`:109-332`) including `DetermineInterval` (`day`/`week`/`month`). |
| `ProductAnalyticsService` | `170` | `GetProductAnalytics(days = 30)` (`:54`), `GetKeyMetrics` (`:77`), `GetTopProductsData` (`:104`), `GetInventoryStatusData` (`:123` — low-stock threshold = `10`), `GetSalesByCategoryData` (`:143`), `GetLowStockItems` (`:163`). |

---

## Hidden Behaviors & Technical Notes

1. **Email Address as `UserName`:** `AccountService.RegisterUserAsync` sets `UserName = model.Email` (`AccountService.cs:160`), deliberately allowing different accounts to share the same display `Name`.
2. **Legacy Product Repair Side Effect:** `ProductService` contains a private `FixLegacyProductsAsync()` (`:161`) helper to normalize legacy product records loaded during product management.
3. **N+1 Elimination in `UserService`:** `PopulateUserRolesAsync` (`:55`) avoids calling `UserManager.GetRolesAsync(user)` inside a loop by pulling the entire `Dictionary<string, string>` from `IRoleRepository.GetUserRolesMapAsync()` in one SQL join.
4. **Notification Audit Trail Coupling:** `SettingService.UpdateUserPropertyAsync` (`:48`) writes customer preference changes into `AdminActivity` rather than `SecurityActivity`.

---

## Change Log

**Current functionality (verified):** All 17 services are decoupled from `LushThreads.Infrastructure` and covered by **42 unit tests across 17 test files** in `LushThreads.Tests`.

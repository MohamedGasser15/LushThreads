# Shared ViewModels, Scripts & UI Conventions (`Common.md`)

---

## Overview

### Purpose
Cross-cutting reference of shared presentation contracts in `LushThreads` — documenting all **42 ViewModels** across 8 subfolders in `LushThreads.Application/ViewModels/`, client-side JavaScript helpers (`wwwroot/js/`), shared stylesheets (`wwwroot/css/`), Razor Area layouts (`_Layout.cshtml`, `_ProfileLayout.cshtml`, `_SettingLayout.cshtml`, `_LoginPartialView.cshtml`), and storefront responsive UI conventions.

---

## Folder Structure

```
LushThreads.Application/ViewModels/      # 42 classes across 8 domain folders
├── Auth/              (8 classes)       # Login, Register, 2FA, Forgot/Reset, Verify codes
├── Cart/              (1 class)         # CartViewModel
├── Home/              (3 classes)       # HomeViewModel, SearchViewModel, ShopProductViewModel
├── OrderAnalytics/    (15 classes)      # Filter, Dashboard composite, Trend/Volume/Hour/Day/Category charts
├── ProductAnalytics/  (7 classes)       # Dashboard composite, KeyMetrics, InventoryStatus, LowStockItem
├── Products/          (2 classes)       # ProductViewModel, OrderVM
├── Profile/           (2 classes)       # ProfileViewModel, ChangePasswordViewModel
└── UserAnalytics/     (6 classes)       # UserAnalyticsViewModel + 5 chart/activity sub-models

LushThreads/wwwroot/
├── js/
│   ├── external-login-popup.js          # 50 lines — Popup window + postMessage origin check + fallback
│   ├── login.js                         # 31 lines — Input validation + 2s spinner delay before submit
│   ├── register.js                      # 50 lines — Password confirmation match + submit spinner
│   ├── ResendCode.js                    # 15 lines — 60s countdown timer on #resendButton
│   └── script.js                        # 16 lines — Product detail thumbnail switcher (#MainImg / .small-img)
└── css/
    ├── style.css                        # 1316 lines — Storefront CSS, .shop-grid 6/3/2 responsive layout
    ├── admin.css                        #  487 lines — Admin dashboard sidebar, tables, badges & modals
    └── InProduct.css                    #   53 lines — Product image gallery & quick-view styling
```

---

## 1. Auth ViewModels (`ViewModels/Auth/` — 8 Classes)

| Class | File (`Lines`) | Verified Fields & Validation Attributes |
|---|---|---|
| `LoginViewModel` | `LoginViewModel.cs` (`21`) | `[Required] [EmailAddress] Email`, `[Required] [DataType(Password)] Password`, `[Display("Remember Me")] bool RememberMe` |
| `RegisterViewModel` | `RegisterViewModel.cs` (`33`) | `Name` (`[Required]`, `StringLength(50, Min=3)`, `[RegularExpression(@"^[a-zA-Z\s]+$")]`), `[EmailAddress] Email`, `Password` (`Min=8`, `@".*\d.*"` at least one digit), `ConfirmPassword` (`[Compare("Password")]`), `string? ReturnUrl`, `IEnumerable<SelectListItem>? RoleList` |
| `ForgotPasswordViewModel` | `ForgotPasswordViewModel.cs` (`11`) | `[Required] [EmailAddress] Email` |
| `VerifyCodeViewModel` | `VerifyCodeViewModel.cs` (`18`) | `[Required] Email`, `[Required] Code` |
| `ResetPasswordViewModel` | `ResetPasswordViewModel.cs` (`21`) | `[Required] Email`, `NewPassword` (`Min=8`, `@".*\d.*"`), `[Compare("NewPassword")] ConfirmPassword` |
| `VerifyEmailViewModel` | `VerifyEmailViewModel.cs` (`18`) | `UserId`, `[Required] [Display("Verification Code")] Code` |
| `VerifyEmailCodeViewModel` | `VerifyEmailCodeViewModel.cs` (`20`) | `UserId`, `[Required] Code`, `Email` |
| `TwoFactorSetupViewModel` | `TwoFactorSetupViewModel.cs` (`19`) | `Code` (`[Required]`, `StringLength(6, Min=6)`, `@"^\d+$"`), `Email` |

---

## 2. Storefront, Cart, Product & Profile ViewModels (8 Classes)

### `CartViewModel` (`ViewModels/Cart/CartViewModel.cs`, `23 lines`)
- `IEnumerable<CartItem> Items`, `OrderHeader OrderHeader`, `ApplicationUser User`.

### `Home/` (`3 classes`)
- **`HomeViewModel` (`24 lines`):** `Product_Id`, `Product_Name`, `imgUrl`, `BrandName`, `IsFeatured`, `DateAdded`, `Product_Rating`, `Product_Price`, `List<string> AvailableSizes`, plus recursive lists `FeaturedProducts`, `NewArrivals`, `FilteredProducts`.
- **`ShopProductViewModel` (`24 lines`):** `ProductId`, `ProductName`, `ImgUrl`, `BrandName`, `CategoryName`, `IsFeatured`, `ProductRating`, `ProductPrice`, `List<string> AvailableSizes`.
- **`SearchViewModel` (`10 lines`):** `string SearchTerm`, `List<Product> Results`.

### `Products/` (`2 classes`)
- **`ProductViewModel` (`18 lines`):** `Product Product`, `IEnumerable<SelectListItem> BrandList`, `IEnumerable<SelectListItem> CategoryList`, `List<Stock> Stocks = new()`.
- **`OrderVM` (`15 lines`):** `OrderHeader OrderHeader`, `IEnumerable<OrderDetail> OrderDetails`.

### `Profile/` (`2 classes`)
- **`ProfileViewModel` (`40 lines`):** `Id`, `[Required] Name`, `[Required] [EmailAddress] Email`, `IsEmailConfirmed`, `IsTwoFactorEnabled`, `PhoneNumber`, `Address`, `PostalCode`, `Country`, `List<SecurityActivity> RecentSecurityActivities`, `List<UserDevice> ConnectedDevices`.
- **`ChangePasswordViewModel` (`30 lines`):** `CurrentPassword`, `NewPassword` (`Min=8`, regex `^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$` — requires upper, lower, and digit unlike `RegisterViewModel`), `ConfirmPassword` (`[Compare("NewPassword")]`).

---

## 3. Analytics ViewModels (28 Classes across 3 Subfolders)

### `OrderAnalytics/` (`15 classes`)
- **`OrderAnalyticsFilter` (`21 lines`):** `Days = 30`, `StartDate?`, `EndDate?`, `OrderStatus?`, `PaymentStatus?`, `CategoryId?`, `ProductId?`, `Country?`, `CompareWithPreviousPeriod = false`.
- **`OrderAnalyticsViewModel` (`22 lines`):** Composite aggregating `RevenueTrend`, `OrderVolume`, `OrderStatusDistribution`, `PaymentStatusDistribution`, `TopProducts`, `SalesByDayOfWeek`, `SalesByCategory`, `SalesByHour`, `KeyMetrics`, `TotalDays = 30`.
- **`KeyMetricsViewModel` (`25 lines`):** `TotalRevenue`, `RevenueChangePercentage`, `TotalOrders`, `OrderChangePercentage`, `AverageOrderValue`, `AOVChangePercentage`, `NewCustomers`, `RefundRate`, plus dictionary conversion rates (`CartToCheckout`, `CheckoutToPurchase`).
- **Chart Projections:** `RevenueTrendViewModel` / `RevenueTrendItem`, `OrderVolumeViewModel`, `OrderStatusDistributionViewModel` / `OrderStatusGroup`, `PaymentStatusDistributionViewModel` / `PaymentStatusGroup`, `TopProductsViewModel` / `TopProductItem`, `SalesByCategoryViewModel` / `CategorySalesItem`, `SalesByDayOfWeekViewModel` / `DayOfWeekSales`, `SalesByHourViewModel` / `HourlySales`.

### `ProductAnalytics/` (`7 classes`)
- `ProductAnalyticsViewModel` (`KeyMetrics`, `TopProductsData`, `InventoryStatusData`, `SalesByCategoryData`, `List<LowStockItem>`), `TopProductResult`, `CategorySalesResult`.

### `UserAnalytics/` (`6 classes`)
- `UserAnalyticsViewModel` (`UserGrowthViewModel`, `UserRolesDistributionViewModel`, `UserStatusViewModel`, `RegistrationSourcesViewModel`, `RecentActivityViewModel`, `TotalUsers`).

---

## 4. Client-Side JavaScript Assets (`wwwroot/js/`)

| Script | Lines | Key Behavior |
|---|---|---|
| `external-login-popup.js` | `50` | Hooks `form[data-external-login-popup="true"]`, opens centered `500×640` window `externalLoginPopup`, sets hidden `isPopup=true`, listens for same-origin `postMessage` (`external-login-success` → redirect, `external-login-error` → reload), falls back to full-page redirect if popup blocked. |
| `login.js` | `31` | Autofocuses `#email`, disables `#loginButton` when empty, adds an intentional `2000ms` spinner delay on submit. |
| `register.js` | `50` | Client-side password match check (`#passwordError`), button gating, `2000ms` submit delay. |
| `ResendCode.js` | `15` | 60-second countdown timer keeping `#resendButton` disabled until `0`. |
| `script.js` | `16` | Swaps `#MainImg.src` when clicking `.small-img[0..3]` thumbnails in product details. |

---

## 5. Shared Layouts & Responsive UI Conventions

- **Customer Area Layouts:**
  - `Areas/Customer/Views/Shared/_Layout.cshtml` (`677 lines`): Navbar, cart badge counter, `_LoginPartialView.cshtml` (`147 lines`), footer.
  - `_ProfileLayout.cshtml` (`377 lines`): Profile sidebar wrapper for Account/Security/Devices/Orders/Payments/Contact.
  - `_SettingLayout.cshtml` (`376 lines`): Settings sidebar wrapper for Preferences/Shipping/Notifications/Privacy.
- **Admin Area Layouts:** `Areas/Admin/Views/Shared/_Layout.cshtml` (`375 lines`) + `_LoginPartialView.cshtml` (`66 lines`) styled with `wwwroot/css/admin.css` (`487 lines`).
- **Storefront Shop Grid (`style.css` & `shop.cshtml` `1614 lines`):**
  - Uses explicit CSS Grid `.shop-grid` (`6` columns desktop, `3` tablet, `2` mobile) instead of `pro-container` so `pageSize = 18` renders balanced rows (`3×6`, `6×3`, `9×2`) with zero trailing gap.
  - Featured carousel / Quick View interactions call `HomeController.GetProductDetails` & `GetProductSizes`.

---

## Change Log

**Current functionality (verified):** All 42 ViewModels, 5 JS scripts, 3 CSS stylesheets, and Customer/Admin shared layouts documented directly from source.

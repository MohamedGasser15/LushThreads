# LushThreads

A full-featured e-commerce platform with a modern mobile app and robust backend API.

## Live Demo

🌐 **Backend:** [https://lushthreads.runasp.net](https://lushthreads.runasp.net)

---

## Project Structure

```
/workspace
├── apps/
│   ├── backend/      # ASP.NET MVC Web API & Admin Panel
│   └── mobile/       # Flutter Mobile Application
└── README.md
```

---

## Applications

### 🖥️ Backend

**Location:** `apps/backend`

A full-featured E-Commerce web application built with:
- **ASP.NET MVC** & **C#**
- **Entity Framework** (Code-First)
- **Tailwind CSS** & **Bootstrap**
- **Stripe** Payment Integration
- **ASP.NET Identity** for Authentication

**Key Features:**
- User Authentication & Authorization
- Email Confirmation
- Shopping Cart Management
- Product Catalog with filtering
- Admin Panel for managing products, categories, and users
- Secure Stripe payment processing

[View Backend README](apps/backend/README.md)

---

### 📱 Mobile App

**Location:** `apps/mobile`

A cross-platform mobile application built with **Flutter**, providing a seamless shopping experience on iOS and Android.

**Features:**
- Browse products by category and brand
- Add items to cart
- Secure checkout
- Order history
- Responsive UI for all screen sizes

[View Mobile README](apps/mobile/README.md)

---

## Tech Stack

| Component | Technologies |
|-----------|--------------|
| **Backend** | ASP.NET MVC, C#, Entity Framework, LINQ |
| **Frontend (Web)** | Razor Views, Bootstrap 5, Tailwind CSS |
| **Mobile** | Flutter, Dart |
| **Database** | SQL Server (Code-First Migrations) |
| **Authentication** | ASP.NET Identity, Email Verification |
| **Payment** | Stripe |

---

## Getting Started

### Prerequisites

- **.NET SDK** (for backend)
- **Flutter SDK** (for mobile app)
- **SQL Server** or compatible database
- **Visual Studio** or **VS Code**

### Backend Setup

1. Navigate to the backend directory:
   ```bash
   cd apps/backend
   ```

2. Open `LushThreads.sln` in Visual Studio

3. Update the connection string in `appsettings.json`

4. Apply migrations:
   ```bash
   Update-Database
   ```

5. Run the project

For detailed instructions, see [Backend README](apps/backend/README.md)

### Mobile App Setup

1. Navigate to the mobile directory:
   ```bash
   cd apps/mobile
   ```

2. Install dependencies:
   ```bash
   flutter pub get
   ```

3. Run the app:
   ```bash
   flutter run
   ```

For detailed instructions, see [Mobile README](apps/mobile/README.md)

---

## Architecture

The project follows **Clean Architecture** principles with separation of concerns:

- **Domain Layer**: Core business entities and rules
- **Application Layer**: Use cases and business logic
- **Infrastructure Layer**: Data access, external services
- **API/Presentation Layer**: Controllers, views, and UI

---

## License

This project is licensed under the MIT License.

---

## Contact

Feel free to reach out or connect on [LinkedIn](https://www.linkedin.com/in/mohamed-gasser-4b3ab1333)

If you like the project, consider giving it a ⭐!

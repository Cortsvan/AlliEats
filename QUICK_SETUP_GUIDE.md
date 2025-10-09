# AllieEats Menu Management - Quick Setup Guide

## 🗄️ Database Connection

### Connection String
Located in `ASI.Basecode.WebApp/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Addr=(localdb)\\MSSQLLocalDB; database=AlliEatsDB; Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### Run Migrations
```bash
add-migration context
update-database
```

## 👤 Default Admin Account

The system automatically creates an admin account on startup:
- **Email**: `allieatsadmin@gmail.com`
- **Password**: `@Admin123`
- **Role**: Admin (full menu management access)

## 🗂️ Layer Structure

```
ASI.Basecode.Data/          # Data Access Layer
├── Models/                 # Entity models (User, MenuItem)
├── Interfaces/             # Repository contracts
├── Repositories/           # Data access implementations
└── AsiBasecodeDbContext.cs # Database context

ASI.Basecode.Services/      # Business Logic Layer
├── Interfaces/             # Service contracts
├── Services/               # Business logic implementations
└── ServiceModels/          # DTOs and ViewModels

ASI.Basecode.WebApp/        # Presentation Layer
├── Controllers/            # MVC Controllers
├── Views/AdminMenu/        # Menu management views
└── Models/                 # Web-specific models
```

### Menu Management Components

#### Controllers
- `AdminMenuController.cs` - Handles all menu CRUD operations
- `AccountController.cs` - Authentication with role-based redirection

#### Views
- `ViewItems.cshtml` - Menu items dashboard with table view
- `AddItem.cshtml` - Form to create new menu items
- `EditItem.cshtml` - Form to update existing items

#### Services
- `IMenuService` / `MenuService` - Business logic for menu operations
- `IMenuRepository` / `MenuRepository` - Data access for menu items

#### Models
- `MenuItem` - Entity model for database
- `MenuItemViewModel` - View model with validation


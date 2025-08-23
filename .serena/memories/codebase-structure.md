# Codebase Structure

## Project Root Structure
```
dotnet-demo-d2/
├── .claude/              # Claude Code configuration
├── .serena/              # Serena MCP server data
├── docs/                 # Comprehensive documentation
├── issues/               # Issue tracking and management
├── EmployeeManagement/   # Main .NET project
├── CLAUDE.md            # Project instructions for Claude
├── .gitignore           # Git ignore rules
└── .mcp.json           # MCP server configuration
```

## Main Project Structure (EmployeeManagement/)
```
EmployeeManagement/
├── Components/                    # Blazor UI Components
│   ├── Layout/                   # Layout components
│   ├── Pages/                    # Page components
│   ├── Shared/                   # Shared UI components
│   ├── Dialogs/                  # Modal dialogs
│   ├── App.razor                 # Root application component
│   ├── Routes.razor              # Routing configuration
│   ├── _Imports.razor           # Global imports
│   └── AuthRequiredComponentBase.cs # Base class for authenticated components
├── Application/                  # Business Logic Layer
│   ├── Interfaces/              # Service interfaces
│   └── Services/                # Service implementations
├── Domain/                       # Domain Models
│   ├── Interfaces/              # Domain interfaces
│   └── Models/                  # Domain entities
├── Infrastructure/               # Data Access Layer
│   ├── DataStores/              # Data storage implementations
│   └── Repositories/            # Data access repositories
├── ViewModels/                   # State Management
├── Constants/                    # Application constants
├── Models/                       # Data Transfer Objects
├── Properties/                   # Project properties
├── Tests/                        # Test projects (future)
├── wwwroot/                      # Static assets
├── Program.cs                    # Application startup
├── EmployeeManagement.csproj     # Project file
├── appsettings.json             # Configuration
└── appsettings.Development.json  # Development configuration
```

## Architecture Layers

### Presentation Layer (Components/)
- **Pages/**: Main application pages (Dashboard, Employee List, etc.)
- **Layout/**: Layout components (MainLayout, Navigation)  
- **Shared/**: Reusable UI components
- **Dialogs/**: Modal dialog components
- **Blazor Components**: .razor files with C# code-behind

### Application Layer (Application/)
- **Services/**: Business logic services
  - EmployeeService, DepartmentService
  - ValidationService, UIService
  - AuthenticationService
- **Interfaces/**: Service contracts
- **Clean Architecture**: Business rules and use cases

### Domain Layer (Domain/)
- **Models/**: Core business entities
  - Employee, DepartmentMaster
  - User, EmployeeNumber
- **Interfaces/**: Domain contracts
- **Business Rules**: Domain-specific logic

### Infrastructure Layer (Infrastructure/)
- **Repositories/**: Data access implementations
- **DataStores/**: In-memory data storage
- **External Concerns**: Database, file system, external APIs

### Cross-cutting Concerns
- **ViewModels/**: State management for components
- **Constants/**: Application-wide constants
- **Models/**: DTOs and view models

## Key Files and Their Purposes

### Configuration Files
- **Program.cs**: Dependency injection, middleware setup
- **appsettings.json**: Application configuration
- **EmployeeManagement.csproj**: NuGet packages, target framework

### Core Components
- **App.razor**: Root application component
- **Routes.razor**: Routing configuration
- **_Imports.razor**: Global using statements
- **AuthRequiredComponentBase.cs**: Authentication base class

### Documentation Structure
- **docs/specifications/**: Project specifications and features
- **docs/development/**: Development processes and coding standards
- **docs/architecture/**: System design and technical decisions
- **docs/workflows/**: Development, testing, and deployment procedures

## Data Flow Architecture
```
UI Components → ViewModels → Application Services → Domain Services → Repositories → DataStore
```

## Dependency Injection Structure
- **Singleton**: ConcurrentInMemoryDataStore (data persistence)
- **Scoped**: Services, Repositories, ViewModels (per request)
- **Transient**: Short-lived objects (if needed)

This structure follows Domain-Driven Design principles with clear separation of concerns and layered architecture.
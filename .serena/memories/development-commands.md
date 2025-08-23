# Development Commands

## Core Development Commands

### Project Navigation
```bash
# Navigate to main project directory
cd EmployeeManagement
```

### Package Management
```bash
# Restore NuGet packages
dotnet restore

# Add new package
dotnet add package [PackageName]

# Remove package  
dotnet remove package [PackageName]
```

### Build Operations
```bash
# Build project
dotnet build

# Build in Release mode
dotnet build --configuration Release

# Clean build artifacts
dotnet clean

# Rebuild (clean + build)
dotnet clean && dotnet build
```

### Application Execution
```bash
# Run application (recommended for development)
dotnet watch run

# Standard run (production testing)
dotnet run

# Run with specific environment
dotnet run --environment Development
```

### Testing Commands
```bash
# Run unit tests (when implemented)
dotnet test

# Run tests with verbose output
dotnet test --verbosity normal

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Application URLs
- **HTTPS Development**: https://localhost:7049
- **HTTP Development**: http://localhost:5000 (if configured)

## Essential Development Workflow
1. **Start Development**: `cd EmployeeManagement && dotnet watch run`
2. **Build Check**: `dotnet build` (run before commits)
3. **Clean Build**: `dotnet clean && dotnet build` (when issues occur)
4. **Package Restore**: `dotnet restore` (after pulling changes)

## Troubleshooting Commands
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Check port usage (Linux)
netstat -tulpn | grep :7049

# Kill processes using port
sudo fuser -k 7049/tcp
```

## Best Practices
- Use `dotnet watch run` for development (hot reload enabled)
- Run `dotnet build` regularly to catch errors early  
- Use `dotnet clean` when experiencing build issues
- Monitor browser console for runtime errors
- Clear browser cache when testing authentication changes
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Infrastructure.DataStores;

namespace EmployeeManagement.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ConcurrentInMemoryDataStore _dataStore;

    public EmployeeRepository(ConcurrentInMemoryDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<IEnumerable<Employee>> GetAllAsync()
    {
        return Task.FromResult(_dataStore.GetAllEmployees());
    }

    public Task<Employee?> GetByIdAsync(string employeeNumber)
    {
        return Task.FromResult(_dataStore.GetEmployee(employeeNumber));
    }

    public Task<Employee?> GetByEmployeeNumberAsync(string employeeNumber)
    {
        return Task.FromResult(_dataStore.GetEmployee(employeeNumber));
    }

    public Task<bool> AddAsync(Employee employee)
    {
        return Task.FromResult(_dataStore.AddEmployee(employee));
    }

    public Task<bool> UpdateAsync(Employee employee)
    {
        return Task.FromResult(_dataStore.UpdateEmployee(employee));
    }

    public Task<bool> DeleteAsync(string employeeNumber)
    {
        return Task.FromResult(_dataStore.DeleteEmployee(employeeNumber));
    }

    public Task<int> GetCountAsync()
    {
        return Task.FromResult(_dataStore.GetEmployeeCount());
    }

    public Task<IEnumerable<Employee>> SearchAsync(
        string? employeeNumber = null,
        string? name = null,
        Department? department = null,
        Position? position = null)
    {
        var employees = _dataStore.GetAllEmployees();
        
        var filtered = employees.Where(emp =>
        {
            // Filter by employee number (partial match)
            if (!string.IsNullOrEmpty(employeeNumber) && 
                !emp.EmployeeNumber.Contains(employeeNumber, StringComparison.OrdinalIgnoreCase))
                return false;
                
            // Filter by name (partial match)
            if (!string.IsNullOrEmpty(name) && 
                !emp.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                return false;
                
            // Filter by department
            if (department.HasValue && emp.CurrentDepartment != department.Value)
                return false;
                
            // Filter by position
            if (position.HasValue && emp.CurrentPosition != position.Value)
                return false;
                
            return true;
        });
        
        return Task.FromResult(filtered.OrderBy(e => e.EmployeeNumber).AsEnumerable());
    }

    public Task<IEnumerable<Employee>> GetByEmployeeNumberPartialAsync(string partialNumber)
    {
        var employees = _dataStore.GetAllEmployees()
            .Where(emp => emp.EmployeeNumber.Contains(partialNumber, StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.EmployeeNumber);
            
        return Task.FromResult(employees.AsEnumerable());
    }
}
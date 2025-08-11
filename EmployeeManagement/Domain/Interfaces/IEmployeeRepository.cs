using EmployeeManagement.Domain.Models;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Domain.Interfaces;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(string employeeNumber);
    Task<Employee?> GetByEmployeeNumberAsync(string employeeNumber);
    Task<bool> AddAsync(Employee employee);
    Task<bool> UpdateAsync(Employee employee);
    Task<bool> DeleteAsync(string employeeNumber);
    Task<int> GetCountAsync();
    
    // Search methods
    Task<IEnumerable<Employee>> SearchAsync(
        string? employeeNumber = null,
        string? name = null,
        Department? department = null,
        Position? position = null);
    
    Task<IEnumerable<Employee>> GetByEmployeeNumberPartialAsync(string partialNumber);
}
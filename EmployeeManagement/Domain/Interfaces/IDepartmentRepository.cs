using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Domain.Interfaces;

public interface IDepartmentRepository
{
    Task<IEnumerable<DepartmentMaster>> GetAllAsync();
    Task<DepartmentMaster?> GetByIdAsync(string departmentCode);
    Task<bool> AddAsync(DepartmentMaster department);
    Task<bool> UpdateAsync(DepartmentMaster department);
    Task<bool> DeleteAsync(string departmentCode);
    Task<int> GetCountAsync();
    Task<int> GetActiveCountAsync();
    Task<int> GetWithManagerCountAsync();
    Task<DateTime> GetLastUpdateDateAsync();
}
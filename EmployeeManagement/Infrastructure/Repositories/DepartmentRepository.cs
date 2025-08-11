using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Infrastructure.DataStores;

namespace EmployeeManagement.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ConcurrentInMemoryDataStore _dataStore;

    public DepartmentRepository(ConcurrentInMemoryDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<IEnumerable<DepartmentMaster>> GetAllAsync()
    {
        return Task.FromResult(_dataStore.GetAllDepartments());
    }

    public Task<DepartmentMaster?> GetByIdAsync(string departmentCode)
    {
        return Task.FromResult(_dataStore.GetDepartment(departmentCode));
    }

    public Task<bool> AddAsync(DepartmentMaster department)
    {
        return Task.FromResult(_dataStore.AddDepartment(department));
    }

    public Task<bool> UpdateAsync(DepartmentMaster department)
    {
        return Task.FromResult(_dataStore.UpdateDepartment(department));
    }

    public Task<bool> DeleteAsync(string departmentCode)
    {
        return Task.FromResult(_dataStore.DeleteDepartment(departmentCode));
    }

    public Task<int> GetCountAsync()
    {
        return Task.FromResult(_dataStore.GetDepartmentCount());
    }

    public Task<int> GetActiveCountAsync()
    {
        return Task.FromResult(_dataStore.GetActiveDepartmentCount());
    }

    public Task<int> GetWithManagerCountAsync()
    {
        return Task.FromResult(_dataStore.GetDepartmentsWithManagerCount());
    }

    public Task<DateTime> GetLastUpdateDateAsync()
    {
        return Task.FromResult(_dataStore.GetLastDepartmentUpdateDate());
    }
}
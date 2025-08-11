using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Infrastructure.DataStores;

namespace EmployeeManagement.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ConcurrentInMemoryDataStore _dataStore;

    public UserRepository(ConcurrentInMemoryDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<User?> GetByIdAsync(string userId)
    {
        return Task.FromResult(_dataStore.GetUser(userId));
    }

    public Task<bool> ValidateUserAsync(string userId, string password)
    {
        return Task.FromResult(_dataStore.ValidateUser(userId, password));
    }

    public Task UpdateLastLoginAsync(string userId)
    {
        _dataStore.UpdateLastLogin(userId);
        return Task.CompletedTask;
    }
}
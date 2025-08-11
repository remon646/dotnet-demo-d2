using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string userId);
    Task<bool> ValidateUserAsync(string userId, string password);
    Task UpdateLastLoginAsync(string userId);
}
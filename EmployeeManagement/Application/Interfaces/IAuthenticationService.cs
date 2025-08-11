using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Application.Interfaces;

public interface IAuthenticationService
{
    Task<bool> LoginAsync(string userId, string password);
    Task LogoutAsync();
    Task<User?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<bool> IsAdminAsync();
}
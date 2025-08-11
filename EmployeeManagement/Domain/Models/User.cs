using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Domain.Models;

public class User
{
    [Required]
    [StringLength(20)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string DisplayName { get; set; } = string.Empty;

    public bool IsAdmin { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime LastLoginAt { get; set; }
}
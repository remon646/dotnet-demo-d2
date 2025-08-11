using EmployeeManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Domain.Models;

public class DepartmentMaster
{
    [Required]
    [StringLength(10)]
    public string DepartmentCode { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string DepartmentName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? ManagerName { get; set; }

    [StringLength(10)]
    public string? ManagerEmployeeNumber { get; set; }

    [Required]
    public Department DepartmentType { get; set; }

    public DateTime EstablishedDate { get; set; } = DateTime.Now;

    [StringLength(10)]
    public string? Extension { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public string DepartmentTypeDisplayName => DepartmentType.ToDisplayName();
}
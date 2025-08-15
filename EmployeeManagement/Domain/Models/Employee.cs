using EmployeeManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Domain.Models;

public class Employee
{
    [Required]
    [StringLength(10, MinimumLength = 10)]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime JoinDate { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [StringLength(15)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 退職日（未退職の場合はnull）
    /// </summary>
    public DateTime? RetirementDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 現在の部門履歴を取得
    /// </summary>
    public DepartmentHistory? CurrentDepartmentHistory { get; set; }

    /// <summary>
    /// 全ての部門履歴
    /// </summary>
    public List<DepartmentHistory> DepartmentHistories { get; set; } = new();

    /// <summary>
    /// 現在の部門
    /// </summary>
    public Department? CurrentDepartment => CurrentDepartmentHistory?.Department;

    /// <summary>
    /// 現在の役職
    /// </summary>
    public Position? CurrentPosition => CurrentDepartmentHistory?.Position;

    /// <summary>
    /// 現在の部門表示名
    /// </summary>
    public string CurrentDepartmentDisplayName => CurrentDepartmentHistory?.DepartmentDisplayName ?? "未配属";

    /// <summary>
    /// 現在の役職表示名
    /// </summary>
    public string CurrentPositionDisplayName => CurrentDepartmentHistory?.PositionDisplayName ?? "未設定";

    /// <summary>
    /// 退職しているかどうか
    /// </summary>
    public bool IsRetired => RetirementDate.HasValue && RetirementDate.Value <= DateTime.Today;

    /// <summary>
    /// 退職状況の表示テキスト
    /// </summary>
    public string RetirementStatusDisplayText => IsRetired ? "退職済み" : "在職中";
}
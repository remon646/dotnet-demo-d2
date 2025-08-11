using EmployeeManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Domain.Models;

/// <summary>
/// 部門履歴エンティティ - 社員の部門異動履歴を管理
/// </summary>
public class DepartmentHistory
{
    /// <summary>
    /// 履歴ID
    /// </summary>
    [Required]
    public string HistoryId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 社員番号
    /// </summary>
    [Required]
    [StringLength(10, MinimumLength = 10)]
    public string EmployeeNumber { get; set; } = string.Empty;

    /// <summary>
    /// 部門
    /// </summary>
    [Required]
    public Department Department { get; set; }

    /// <summary>
    /// 役職
    /// </summary>
    [Required]
    public Position Position { get; set; }

    /// <summary>
    /// 配属開始日
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// 配属終了日 (null: 現在所属中)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 異動事由
    /// </summary>
    [StringLength(100)]
    public string? TransferReason { get; set; }

    /// <summary>
    /// 備考
    /// </summary>
    [StringLength(200)]
    public string? Remarks { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 現在所属中かどうか
    /// </summary>
    public bool IsCurrent => EndDate == null;

    /// <summary>
    /// 部門表示名
    /// </summary>
    public string DepartmentDisplayName => Department.ToDisplayName();

    /// <summary>
    /// 役職表示名
    /// </summary>
    public string PositionDisplayName => Position.ToDisplayName();

    /// <summary>
    /// 期間表示
    /// </summary>
    public string PeriodDisplay => EndDate.HasValue 
        ? $"{StartDate:yyyy/MM/dd} - {EndDate:yyyy/MM/dd}"
        : $"{StartDate:yyyy/MM/dd} - 現在";
}
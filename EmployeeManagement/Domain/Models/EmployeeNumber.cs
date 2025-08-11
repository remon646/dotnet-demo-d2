using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Domain.Models;

/// <summary>
/// 社員番号エンティティ - 固定番号方式
/// </summary>
public class EmployeeNumber
{
    /// <summary>
    /// 社員番号 (例: EMP2024001) - 生涯不変
    /// </summary>
    [Required]
    [StringLength(10, MinimumLength = 10)]
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// 発行年度
    /// </summary>
    [Required]
    public int IssueYear { get; set; }

    /// <summary>
    /// 年度内連番
    /// </summary>
    [Required]
    public int SequenceNumber { get; set; }

    /// <summary>
    /// 発行日時
    /// </summary>
    [Required]
    public DateTime IssuedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 使用状況 (true: 使用中, false: 廃番)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 状態 (Active, Reserved, Deactivated)
    /// </summary>
    public EmployeeNumberStatus Status { get; set; } = EmployeeNumberStatus.Reserved;

    /// <summary>
    /// 予約ID (予約時のみ使用)
    /// </summary>
    public string? ReservationId { get; set; }

    /// <summary>
    /// 予約期限 (予約時のみ使用)
    /// </summary>
    public DateTime? ReservationExpiresAt { get; set; }

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
}

/// <summary>
/// 社員番号の状態
/// </summary>
public enum EmployeeNumberStatus
{
    /// <summary>
    /// 予約中
    /// </summary>
    Reserved = 0,
    
    /// <summary>
    /// 使用中
    /// </summary>
    Active = 1,
    
    /// <summary>
    /// 廃番
    /// </summary>
    Deactivated = 2
}
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Domain.Models;

/// <summary>
/// ロール（役割）エンティティ
/// ユーザーに割り当てられる権限の集合を表す
/// </summary>
public class Role
{
    /// <summary>
    /// ロールID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ロール名
    /// システム内で一意である必要がある
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ロールの説明
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// ロールの優先度
    /// 数値が高いほど上位のロール
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// システムロールフラグ
    /// trueの場合、システムで事前定義されたロールで削除不可
    /// </summary>
    public bool IsSystemRole { get; set; }

    /// <summary>
    /// アクティブフラグ
    /// falseの場合、無効化されたロール
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 作成者ID
    /// </summary>
    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// 最終更新日時
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 最終更新者ID
    /// </summary>
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// このロールに割り当てられた権限一覧
    /// </summary>
    public List<RolePermission> Permissions { get; set; } = new();

    /// <summary>
    /// このロールが割り当てられたユーザー一覧
    /// </summary>
    public List<UserRole> UserRoles { get; set; } = new();

    /// <summary>
    /// ロールの表示名を取得
    /// </summary>
    public string DisplayName => $"{Name} ({Description})";

    /// <summary>
    /// システムで事前定義されているロールかどうかを判定
    /// </summary>
    /// <returns>システムロールの場合true</returns>
    public bool IsSystemDefined() => IsSystemRole;

    /// <summary>
    /// ロールが削除可能かどうかを判定
    /// </summary>
    /// <returns>削除可能な場合true</returns>
    public bool CanBeDeleted() => !IsSystemRole && UserRoles.Count == 0;

    /// <summary>
    /// ロールが編集可能かどうかを判定
    /// </summary>
    /// <returns>編集可能な場合true</returns>
    public bool CanBeEdited() => !IsSystemRole;

    /// <summary>
    /// 指定した権限を持っているかチェック
    /// </summary>
    /// <param name="permissionName">権限名</param>
    /// <returns>権限を持っている場合true</returns>
    public bool HasPermission(string permissionName)
    {
        return Permissions.Any(rp => 
            rp.IsGranted && 
            rp.Permission?.Name == permissionName);
    }

    /// <summary>
    /// アクティブなユーザー数を取得
    /// </summary>
    /// <returns>このロールが割り当てられたアクティブなユーザー数</returns>
    public int GetActiveUserCount()
    {
        return UserRoles.Count(ur => ur.IsActive);
    }

    /// <summary>
    /// ロールの権限数を取得
    /// </summary>
    /// <returns>付与された権限の数</returns>
    public int GetPermissionCount()
    {
        return Permissions.Count(rp => rp.IsGranted);
    }

    /// <summary>
    /// ロールを更新
    /// </summary>
    /// <param name="name">ロール名</param>
    /// <param name="description">説明</param>
    /// <param name="priority">優先度</param>
    /// <param name="isActive">アクティブフラグ</param>
    /// <param name="updatedBy">更新者</param>
    public void Update(string name, string description, int priority, bool isActive, string updatedBy)
    {
        if (IsSystemRole && name != Name)
        {
            throw new InvalidOperationException("システムロールの名前は変更できません。");
        }

        Name = name;
        Description = description;
        Priority = priority;
        IsActive = isActive;
        UpdatedAt = DateTime.Now;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// ロールを無効化
    /// </summary>
    /// <param name="updatedBy">更新者</param>
    public void Deactivate(string updatedBy)
    {
        if (IsSystemRole)
        {
            throw new InvalidOperationException("システムロールは無効化できません。");
        }

        IsActive = false;
        UpdatedAt = DateTime.Now;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// ロールを有効化
    /// </summary>
    /// <param name="updatedBy">更新者</param>
    public void Activate(string updatedBy)
    {
        IsActive = true;
        UpdatedAt = DateTime.Now;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// 文字列表現を取得
    /// </summary>
    /// <returns>ロール名</returns>
    public override string ToString() => Name;
}

/// <summary>
/// システムで事前定義されているロール
/// </summary>
public enum SystemRole
{
    /// <summary>
    /// システム管理者 - 全機能アクセス可能
    /// </summary>
    SystemAdmin = 1,

    /// <summary>
    /// 人事管理者 - 社員・部署管理可能
    /// </summary>
    HRManager = 2,

    /// <summary>
    /// 部門管理者 - 所属部署の社員管理可能
    /// </summary>
    DepartmentManager = 3,

    /// <summary>
    /// 一般ユーザー - 閲覧権限のみ
    /// </summary>
    User = 4,

    /// <summary>
    /// ゲストユーザー - 限定的な閲覧権限
    /// </summary>
    Guest = 5
}

/// <summary>
/// システムロールの拡張メソッド
/// </summary>
public static class SystemRoleExtensions
{
    /// <summary>
    /// システムロールの名前を取得
    /// </summary>
    /// <param name="role">システムロール</param>
    /// <returns>ロール名</returns>
    public static string GetName(this SystemRole role) => role switch
    {
        SystemRole.SystemAdmin => "SystemAdmin",
        SystemRole.HRManager => "HRManager",
        SystemRole.DepartmentManager => "DepartmentManager",
        SystemRole.User => "User",
        SystemRole.Guest => "Guest",
        _ => role.ToString()
    };

    /// <summary>
    /// システムロールの説明を取得
    /// </summary>
    /// <param name="role">システムロール</param>
    /// <returns>ロールの説明</returns>
    public static string GetDescription(this SystemRole role) => role switch
    {
        SystemRole.SystemAdmin => "システム管理者 - 全機能へのアクセス権限",
        SystemRole.HRManager => "人事管理者 - 社員・部署管理権限",
        SystemRole.DepartmentManager => "部門管理者 - 所属部署の社員管理権限",
        SystemRole.User => "一般ユーザー - 基本的な閲覧権限",
        SystemRole.Guest => "ゲストユーザー - 限定的な閲覧権限",
        _ => role.ToString()
    };

    /// <summary>
    /// システムロールの優先度を取得
    /// </summary>
    /// <param name="role">システムロール</param>
    /// <returns>優先度（数値が高いほど上位）</returns>
    public static int GetPriority(this SystemRole role) => role switch
    {
        SystemRole.SystemAdmin => 100,
        SystemRole.HRManager => 80,
        SystemRole.DepartmentManager => 60,
        SystemRole.User => 40,
        SystemRole.Guest => 20,
        _ => 0
    };
}
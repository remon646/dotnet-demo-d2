using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Domain.Models;

/// <summary>
/// ユーザーロール関連エンティティ
/// ユーザーに割り当てられたロールを表す
/// </summary>
public class UserRole
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ユーザーID
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// ロールID
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// ロール割り当て日時
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// ロール割り当て者ID
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string AssignedBy { get; set; } = string.Empty;

    /// <summary>
    /// ロールの有効期限（オプション）
    /// nullの場合は無期限
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// アクティブフラグ
    /// falseの場合、無効化されたロール割り当て
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// プライマリロールフラグ
    /// trueの場合、ユーザーの主要なロール
    /// </summary>
    public bool IsPrimary { get; set; } = false;

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
    /// ロール割り当ての理由・コメント
    /// </summary>
    [MaxLength(500)]
    public string? Comment { get; set; }

    /// <summary>
    /// 関連するロール
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// 関連するユーザー
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// ロール割り当てが現在有効かどうかを判定
    /// </summary>
    /// <returns>有効な場合true</returns>
    public bool IsCurrentlyValid()
    {
        return IsActive && 
               Role?.IsActive == true &&
               (ExpiresAt == null || ExpiresAt > DateTime.Now);
    }

    /// <summary>
    /// ロール割り当てが期限切れかどうかを判定
    /// </summary>
    /// <returns>期限切れの場合true</returns>
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt <= DateTime.Now;
    }

    /// <summary>
    /// 一時的なロール割り当てかどうかを判定
    /// </summary>
    /// <returns>一時的な場合true</returns>
    public bool IsTemporary()
    {
        return ExpiresAt.HasValue;
    }

    /// <summary>
    /// システムロールかどうかを判定
    /// </summary>
    /// <returns>システムロールの場合true</returns>
    public bool IsSystemRole()
    {
        return Role?.IsSystemRole == true;
    }

    /// <summary>
    /// ロール割り当てを無効化
    /// </summary>
    /// <param name="updatedBy">更新者</param>
    /// <param name="comment">無効化理由</param>
    public void Deactivate(string updatedBy, string? comment = null)
    {
        IsActive = false;
        UpdatedAt = DateTime.Now;
        UpdatedBy = updatedBy;
        Comment = comment ?? "ロール割り当てが無効化されました";
    }

    /// <summary>
    /// ロール割り当てを有効化
    /// </summary>
    /// <param name="updatedBy">更新者</param>
    /// <param name="comment">有効化理由</param>
    public void Activate(string updatedBy, string? comment = null)
    {
        IsActive = true;
        UpdatedAt = DateTime.Now;
        UpdatedBy = updatedBy;
        Comment = comment ?? "ロール割り当てが有効化されました";
    }

    /// <summary>
    /// プライマリロールに設定
    /// </summary>
    /// <param name="updatedBy">更新者</param>
    public void SetAsPrimary(string updatedBy)
    {
        IsPrimary = true;
        UpdatedAt = DateTime.Now;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// プライマリロール設定を解除
    /// </summary>
    /// <param name="updatedBy">更新者</param>
    public void UnsetAsPrimary(string updatedBy)
    {
        IsPrimary = false;
        UpdatedAt = DateTime.Now;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// 有効期限を延長
    /// </summary>
    /// <param name="newExpiresAt">新しい有効期限</param>
    /// <param name="updatedBy">更新者</param>
    public void ExtendExpiry(DateTime newExpiresAt, string updatedBy)
    {
        if (newExpiresAt <= DateTime.Now)
        {
            throw new ArgumentException("有効期限は現在時刻より後である必要があります。");
        }

        ExpiresAt = newExpiresAt;
        UpdatedAt = DateTime.Now;
        UpdatedBy = updatedBy;
        Comment = $"有効期限を {newExpiresAt:yyyy/MM/dd HH:mm} に延長しました";
    }

    /// <summary>
    /// 永続ロールに変更（有効期限を削除）
    /// </summary>
    /// <param name="updatedBy">更新者</param>
    public void MakePermanent(string updatedBy)
    {
        ExpiresAt = null;
        UpdatedAt = DateTime.Now;
        UpdatedBy = updatedBy;
        Comment = "一時的なロールから永続ロールに変更されました";
    }

    /// <summary>
    /// ロール割り当て情報を更新
    /// </summary>
    /// <param name="isActive">アクティブフラグ</param>
    /// <param name="isPrimary">プライマリフラグ</param>
    /// <param name="expiresAt">有効期限</param>
    /// <param name="updatedBy">更新者</param>
    /// <param name="comment">コメント</param>
    public void Update(bool isActive, bool isPrimary, DateTime? expiresAt, 
                      string updatedBy, string? comment = null)
    {
        IsActive = isActive;
        IsPrimary = isPrimary;
        ExpiresAt = expiresAt;
        UpdatedAt = DateTime.Now;
        UpdatedBy = updatedBy;
        Comment = comment ?? "ロール割り当て情報が更新されました";
    }

    /// <summary>
    /// 残り有効期間を取得
    /// </summary>
    /// <returns>残り日数（無期限の場合はnull）</returns>
    public int? GetRemainingDays()
    {
        if (!ExpiresAt.HasValue) return null;
        
        var remaining = ExpiresAt.Value.Date - DateTime.Now.Date;
        return remaining.Days > 0 ? remaining.Days : 0;
    }

    /// <summary>
    /// 有効期限警告が必要かどうかを判定
    /// </summary>
    /// <param name="warningDays">警告日数（デフォルト：7日前）</param>
    /// <returns>警告が必要な場合true</returns>
    public bool NeedsExpiryWarning(int warningDays = 7)
    {
        if (!ExpiresAt.HasValue || !IsActive) return false;
        
        var remainingDays = GetRemainingDays();
        return remainingDays.HasValue && remainingDays <= warningDays;
    }

    /// <summary>
    /// ロール階層レベルを取得
    /// </summary>
    /// <returns>ロールの優先度（階層レベル）</returns>
    public int GetRoleHierarchyLevel()
    {
        return Role?.Priority ?? 0;
    }

    /// <summary>
    /// ロール割り当ての状態文字列を取得
    /// </summary>
    /// <returns>状態の説明</returns>
    public string GetStatusDescription()
    {
        if (!IsActive) return "無効";
        if (IsExpired()) return "期限切れ";
        if (!IsCurrentlyValid()) return "無効（ロール無効）";
        if (NeedsExpiryWarning()) return "期限間近";
        if (IsTemporary()) return "一時的";
        return "有効";
    }

    /// <summary>
    /// ロール割り当ての重要度を取得
    /// </summary>
    /// <returns>重要度レベル（1-5、5が最重要）</returns>
    public int GetImportanceLevel()
    {
        if (!IsCurrentlyValid()) return 1;
        if (IsPrimary) return 5;
        if (Role?.IsSystemRole == true) return 4;
        if (IsTemporary()) return 2;
        return 3;
    }

    /// <summary>
    /// 文字列表現を取得
    /// </summary>
    /// <returns>ロール名と状態</returns>
    public override string ToString()
    {
        var status = GetStatusDescription();
        var primary = IsPrimary ? " (主要)" : "";
        return $"{Role?.Name ?? "Unknown"}{primary} - {status}";
    }
}

/// <summary>
/// ユーザーロール関連の拡張メソッド
/// </summary>
public static class UserRoleExtensions
{
    /// <summary>
    /// 有効なロール割り当てのみを取得
    /// </summary>
    /// <param name="userRoles">ユーザーロール一覧</param>
    /// <returns>有効なロール割り当て</returns>
    public static IEnumerable<UserRole> GetValid(this IEnumerable<UserRole> userRoles)
    {
        return userRoles.Where(ur => ur.IsCurrentlyValid());
    }

    /// <summary>
    /// プライマリロールを取得
    /// </summary>
    /// <param name="userRoles">ユーザーロール一覧</param>
    /// <returns>プライマリロール（存在しない場合はnull）</returns>
    public static UserRole? GetPrimary(this IEnumerable<UserRole> userRoles)
    {
        return userRoles.GetValid()
                       .FirstOrDefault(ur => ur.IsPrimary);
    }

    /// <summary>
    /// 最高権限のロールを取得
    /// </summary>
    /// <param name="userRoles">ユーザーロール一覧</param>
    /// <returns>最高権限のロール</returns>
    public static UserRole? GetHighestPrivilege(this IEnumerable<UserRole> userRoles)
    {
        return userRoles.GetValid()
                       .OrderByDescending(ur => ur.GetRoleHierarchyLevel())
                       .FirstOrDefault();
    }

    /// <summary>
    /// 期限切れ間近のロールを取得
    /// </summary>
    /// <param name="userRoles">ユーザーロール一覧</param>
    /// <param name="warningDays">警告日数</param>
    /// <returns>期限切れ間近のロール</returns>
    public static IEnumerable<UserRole> GetExpiringRoles(this IEnumerable<UserRole> userRoles, 
                                                         int warningDays = 7)
    {
        return userRoles.GetValid()
                       .Where(ur => ur.NeedsExpiryWarning(warningDays));
    }

    /// <summary>
    /// システムロールを取得
    /// </summary>
    /// <param name="userRoles">ユーザーロール一覧</param>
    /// <returns>システムロール一覧</returns>
    public static IEnumerable<UserRole> GetSystemRoles(this IEnumerable<UserRole> userRoles)
    {
        return userRoles.GetValid()
                       .Where(ur => ur.IsSystemRole());
    }

    /// <summary>
    /// カスタムロールを取得
    /// </summary>
    /// <param name="userRoles">ユーザーロール一覧</param>
    /// <returns>カスタムロール一覧</returns>
    public static IEnumerable<UserRole> GetCustomRoles(this IEnumerable<UserRole> userRoles)
    {
        return userRoles.GetValid()
                       .Where(ur => !ur.IsSystemRole());
    }

    /// <summary>
    /// ロール名一覧を取得
    /// </summary>
    /// <param name="userRoles">ユーザーロール一覧</param>
    /// <returns>有効なロール名一覧</returns>
    public static IEnumerable<string> GetRoleNames(this IEnumerable<UserRole> userRoles)
    {
        return userRoles.GetValid()
                       .Select(ur => ur.Role?.Name)
                       .Where(name => !string.IsNullOrEmpty(name))
                       .Cast<string>();
    }

    /// <summary>
    /// 特定のロールを持っているかどうかを判定
    /// </summary>
    /// <param name="userRoles">ユーザーロール一覧</param>
    /// <param name="roleName">ロール名</param>
    /// <returns>ロールを持っている場合true</returns>
    public static bool HasRole(this IEnumerable<UserRole> userRoles, string roleName)
    {
        return userRoles.GetRoleNames()
                       .Any(name => string.Equals(name, roleName, 
                                                  StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// いずれかのロールを持っているかどうかを判定
    /// </summary>
    /// <param name="userRoles">ユーザーロール一覧</param>
    /// <param name="roleNames">ロール名一覧</param>
    /// <returns>いずれかのロールを持っている場合true</returns>
    public static bool HasAnyRole(this IEnumerable<UserRole> userRoles, params string[] roleNames)
    {
        var userRoleNames = userRoles.GetRoleNames().ToList();
        return roleNames.Any(roleName => 
            userRoleNames.Any(userRoleName => 
                string.Equals(userRoleName, roleName, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// すべてのロールを持っているかどうかを判定
    /// </summary>
    /// <param name="userRoles">ユーザーロール一覧</param>
    /// <param name="roleNames">ロール名一覧</param>
    /// <returns>すべてのロールを持っている場合true</returns>
    public static bool HasAllRoles(this IEnumerable<UserRole> userRoles, params string[] roleNames)
    {
        var userRoleNames = userRoles.GetRoleNames().ToList();
        return roleNames.All(roleName => 
            userRoleNames.Any(userRoleName => 
                string.Equals(userRoleName, roleName, StringComparison.OrdinalIgnoreCase)));
    }
}
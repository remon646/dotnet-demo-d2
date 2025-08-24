using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Domain.Models;

/// <summary>
/// ユーザーエンティティ
/// 認証と認可の基本となるユーザー情報を表す
/// </summary>
public class User
{
    /// <summary>
    /// ユーザーID（プライマリキー）
    /// </summary>
    [Required]
    [StringLength(100)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// パスワード
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 表示名
    /// </summary>
    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// メールアドレス
    /// </summary>
    [StringLength(200)]
    public string? Email { get; set; }

    /// <summary>
    /// システム管理者フラグ（後方互換性のため残す）
    /// 新システムではロールベースで管理
    /// </summary>
    [Obsolete("Use role-based authorization instead")]
    public bool IsAdmin { get; set; } = false;

    /// <summary>
    /// アクティブフラグ
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// アカウントロック状態
    /// </summary>
    public bool IsLocked { get; set; } = false;

    /// <summary>
    /// パスワード変更が必要かどうか
    /// </summary>
    public bool RequirePasswordChange { get; set; } = false;

    /// <summary>
    /// ログイン試行失敗回数
    /// </summary>
    public int FailedLoginAttempts { get; set; } = 0;

    /// <summary>
    /// 最後のログイン試行失敗日時
    /// </summary>
    public DateTime? LastFailedLoginAt { get; set; }

    /// <summary>
    /// アカウントロック日時
    /// </summary>
    public DateTime? LockedAt { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 作成者ID
    /// </summary>
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// 最終更新日時
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 最終更新者ID
    /// </summary>
    [StringLength(100)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// 最終ログイン日時
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// このユーザーに割り当てられたロール一覧
    /// </summary>
    public List<UserRole> UserRoles { get; set; } = new();

    /// <summary>
    /// ユーザーが現在アクティブかどうかを判定
    /// </summary>
    /// <returns>アクティブな場合true</returns>
    public bool IsCurrentlyActive()
    {
        return IsActive && !IsLocked;
    }

    /// <summary>
    /// ログインが可能かどうかを判定
    /// </summary>
    /// <returns>ログイン可能な場合true</returns>
    public bool CanLogin()
    {
        return IsCurrentlyActive() && 
               (LastFailedLoginAt == null || 
                DateTime.Now.Subtract(LastFailedLoginAt.Value).TotalMinutes > 15); // 15分後にリセット
    }

    /// <summary>
    /// ログイン成功を記録
    /// </summary>
    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTime.Now;
        FailedLoginAttempts = 0;
        LastFailedLoginAt = null;
        
        if (IsLocked && FailedLoginAttempts == 0)
        {
            IsLocked = false;
            LockedAt = null;
        }
    }

    /// <summary>
    /// ログイン失敗を記録
    /// </summary>
    /// <param name="maxAttempts">最大試行回数（デフォルト：5回）</param>
    public void RecordFailedLogin(int maxAttempts = 5)
    {
        FailedLoginAttempts++;
        LastFailedLoginAt = DateTime.Now;
        
        if (FailedLoginAttempts >= maxAttempts)
        {
            IsLocked = true;
            LockedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// アカウントをロック
    /// </summary>
    /// <param name="lockedBy">ロック実行者</param>
    public void Lock(string lockedBy)
    {
        IsLocked = true;
        LockedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
        UpdatedBy = lockedBy;
    }

    /// <summary>
    /// アカウントのロックを解除
    /// </summary>
    /// <param name="unlockedBy">ロック解除実行者</param>
    public void Unlock(string unlockedBy)
    {
        IsLocked = false;
        LockedAt = null;
        FailedLoginAttempts = 0;
        LastFailedLoginAt = null;
        UpdatedAt = DateTime.Now;
        UpdatedBy = unlockedBy;
    }

    /// <summary>
    /// ユーザーを無効化
    /// </summary>
    /// <param name="deactivatedBy">無効化実行者</param>
    public void Deactivate(string deactivatedBy)
    {
        IsActive = false;
        UpdatedAt = DateTime.Now;
        UpdatedBy = deactivatedBy;
    }

    /// <summary>
    /// ユーザーを有効化
    /// </summary>
    /// <param name="activatedBy">有効化実行者</param>
    public void Activate(string activatedBy)
    {
        IsActive = true;
        UpdatedAt = DateTime.Now;
        UpdatedBy = activatedBy;
    }

    /// <summary>
    /// パスワード変更を要求
    /// </summary>
    /// <param name="requiredBy">要求者</param>
    public void SetRequirePasswordChange(string requiredBy)
    {
        RequirePasswordChange = true;
        UpdatedAt = DateTime.Now;
        UpdatedBy = requiredBy;
    }

    /// <summary>
    /// パスワード変更完了を記録
    /// </summary>
    public void CompletePasswordChange()
    {
        RequirePasswordChange = false;
        UpdatedAt = DateTime.Now;
    }

    /// <summary>
    /// ユーザー情報を更新
    /// </summary>
    /// <param name="displayName">表示名</param>
    /// <param name="email">メールアドレス</param>
    /// <param name="updatedBy">更新者</param>
    public void UpdateInfo(string displayName, string? email, string updatedBy)
    {
        DisplayName = displayName;
        Email = email;
        UpdatedAt = DateTime.Now;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// 有効なロール一覧を取得
    /// </summary>
    /// <returns>有効なユーザーロール</returns>
    public List<UserRole> GetActiveRoles()
    {
        return UserRoles.Where(ur => ur.IsCurrentlyValid()).ToList();
    }

    /// <summary>
    /// プライマリロールを取得
    /// </summary>
    /// <returns>プライマリロール（存在しない場合はnull）</returns>
    public UserRole? GetPrimaryRole()
    {
        return GetActiveRoles().FirstOrDefault(ur => ur.IsPrimary);
    }

    /// <summary>
    /// 最高権限のロールを取得
    /// </summary>
    /// <returns>最高権限のロール</returns>
    public UserRole? GetHighestPrivilegeRole()
    {
        return GetActiveRoles()
               .OrderByDescending(ur => ur.GetRoleHierarchyLevel())
               .FirstOrDefault();
    }

    /// <summary>
    /// 指定したロールを持っているかチェック
    /// </summary>
    /// <param name="roleName">ロール名</param>
    /// <returns>ロールを持っている場合true</returns>
    public bool HasRole(string roleName)
    {
        return GetActiveRoles()
               .Any(ur => string.Equals(ur.Role?.Name, roleName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// システム管理者かどうかを判定（ロールベース）
    /// </summary>
    /// <returns>システム管理者の場合true</returns>
    public bool IsSystemAdmin()
    {
        return HasRole("SystemAdmin") || IsAdmin; // 後方互換性
    }

    /// <summary>
    /// ユーザーの状態を取得
    /// </summary>
    /// <returns>ユーザー状態の説明</returns>
    public string GetStatusDescription()
    {
        if (!IsActive) return "無効";
        if (IsLocked) return "ロック中";
        if (RequirePasswordChange) return "パスワード変更必要";
        if (FailedLoginAttempts > 0) return $"ログイン失敗 {FailedLoginAttempts} 回";
        return "正常";
    }

    /// <summary>
    /// 文字列表現を取得
    /// </summary>
    /// <returns>表示名またはユーザーID</returns>
    public override string ToString()
    {
        return string.IsNullOrEmpty(DisplayName) ? UserId : DisplayName;
    }
}
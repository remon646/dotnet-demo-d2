using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Domain.Models;

/// <summary>
/// 権限エンティティ
/// システム内の特定の操作やリソースへのアクセス権限を表す
/// </summary>
public class Permission
{
    /// <summary>
    /// 権限ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 権限名
    /// システム内で一意である必要がある（例：Employee.View, Department.Create）
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 権限の説明
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 権限が属するモジュール
    /// 機能単位でのグループ化（Employee, Department, Report, System など）
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// 権限の操作種別
    /// </summary>
    public PermissionAction Action { get; set; }

    /// <summary>
    /// 対象リソース
    /// 特定のリソースに対する権限の場合に設定（オプション）
    /// </summary>
    [MaxLength(200)]
    public string? Resource { get; set; }

    /// <summary>
    /// システム権限フラグ
    /// trueの場合、システムで事前定義された権限で削除不可
    /// </summary>
    public bool IsSystemPermission { get; set; }

    /// <summary>
    /// アクティブフラグ
    /// falseの場合、無効化された権限
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
    /// この権限が割り当てられたロール一覧
    /// </summary>
    public List<RolePermission> RolePermissions { get; set; } = new();

    /// <summary>
    /// 権限の表示名を取得
    /// </summary>
    public string DisplayName => $"{Module}.{Action}" + (string.IsNullOrEmpty(Resource) ? "" : $" ({Resource})");

    /// <summary>
    /// 権限のフルネームを取得
    /// </summary>
    public string FullName => string.IsNullOrEmpty(Resource) ? Name : $"{Name}.{Resource}";

    /// <summary>
    /// システムで事前定義されている権限かどうかを判定
    /// </summary>
    /// <returns>システム権限の場合true</returns>
    public bool IsSystemDefined() => IsSystemPermission;

    /// <summary>
    /// 権限が削除可能かどうかを判定
    /// </summary>
    /// <returns>削除可能な場合true</returns>
    public bool CanBeDeleted() => !IsSystemPermission && RolePermissions.Count == 0;

    /// <summary>
    /// 権限が編集可能かどうかを判定
    /// </summary>
    /// <returns>編集可能な場合true</returns>
    public bool CanBeEdited() => !IsSystemPermission;

    /// <summary>
    /// この権限を持つアクティブなロール数を取得
    /// </summary>
    /// <returns>権限を持つアクティブなロールの数</returns>
    public int GetActiveRoleCount()
    {
        return RolePermissions.Count(rp => 
            rp.IsGranted && 
            rp.Role?.IsActive == true);
    }

    /// <summary>
    /// 権限を更新
    /// </summary>
    /// <param name="name">権限名</param>
    /// <param name="description">説明</param>
    /// <param name="module">モジュール</param>
    /// <param name="action">操作種別</param>
    /// <param name="resource">リソース</param>
    /// <param name="isActive">アクティブフラグ</param>
    /// <param name="updatedBy">更新者</param>
    public void Update(string name, string description, string module, 
                      PermissionAction action, string? resource, bool isActive, string updatedBy)
    {
        if (IsSystemPermission && name != Name)
        {
            throw new InvalidOperationException("システム権限の名前は変更できません。");
        }

        Name = name;
        Description = description;
        Module = module;
        Action = action;
        Resource = resource;
        IsActive = isActive;
        UpdatedAt = DateTime.Now;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// 権限を無効化
    /// </summary>
    /// <param name="updatedBy">更新者</param>
    public void Deactivate(string updatedBy)
    {
        if (IsSystemPermission)
        {
            throw new InvalidOperationException("システム権限は無効化できません。");
        }

        IsActive = false;
        UpdatedAt = DateTime.Now;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// 権限を有効化
    /// </summary>
    /// <param name="updatedBy">更新者</param>
    public void Activate(string updatedBy)
    {
        IsActive = true;
        UpdatedAt = DateTime.Now;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// 権限名をパースして情報を抽出
    /// </summary>
    /// <param name="permissionName">権限名（例：Employee.View）</param>
    /// <returns>パース結果</returns>
    public static (string Module, PermissionAction Action, string? Resource) ParsePermissionName(string permissionName)
    {
        var parts = permissionName.Split('.');
        
        if (parts.Length < 2)
        {
            throw new ArgumentException($"Invalid permission name format: {permissionName}");
        }

        var module = parts[0];
        var actionStr = parts[1];
        var resource = parts.Length > 2 ? parts[2] : null;

        if (!Enum.TryParse<PermissionAction>(actionStr, true, out var action))
        {
            throw new ArgumentException($"Invalid permission action: {actionStr}");
        }

        return (module, action, resource);
    }

    /// <summary>
    /// 権限名を生成
    /// </summary>
    /// <param name="module">モジュール名</param>
    /// <param name="action">操作種別</param>
    /// <param name="resource">リソース（オプション）</param>
    /// <returns>権限名</returns>
    public static string GeneratePermissionName(string module, PermissionAction action, string? resource = null)
    {
        var name = $"{module}.{action}";
        return string.IsNullOrEmpty(resource) ? name : $"{name}.{resource}";
    }

    /// <summary>
    /// 文字列表現を取得
    /// </summary>
    /// <returns>権限の表示名</returns>
    public override string ToString() => DisplayName;
}

/// <summary>
/// 権限の操作種別
/// </summary>
public enum PermissionAction
{
    /// <summary>
    /// 閲覧・参照権限
    /// </summary>
    View = 1,

    /// <summary>
    /// 作成権限
    /// </summary>
    Create = 2,

    /// <summary>
    /// 更新・編集権限
    /// </summary>
    Update = 3,

    /// <summary>
    /// 削除権限
    /// </summary>
    Delete = 4,

    /// <summary>
    /// エクスポート権限
    /// </summary>
    Export = 5,

    /// <summary>
    /// インポート権限
    /// </summary>
    Import = 6,

    /// <summary>
    /// 管理権限（包括的な管理操作）
    /// </summary>
    Manage = 7,

    /// <summary>
    /// 実行権限（特定の処理実行）
    /// </summary>
    Execute = 8,

    /// <summary>
    /// 承認権限
    /// </summary>
    Approve = 9,

    /// <summary>
    /// 設定権限
    /// </summary>
    Configure = 10
}

/// <summary>
/// 権限操作種別の拡張メソッド
/// </summary>
public static class PermissionActionExtensions
{
    /// <summary>
    /// 権限操作の説明を取得
    /// </summary>
    /// <param name="action">権限操作</param>
    /// <returns>操作の説明</returns>
    public static string GetDescription(this PermissionAction action) => action switch
    {
        PermissionAction.View => "閲覧・参照権限",
        PermissionAction.Create => "作成権限",
        PermissionAction.Update => "更新・編集権限",
        PermissionAction.Delete => "削除権限",
        PermissionAction.Export => "エクスポート権限",
        PermissionAction.Import => "インポート権限",
        PermissionAction.Manage => "管理権限",
        PermissionAction.Execute => "実行権限",
        PermissionAction.Approve => "承認権限",
        PermissionAction.Configure => "設定権限",
        _ => action.ToString()
    };

    /// <summary>
    /// 権限操作のアイコンを取得
    /// </summary>
    /// <param name="action">権限操作</param>
    /// <returns>MudBlazor アイコン名</returns>
    public static string GetIcon(this PermissionAction action) => action switch
    {
        PermissionAction.View => "visibility",
        PermissionAction.Create => "add",
        PermissionAction.Update => "edit",
        PermissionAction.Delete => "delete",
        PermissionAction.Export => "download",
        PermissionAction.Import => "upload",
        PermissionAction.Manage => "settings",
        PermissionAction.Execute => "play_arrow",
        PermissionAction.Approve => "check_circle",
        PermissionAction.Configure => "tune",
        _ => "help"
    };

    /// <summary>
    /// 権限操作の危険度を取得
    /// </summary>
    /// <param name="action">権限操作</param>
    /// <returns>危険度（1-5、5が最も危険）</returns>
    public static int GetRiskLevel(this PermissionAction action) => action switch
    {
        PermissionAction.View => 1,
        PermissionAction.Export => 2,
        PermissionAction.Create => 3,
        PermissionAction.Update => 3,
        PermissionAction.Import => 4,
        PermissionAction.Execute => 4,
        PermissionAction.Approve => 4,
        PermissionAction.Delete => 5,
        PermissionAction.Manage => 5,
        PermissionAction.Configure => 5,
        _ => 3
    };

    /// <summary>
    /// 上位権限かどうかを判定
    /// </summary>
    /// <param name="action">権限操作</param>
    /// <returns>上位権限の場合true</returns>
    public static bool IsHighLevel(this PermissionAction action)
    {
        return GetRiskLevel(action) >= 4;
    }

    /// <summary>
    /// 読み取り専用権限かどうかを判定
    /// </summary>
    /// <param name="action">権限操作</param>
    /// <returns>読み取り専用の場合true</returns>
    public static bool IsReadOnly(this PermissionAction action)
    {
        return action == PermissionAction.View || action == PermissionAction.Export;
    }
}

/// <summary>
/// ロール権限の関連エンティティ
/// ロールと権限の多対多の関係を表す
/// </summary>
public class RolePermission
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ロールID
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// 権限ID
    /// </summary>
    public int PermissionId { get; set; }

    /// <summary>
    /// 権限が付与されているかどうか
    /// falseの場合は明示的に拒否された権限
    /// </summary>
    public bool IsGranted { get; set; } = true;

    /// <summary>
    /// 権限付与日時
    /// </summary>
    public DateTime GrantedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 権限付与者ID
    /// </summary>
    [MaxLength(100)]
    public string GrantedBy { get; set; } = string.Empty;

    /// <summary>
    /// 権限の有効期限（オプション）
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 権限付与の理由・コメント
    /// </summary>
    [MaxLength(500)]
    public string? Comment { get; set; }

    /// <summary>
    /// 関連するロール
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// 関連する権限
    /// </summary>
    public Permission Permission { get; set; } = null!;

    /// <summary>
    /// 権限が現在有効かどうかを判定
    /// </summary>
    /// <returns>有効な場合true</returns>
    public bool IsCurrentlyValid()
    {
        return IsGranted && 
               (ExpiresAt == null || ExpiresAt > DateTime.Now);
    }

    /// <summary>
    /// 権限が期限切れかどうかを判定
    /// </summary>
    /// <returns>期限切れの場合true</returns>
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt <= DateTime.Now;
    }

    /// <summary>
    /// 権限を取り消し
    /// </summary>
    /// <param name="revokedBy">取り消し者</param>
    /// <param name="comment">取り消し理由</param>
    public void Revoke(string revokedBy, string? comment = null)
    {
        IsGranted = false;
        GrantedBy = revokedBy;
        GrantedAt = DateTime.Now;
        Comment = comment ?? "権限が取り消されました";
    }

    /// <summary>
    /// 権限を付与
    /// </summary>
    /// <param name="grantedBy">付与者</param>
    /// <param name="expiresAt">有効期限</param>
    /// <param name="comment">付与理由</param>
    public void Grant(string grantedBy, DateTime? expiresAt = null, string? comment = null)
    {
        IsGranted = true;
        GrantedBy = grantedBy;
        GrantedAt = DateTime.Now;
        ExpiresAt = expiresAt;
        Comment = comment;
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
        GrantedBy = updatedBy;
        GrantedAt = DateTime.Now;
    }
}
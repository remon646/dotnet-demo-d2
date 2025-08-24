using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Application.Interfaces;

/// <summary>
/// 認可サービスのインターフェース
/// ユーザーの権限チェックとロール管理機能を提供
/// </summary>
public interface IAuthorizationService
{
    #region 権限チェック

    /// <summary>
    /// ユーザーが指定した権限を持っているかチェック
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="permissionName">権限名</param>
    /// <returns>権限を持っている場合true</returns>
    Task<bool> HasPermissionAsync(string userId, string permissionName);

    /// <summary>
    /// ユーザーが指定したモジュール・アクションの権限を持っているかチェック
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="module">モジュール名</param>
    /// <param name="action">操作種別</param>
    /// <param name="resource">リソース名（オプション）</param>
    /// <returns>権限を持っている場合true</returns>
    Task<bool> HasPermissionAsync(string userId, string module, PermissionAction action, string? resource = null);

    /// <summary>
    /// ユーザーがいずれかの権限を持っているかチェック
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="permissionNames">権限名一覧</param>
    /// <returns>いずれかの権限を持っている場合true</returns>
    Task<bool> HasAnyPermissionAsync(string userId, params string[] permissionNames);

    /// <summary>
    /// ユーザーがすべての権限を持っているかチェック
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="permissionNames">権限名一覧</param>
    /// <returns>すべての権限を持っている場合true</returns>
    Task<bool> HasAllPermissionsAsync(string userId, params string[] permissionNames);

    /// <summary>
    /// リソースへのアクセス権限をチェック
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="resource">リソース名</param>
    /// <param name="action">操作種別</param>
    /// <returns>アクセス可能な場合true</returns>
    Task<bool> CanAccessAsync(string userId, string resource, PermissionAction action);

    #endregion

    #region ロールチェック

    /// <summary>
    /// ユーザーが指定したロールを持っているかチェック
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="roleName">ロール名</param>
    /// <returns>ロールを持っている場合true</returns>
    Task<bool> IsInRoleAsync(string userId, string roleName);

    /// <summary>
    /// ユーザーが指定したシステムロールを持っているかチェック
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="systemRole">システムロール</param>
    /// <returns>ロールを持っている場合true</returns>
    Task<bool> IsInRoleAsync(string userId, SystemRole systemRole);

    /// <summary>
    /// ユーザーがいずれかのロールを持っているかチェック
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="roleNames">ロール名一覧</param>
    /// <returns>いずれかのロールを持っている場合true</returns>
    Task<bool> IsInAnyRoleAsync(string userId, params string[] roleNames);

    /// <summary>
    /// ユーザーがすべてのロールを持っているかチェック
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="roleNames">ロール名一覧</param>
    /// <returns>すべてのロールを持っている場合true</returns>
    Task<bool> IsInAllRolesAsync(string userId, params string[] roleNames);

    #endregion

    #region ユーザー情報取得

    /// <summary>
    /// ユーザーの権限一覧を取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <returns>権限一覧</returns>
    Task<List<Permission>> GetUserPermissionsAsync(string userId);

    /// <summary>
    /// ユーザーのロール一覧を取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <returns>ロール一覧</returns>
    Task<List<Role>> GetUserRolesAsync(string userId);

    /// <summary>
    /// ユーザーのプライマリロールを取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <returns>プライマリロール（存在しない場合はnull）</returns>
    Task<Role?> GetUserPrimaryRoleAsync(string userId);

    /// <summary>
    /// ユーザーの最高権限ロールを取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <returns>最高権限ロール（存在しない場合はnull）</returns>
    Task<Role?> GetUserHighestRoleAsync(string userId);

    /// <summary>
    /// ユーザーのロール割り当て詳細を取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <returns>ユーザーロール一覧</returns>
    Task<List<UserRole>> GetUserRoleAssignmentsAsync(string userId);

    #endregion

    #region データフィルタリング

    /// <summary>
    /// 権限に基づいてデータをフィルタリング
    /// </summary>
    /// <typeparam name="T">データ型</typeparam>
    /// <param name="userId">ユーザーID</param>
    /// <param name="data">データ一覧</param>
    /// <param name="permissionName">必要な権限名</param>
    /// <returns>フィルタリングされたデータ</returns>
    Task<List<T>> FilterByPermissionAsync<T>(string userId, List<T> data, string permissionName) 
        where T : class;

    /// <summary>
    /// ユーザーがアクセス可能なデータのIDを取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="resourceType">リソースタイプ</param>
    /// <param name="action">操作種別</param>
    /// <returns>アクセス可能なデータID一覧</returns>
    Task<List<string>> GetAccessibleResourceIdsAsync(string userId, string resourceType, PermissionAction action);

    #endregion

    #region 権限階層・継承

    /// <summary>
    /// 権限が他の権限を包含しているかチェック
    /// </summary>
    /// <param name="parentPermission">親権限</param>
    /// <param name="childPermission">子権限</param>
    /// <returns>包含している場合true</returns>
    Task<bool> IsPermissionIncludedAsync(string parentPermission, string childPermission);

    /// <summary>
    /// ロールが他のロールを包含しているかチェック
    /// </summary>
    /// <param name="parentRole">親ロール</param>
    /// <param name="childRole">子ロール</param>
    /// <returns>包含している場合true</returns>
    Task<bool> IsRoleIncludedAsync(string parentRole, string childRole);

    /// <summary>
    /// ユーザーの有効な権限を階層を考慮して取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <returns>有効な権限一覧（重複排除済み）</returns>
    Task<List<Permission>> GetEffectivePermissionsAsync(string userId);

    #endregion

    #region セキュリティ・監査

    /// <summary>
    /// ユーザーの権限使用履歴を記録
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="permissionName">使用した権限名</param>
    /// <param name="resource">対象リソース</param>
    /// <param name="success">権限チェック結果</param>
    /// <returns>記録に成功した場合true</returns>
    Task<bool> LogPermissionUsageAsync(string userId, string permissionName, string? resource = null, bool success = true);

    /// <summary>
    /// セキュリティコンテキストを取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <returns>セキュリティコンテキスト</returns>
    Task<SecurityContext> GetSecurityContextAsync(string userId);

    /// <summary>
    /// ユーザーの権限変更を検証
    /// </summary>
    /// <param name="userId">対象ユーザーID</param>
    /// <param name="requesterId">要求者ユーザーID</param>
    /// <param name="newRoles">新しいロール一覧</param>
    /// <returns>変更可能な場合true</returns>
    Task<bool> CanChangeUserRolesAsync(string userId, string requesterId, List<string> newRoles);

    #endregion

    #region キャッシュ・パフォーマンス

    /// <summary>
    /// ユーザーの権限キャッシュをクリア
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <returns>クリア完了</returns>
    Task ClearUserPermissionCacheAsync(string userId);

    /// <summary>
    /// 全権限キャッシュをクリア
    /// </summary>
    /// <returns>クリア完了</returns>
    Task ClearAllPermissionCacheAsync();

    /// <summary>
    /// 権限キャッシュを更新
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <returns>更新完了</returns>
    Task RefreshUserPermissionCacheAsync(string userId);

    #endregion
}

/// <summary>
/// セキュリティコンテキスト
/// ユーザーのセキュリティ情報を包含
/// </summary>
public class SecurityContext
{
    /// <summary>
    /// ユーザーID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// ユーザー名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// プライマリロール
    /// </summary>
    public Role? PrimaryRole { get; set; }

    /// <summary>
    /// 全ロール
    /// </summary>
    public List<Role> Roles { get; set; } = new();

    /// <summary>
    /// 全権限
    /// </summary>
    public List<Permission> Permissions { get; set; } = new();

    /// <summary>
    /// 最高権限レベル
    /// </summary>
    public int HighestPrivilegeLevel { get; set; }

    /// <summary>
    /// システム管理者かどうか
    /// </summary>
    public bool IsSystemAdmin { get; set; }

    /// <summary>
    /// セキュリティクリアランス
    /// </summary>
    public SecurityClearance Clearance { get; set; } = SecurityClearance.Basic;

    /// <summary>
    /// 最終権限チェック日時
    /// </summary>
    public DateTime LastPermissionCheck { get; set; } = DateTime.Now;
}

/// <summary>
/// セキュリティクリアランス
/// </summary>
public enum SecurityClearance
{
    /// <summary>
    /// 基本レベル
    /// </summary>
    Basic = 1,

    /// <summary>
    /// 標準レベル
    /// </summary>
    Standard = 2,

    /// <summary>
    /// 上級レベル
    /// </summary>
    Advanced = 3,

    /// <summary>
    /// 管理者レベル
    /// </summary>
    Administrative = 4,

    /// <summary>
    /// システムレベル
    /// </summary>
    System = 5
}
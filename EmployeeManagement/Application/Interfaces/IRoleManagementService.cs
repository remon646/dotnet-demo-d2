using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Application.Interfaces;

/// <summary>
/// ロール管理サービスのインターフェース
/// ロールとユーザーロールの管理機能を提供
/// </summary>
public interface IRoleManagementService
{
    #region ロール管理

    /// <summary>
    /// 全ロールを取得
    /// </summary>
    /// <param name="includeInactive">非アクティブなロールも含めるか</param>
    /// <returns>ロール一覧</returns>
    Task<List<Role>> GetAllRolesAsync(bool includeInactive = false);

    /// <summary>
    /// ロールを取得
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <returns>ロール（見つからない場合はnull）</returns>
    Task<Role?> GetRoleAsync(int roleId);

    /// <summary>
    /// ロール名でロールを取得
    /// </summary>
    /// <param name="roleName">ロール名</param>
    /// <returns>ロール（見つからない場合はnull）</returns>
    Task<Role?> GetRoleByNameAsync(string roleName);

    /// <summary>
    /// ロールを作成
    /// </summary>
    /// <param name="name">ロール名</param>
    /// <param name="description">説明</param>
    /// <param name="priority">優先度</param>
    /// <param name="permissionIds">権限ID一覧</param>
    /// <param name="createdBy">作成者</param>
    /// <returns>作成されたロール</returns>
    Task<Role> CreateRoleAsync(string name, string description, int priority, List<int> permissionIds, string createdBy);

    /// <summary>
    /// ロールを更新
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <param name="name">ロール名</param>
    /// <param name="description">説明</param>
    /// <param name="priority">優先度</param>
    /// <param name="isActive">アクティブフラグ</param>
    /// <param name="permissionIds">権限ID一覧</param>
    /// <param name="updatedBy">更新者</param>
    /// <returns>更新されたロール</returns>
    Task<Role> UpdateRoleAsync(int roleId, string name, string description, int priority, 
                              bool isActive, List<int> permissionIds, string updatedBy);

    /// <summary>
    /// ロールを削除
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <param name="deletedBy">削除者</param>
    /// <returns>削除に成功した場合true</returns>
    Task<bool> DeleteRoleAsync(int roleId, string deletedBy);

    /// <summary>
    /// ロールを有効化
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <param name="updatedBy">更新者</param>
    /// <returns>有効化に成功した場合true</returns>
    Task<bool> ActivateRoleAsync(int roleId, string updatedBy);

    /// <summary>
    /// ロールを無効化
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <param name="updatedBy">更新者</param>
    /// <returns>無効化に成功した場合true</returns>
    Task<bool> DeactivateRoleAsync(int roleId, string updatedBy);

    #endregion

    #region ロール権限管理

    /// <summary>
    /// ロールの権限一覧を取得
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <returns>権限一覧</returns>
    Task<List<Permission>> GetRolePermissionsAsync(int roleId);

    /// <summary>
    /// ロールに権限を追加
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <param name="permissionId">権限ID</param>
    /// <param name="grantedBy">付与者</param>
    /// <param name="comment">コメント</param>
    /// <returns>追加に成功した場合true</returns>
    Task<bool> AddPermissionToRoleAsync(int roleId, int permissionId, string grantedBy, string? comment = null);

    /// <summary>
    /// ロールから権限を削除
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <param name="permissionId">権限ID</param>
    /// <param name="revokedBy">削除者</param>
    /// <param name="comment">コメント</param>
    /// <returns>削除に成功した場合true</returns>
    Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId, string revokedBy, string? comment = null);

    /// <summary>
    /// ロールの権限を一括更新
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <param name="permissionIds">権限ID一覧</param>
    /// <param name="updatedBy">更新者</param>
    /// <returns>更新に成功した場合true</returns>
    Task<bool> UpdateRolePermissionsAsync(int roleId, List<int> permissionIds, string updatedBy);

    #endregion

    #region ユーザーロール管理

    /// <summary>
    /// ユーザーのロール割り当て一覧を取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="includeInactive">非アクティブな割り当ても含めるか</param>
    /// <returns>ユーザーロール一覧</returns>
    Task<List<UserRole>> GetUserRoleAssignmentsAsync(string userId, bool includeInactive = false);

    /// <summary>
    /// ユーザーにロールを割り当て
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="roleId">ロールID</param>
    /// <param name="assignedBy">割り当て者</param>
    /// <param name="isPrimary">プライマリロールか</param>
    /// <param name="expiresAt">有効期限</param>
    /// <param name="comment">コメント</param>
    /// <returns>ユーザーロール</returns>
    Task<UserRole> AssignRoleToUserAsync(string userId, int roleId, string assignedBy, 
                                        bool isPrimary = false, DateTime? expiresAt = null, string? comment = null);

    /// <summary>
    /// ユーザーからロールを削除
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="roleId">ロールID</param>
    /// <param name="removedBy">削除者</param>
    /// <param name="comment">コメント</param>
    /// <returns>削除に成功した場合true</returns>
    Task<bool> RemoveRoleFromUserAsync(string userId, int roleId, string removedBy, string? comment = null);

    /// <summary>
    /// ユーザーのロール割り当てを更新
    /// </summary>
    /// <param name="userRoleId">ユーザーロールID</param>
    /// <param name="isActive">アクティブフラグ</param>
    /// <param name="isPrimary">プライマリフラグ</param>
    /// <param name="expiresAt">有効期限</param>
    /// <param name="updatedBy">更新者</param>
    /// <param name="comment">コメント</param>
    /// <returns>更新されたユーザーロール</returns>
    Task<UserRole> UpdateUserRoleAssignmentAsync(int userRoleId, bool isActive, bool isPrimary, 
                                                DateTime? expiresAt, string updatedBy, string? comment = null);

    /// <summary>
    /// ユーザーのプライマリロールを設定
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="roleId">ロールID</param>
    /// <param name="updatedBy">更新者</param>
    /// <returns>設定に成功した場合true</returns>
    Task<bool> SetUserPrimaryRoleAsync(string userId, int roleId, string updatedBy);

    /// <summary>
    /// ユーザーのロールを一括更新
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="roleAssignments">ロール割り当て設定一覧</param>
    /// <param name="updatedBy">更新者</param>
    /// <returns>更新に成功した場合true</returns>
    Task<bool> UpdateUserRolesAsync(string userId, List<UserRoleAssignment> roleAssignments, string updatedBy);

    #endregion

    #region 統計・分析

    /// <summary>
    /// ロール統計を取得
    /// </summary>
    /// <returns>ロール統計</returns>
    Task<RoleStatistics> GetRoleStatisticsAsync();

    /// <summary>
    /// ユーザーロール分布を取得
    /// </summary>
    /// <returns>ロール別ユーザー数</returns>
    Task<Dictionary<string, int>> GetUserRoleDistributionAsync();

    /// <summary>
    /// 期限切れ間近のロール割り当てを取得
    /// </summary>
    /// <param name="warningDays">警告日数（デフォルト：7日）</param>
    /// <returns>期限切れ間近のユーザーロール一覧</returns>
    Task<List<UserRole>> GetExpiringRoleAssignmentsAsync(int warningDays = 7);

    /// <summary>
    /// 使用されていないロールを取得
    /// </summary>
    /// <returns>未使用ロール一覧</returns>
    Task<List<Role>> GetUnusedRolesAsync();

    /// <summary>
    /// 権限の使用状況を取得
    /// </summary>
    /// <returns>権限別使用ロール数</returns>
    Task<Dictionary<string, int>> GetPermissionUsageAsync();

    #endregion

    #region バリデーション

    /// <summary>
    /// ロール名の重複チェック
    /// </summary>
    /// <param name="roleName">ロール名</param>
    /// <param name="excludeRoleId">チェックから除外するロールID</param>
    /// <returns>重複している場合true</returns>
    Task<bool> IsRoleNameDuplicateAsync(string roleName, int? excludeRoleId = null);

    /// <summary>
    /// ロール削除可能性チェック
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <returns>削除可能な場合true</returns>
    Task<bool> CanDeleteRoleAsync(int roleId);

    /// <summary>
    /// ユーザーロール変更可能性チェック
    /// </summary>
    /// <param name="requesterId">要求者ID</param>
    /// <param name="targetUserId">対象ユーザーID</param>
    /// <param name="newRoleIds">新しいロールID一覧</param>
    /// <returns>変更可能な場合true</returns>
    Task<bool> CanChangeUserRolesAsync(string requesterId, string targetUserId, List<int> newRoleIds);

    /// <summary>
    /// 権限昇格の妥当性チェック
    /// </summary>
    /// <param name="requesterId">要求者ID</param>
    /// <param name="targetUserId">対象ユーザーID</param>
    /// <param name="newRoleId">新しいロールID</param>
    /// <returns>昇格可能な場合true</returns>
    Task<bool> CanEscalatePrivilegeAsync(string requesterId, string targetUserId, int newRoleId);

    #endregion

    #region システム初期化

    /// <summary>
    /// システムロールを初期化
    /// </summary>
    /// <param name="createdBy">作成者</param>
    /// <returns>初期化に成功した場合true</returns>
    Task<bool> InitializeSystemRolesAsync(string createdBy);

    /// <summary>
    /// デフォルトユーザーロールを設定
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="assignedBy">割り当て者</param>
    /// <returns>設定に成功した場合true</returns>
    Task<bool> AssignDefaultUserRoleAsync(string userId, string assignedBy);

    /// <summary>
    /// 管理者ユーザーを作成
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="createdBy">作成者</param>
    /// <returns>作成に成功した場合true</returns>
    Task<bool> CreateAdminUserAsync(string userId, string createdBy);

    #endregion
}

/// <summary>
/// ユーザーロール割り当て設定
/// </summary>
public class UserRoleAssignment
{
    /// <summary>
    /// ロールID
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// アクティブフラグ
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// プライマリロールフラグ
    /// </summary>
    public bool IsPrimary { get; set; } = false;

    /// <summary>
    /// 有効期限
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// コメント
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// ロール統計情報
/// </summary>
public class RoleStatistics
{
    /// <summary>
    /// 総ロール数
    /// </summary>
    public int TotalRoles { get; set; }

    /// <summary>
    /// アクティブなロール数
    /// </summary>
    public int ActiveRoles { get; set; }

    /// <summary>
    /// システムロール数
    /// </summary>
    public int SystemRoles { get; set; }

    /// <summary>
    /// カスタムロール数
    /// </summary>
    public int CustomRoles { get; set; }

    /// <summary>
    /// 総ユーザーロール割り当て数
    /// </summary>
    public int TotalUserRoleAssignments { get; set; }

    /// <summary>
    /// アクティブなユーザーロール割り当て数
    /// </summary>
    public int ActiveUserRoleAssignments { get; set; }

    /// <summary>
    /// 使用されていないロール数
    /// </summary>
    public int UnusedRoles { get; set; }

    /// <summary>
    /// 期限切れ間近のロール割り当て数
    /// </summary>
    public int ExpiringAssignments { get; set; }

    /// <summary>
    /// 最も使用されているロール
    /// </summary>
    public string? MostUsedRole { get; set; }

    /// <summary>
    /// 平均ユーザー当たりロール数
    /// </summary>
    public double AverageRolesPerUser { get; set; }
}
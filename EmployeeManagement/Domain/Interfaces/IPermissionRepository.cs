using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Domain.Interfaces;

/// <summary>
/// 権限リポジトリのインターフェース
/// </summary>
public interface IPermissionRepository
{
    /// <summary>
    /// 全ての権限を取得
    /// </summary>
    /// <param name="includeInactive">非アクティブな権限も含めるか</param>
    /// <returns>権限一覧</returns>
    Task<List<Permission>> GetAllAsync(bool includeInactive = false);

    /// <summary>
    /// IDで権限を取得
    /// </summary>
    /// <param name="id">権限ID</param>
    /// <returns>権限（見つからない場合はnull）</returns>
    Task<Permission?> GetByIdAsync(int id);

    /// <summary>
    /// 名前で権限を取得
    /// </summary>
    /// <param name="name">権限名</param>
    /// <returns>権限（見つからない場合はnull）</returns>
    Task<Permission?> GetByNameAsync(string name);

    /// <summary>
    /// モジュール別に権限を取得
    /// </summary>
    /// <param name="module">モジュール名</param>
    /// <param name="includeInactive">非アクティブな権限も含めるか</param>
    /// <returns>権限一覧</returns>
    Task<List<Permission>> GetByModuleAsync(string module, bool includeInactive = false);

    /// <summary>
    /// 操作種別で権限を取得
    /// </summary>
    /// <param name="action">操作種別</param>
    /// <param name="includeInactive">非アクティブな権限も含めるか</param>
    /// <returns>権限一覧</returns>
    Task<List<Permission>> GetByActionAsync(PermissionAction action, bool includeInactive = false);

    /// <summary>
    /// ロールの権限一覧を取得
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <param name="includeInactive">非アクティブな権限も含めるか</param>
    /// <returns>ロール権限一覧</returns>
    Task<List<RolePermission>> GetRolePermissionsAsync(int roleId, bool includeInactive = false);

    /// <summary>
    /// ユーザーの権限一覧を取得（全ロールから）
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="includeInactive">非アクティブな権限も含めるか</param>
    /// <returns>権限一覧</returns>
    Task<List<Permission>> GetUserPermissionsAsync(string userId, bool includeInactive = false);

    /// <summary>
    /// ユーザーが指定した権限を持っているかチェック
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="permissionName">権限名</param>
    /// <returns>権限を持っている場合true</returns>
    Task<bool> UserHasPermissionAsync(string userId, string permissionName);

    /// <summary>
    /// ユーザーが指定したモジュール・アクションの権限を持っているかチェック
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="module">モジュール名</param>
    /// <param name="action">操作種別</param>
    /// <param name="resource">リソース名（オプション）</param>
    /// <returns>権限を持っている場合true</returns>
    Task<bool> UserHasPermissionAsync(string userId, string module, PermissionAction action, string? resource = null);

    /// <summary>
    /// 権限を作成
    /// </summary>
    /// <param name="permission">作成する権限</param>
    /// <returns>作成された権限のID</returns>
    Task<int> CreateAsync(Permission permission);

    /// <summary>
    /// 権限を更新
    /// </summary>
    /// <param name="permission">更新する権限</param>
    /// <returns>更新に成功した場合true</returns>
    Task<bool> UpdateAsync(Permission permission);

    /// <summary>
    /// 権限を削除
    /// </summary>
    /// <param name="id">削除する権限のID</param>
    /// <returns>削除に成功した場合true</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// ロールに権限を割り当て
    /// </summary>
    /// <param name="rolePermission">ロール権限</param>
    /// <returns>割り当てに成功した場合true</returns>
    Task<bool> AssignPermissionToRoleAsync(RolePermission rolePermission);

    /// <summary>
    /// ロールから権限を削除
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <param name="permissionId">権限ID</param>
    /// <returns>削除に成功した場合true</returns>
    Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId);

    /// <summary>
    /// ロールの権限を一括更新
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <param name="permissionIds">権限ID一覧</param>
    /// <param name="updatedBy">更新者</param>
    /// <returns>更新に成功した場合true</returns>
    Task<bool> UpdateRolePermissionsAsync(int roleId, List<int> permissionIds, string updatedBy);

    /// <summary>
    /// 権限名が既に存在するかチェック
    /// </summary>
    /// <param name="name">権限名</param>
    /// <param name="excludeId">チェックから除外する権限ID</param>
    /// <returns>存在する場合true</returns>
    Task<bool> ExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// モジュール一覧を取得
    /// </summary>
    /// <param name="includeInactive">非アクティブな権限のモジュールも含めるか</param>
    /// <returns>モジュール名一覧</returns>
    Task<List<string>> GetModulesAsync(bool includeInactive = false);

    /// <summary>
    /// 権限に関連するロール数を取得
    /// </summary>
    /// <param name="permissionId">権限ID</param>
    /// <returns>関連ロール数</returns>
    Task<int> GetRelatedRoleCountAsync(int permissionId);

    /// <summary>
    /// アクティブな権限数を取得
    /// </summary>
    /// <returns>アクティブな権限数</returns>
    Task<int> GetActivePermissionCountAsync();

    /// <summary>
    /// システム権限を取得
    /// </summary>
    /// <param name="includeInactive">非アクティブな権限も含めるか</param>
    /// <returns>システム権限一覧</returns>
    Task<List<Permission>> GetSystemPermissionsAsync(bool includeInactive = false);

    /// <summary>
    /// カスタム権限を取得
    /// </summary>
    /// <param name="includeInactive">非アクティブな権限も含めるか</param>
    /// <returns>カスタム権限一覧</returns>
    Task<List<Permission>> GetCustomPermissionsAsync(bool includeInactive = false);
}
using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Domain.Interfaces;

/// <summary>
/// ロールリポジトリのインターフェース
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// 全てのロールを取得
    /// </summary>
    /// <param name="includeInactive">非アクティブなロールも含めるか</param>
    /// <returns>ロール一覧</returns>
    Task<List<Role>> GetAllAsync(bool includeInactive = false);

    /// <summary>
    /// IDでロールを取得
    /// </summary>
    /// <param name="id">ロールID</param>
    /// <returns>ロール（見つからない場合はnull）</returns>
    Task<Role?> GetByIdAsync(int id);

    /// <summary>
    /// 名前でロールを取得
    /// </summary>
    /// <param name="name">ロール名</param>
    /// <returns>ロール（見つからない場合はnull）</returns>
    Task<Role?> GetByNameAsync(string name);

    /// <summary>
    /// システムロールを取得
    /// </summary>
    /// <param name="systemRole">システムロール種別</param>
    /// <returns>ロール（見つからない場合はnull）</returns>
    Task<Role?> GetSystemRoleAsync(SystemRole systemRole);

    /// <summary>
    /// ユーザーのロール一覧を取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="includeInactive">非アクティブなロールも含めるか</param>
    /// <returns>ユーザーロール一覧</returns>
    Task<List<UserRole>> GetUserRolesAsync(string userId, bool includeInactive = false);

    /// <summary>
    /// ユーザーが指定したロールを持っているかチェック
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="roleName">ロール名</param>
    /// <returns>ロールを持っている場合true</returns>
    Task<bool> UserHasRoleAsync(string userId, string roleName);

    /// <summary>
    /// ロールを作成
    /// </summary>
    /// <param name="role">作成するロール</param>
    /// <returns>作成されたロールのID</returns>
    Task<int> CreateAsync(Role role);

    /// <summary>
    /// ロールを更新
    /// </summary>
    /// <param name="role">更新するロール</param>
    /// <returns>更新に成功した場合true</returns>
    Task<bool> UpdateAsync(Role role);

    /// <summary>
    /// ロールを削除
    /// </summary>
    /// <param name="id">削除するロールのID</param>
    /// <returns>削除に成功した場合true</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// ユーザーにロールを割り当て
    /// </summary>
    /// <param name="userRole">ユーザーロール</param>
    /// <returns>割り当てに成功した場合true</returns>
    Task<bool> AssignRoleToUserAsync(UserRole userRole);

    /// <summary>
    /// ユーザーからロールを削除
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="roleId">ロールID</param>
    /// <returns>削除に成功した場合true</returns>
    Task<bool> RemoveRoleFromUserAsync(string userId, int roleId);

    /// <summary>
    /// ロール名が既に存在するかチェック
    /// </summary>
    /// <param name="name">ロール名</param>
    /// <param name="excludeId">チェックから除外するロールID</param>
    /// <returns>存在する場合true</returns>
    Task<bool> ExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// ロールに割り当てられたユーザー数を取得
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <returns>ユーザー数</returns>
    Task<int> GetUserCountAsync(int roleId);

    /// <summary>
    /// アクティブなロール数を取得
    /// </summary>
    /// <returns>アクティブなロール数</returns>
    Task<int> GetActiveRoleCountAsync();
}
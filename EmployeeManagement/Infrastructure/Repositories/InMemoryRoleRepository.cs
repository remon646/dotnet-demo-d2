using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Infrastructure.DataStores;
using System.Collections.Concurrent;

namespace EmployeeManagement.Infrastructure.Repositories;

/// <summary>
/// ロールのインメモリリポジトリ実装
/// </summary>
public class InMemoryRoleRepository : IRoleRepository
{
    private readonly ConcurrentInMemoryDataStore _dataStore;

    public InMemoryRoleRepository(ConcurrentInMemoryDataStore dataStore)
    {
        _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
    }

    /// <summary>
    /// 全てのロールを取得
    /// </summary>
    /// <param name="includeInactive">非アクティブなロールも含めるか</param>
    /// <returns>ロール一覧</returns>
    public Task<List<Role>> GetAllAsync(bool includeInactive = false)
    {
        var roles = _dataStore.Roles.Values.ToList();
        
        if (!includeInactive)
        {
            roles = roles.Where(r => r.IsActive).ToList();
        }
        
        return Task.FromResult(roles.OrderBy(r => r.Priority).ThenBy(r => r.Name).ToList());
    }

    /// <summary>
    /// IDでロールを取得
    /// </summary>
    /// <param name="id">ロールID</param>
    /// <returns>ロール（見つからない場合はnull）</returns>
    public Task<Role?> GetByIdAsync(int id)
    {
        _dataStore.Roles.TryGetValue(id, out var role);
        return Task.FromResult(role);
    }

    /// <summary>
    /// 名前でロールを取得
    /// </summary>
    /// <param name="name">ロール名</param>
    /// <returns>ロール（見つからない場合はnull）</returns>
    public Task<Role?> GetByNameAsync(string name)
    {
        var role = _dataStore.Roles.Values
            .FirstOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
        
        return Task.FromResult(role);
    }

    /// <summary>
    /// システムロールを取得
    /// </summary>
    /// <param name="systemRole">システムロール種別</param>
    /// <returns>ロール（見つからない場合はnull）</returns>
    public Task<Role?> GetSystemRoleAsync(SystemRole systemRole)
    {
        var roleName = systemRole.GetName();
        return GetByNameAsync(roleName);
    }

    /// <summary>
    /// ユーザーのロール一覧を取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="includeInactive">非アクティブなロールも含めるか</param>
    /// <returns>ユーザーロール一覧</returns>
    public Task<List<UserRole>> GetUserRolesAsync(string userId, bool includeInactive = false)
    {
        var userRoles = _dataStore.UserRoles.Values
            .Where(ur => ur.UserId == userId)
            .ToList();

        if (!includeInactive)
        {
            userRoles = userRoles.Where(ur => ur.IsCurrentlyValid()).ToList();
        }

        // ロール情報を読み込み
        foreach (var userRole in userRoles)
        {
            if (_dataStore.Roles.TryGetValue(userRole.RoleId, out var role))
            {
                userRole.Role = role;
            }
        }

        return Task.FromResult(userRoles.OrderBy(ur => ur.Role?.Priority ?? 0).ToList());
    }

    /// <summary>
    /// ユーザーが指定したロールを持っているかチェック
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="roleName">ロール名</param>
    /// <returns>ロールを持っている場合true</returns>
    public async Task<bool> UserHasRoleAsync(string userId, string roleName)
    {
        var userRoles = await GetUserRolesAsync(userId, false);
        return userRoles.Any(ur => string.Equals(ur.Role?.Name, roleName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// ロールを作成
    /// </summary>
    /// <param name="role">作成するロール</param>
    /// <returns>作成されたロールのID</returns>
    public Task<int> CreateAsync(Role role)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));

        var id = _dataStore.GetNextRoleId();
        role.Id = id;
        role.CreatedAt = DateTime.Now;

        _dataStore.Roles.TryAdd(id, role);
        
        return Task.FromResult(id);
    }

    /// <summary>
    /// ロールを更新
    /// </summary>
    /// <param name="role">更新するロール</param>
    /// <returns>更新に成功した場合true</returns>
    public Task<bool> UpdateAsync(Role role)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));

        if (_dataStore.Roles.TryGetValue(role.Id, out var existingRole))
        {
            // システムロールの名前変更制限
            if (existingRole.IsSystemRole && existingRole.Name != role.Name)
            {
                throw new InvalidOperationException($"システムロール '{existingRole.Name}' の名前は変更できません。");
            }

            role.UpdatedAt = DateTime.Now;
            _dataStore.Roles.TryUpdate(role.Id, role, existingRole);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// ロールを削除
    /// </summary>
    /// <param name="id">削除するロールのID</param>
    /// <returns>削除に成功した場合true</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        if (!_dataStore.Roles.TryGetValue(id, out var role))
        {
            return false;
        }

        // システムロールは削除不可
        if (role.IsSystemRole)
        {
            throw new InvalidOperationException($"システムロール '{role.Name}' は削除できません。");
        }

        // ユーザーが割り当てられている場合は削除不可
        var userCount = await GetUserCountAsync(id);
        if (userCount > 0)
        {
            throw new InvalidOperationException($"ロール '{role.Name}' にはユーザーが割り当てられているため削除できません。");
        }

        // ロール権限を削除
        var rolePermissions = _dataStore.RolePermissions.Values
            .Where(rp => rp.RoleId == id)
            .ToList();

        foreach (var rolePermission in rolePermissions)
        {
            _dataStore.RolePermissions.TryRemove(rolePermission.Id, out _);
        }

        return _dataStore.Roles.TryRemove(id, out _);
    }

    /// <summary>
    /// ユーザーにロールを割り当て
    /// </summary>
    /// <param name="userRole">ユーザーロール</param>
    /// <returns>割り当てに成功した場合true</returns>
    public Task<bool> AssignRoleToUserAsync(UserRole userRole)
    {
        if (userRole == null) throw new ArgumentNullException(nameof(userRole));

        // 既存の割り当てをチェック
        var existingAssignment = _dataStore.UserRoles.Values
            .FirstOrDefault(ur => ur.UserId == userRole.UserId && ur.RoleId == userRole.RoleId);

        if (existingAssignment != null)
        {
            // 既存の割り当てがある場合は有効化
            existingAssignment.IsActive = true;
            existingAssignment.UpdatedAt = DateTime.Now;
            existingAssignment.UpdatedBy = userRole.AssignedBy;
            return Task.FromResult(true);
        }

        // 新規割り当て
        var id = _dataStore.GetNextUserRoleId();
        userRole.Id = id;
        userRole.AssignedAt = DateTime.Now;

        // プライマリロールの場合、他のプライマリロールを解除
        if (userRole.IsPrimary)
        {
            var existingPrimary = _dataStore.UserRoles.Values
                .Where(ur => ur.UserId == userRole.UserId && ur.IsPrimary && ur.IsActive)
                .ToList();

            foreach (var existing in existingPrimary)
            {
                existing.IsPrimary = false;
                existing.UpdatedAt = DateTime.Now;
                existing.UpdatedBy = userRole.AssignedBy;
            }
        }

        _dataStore.UserRoles.TryAdd(id, userRole);
        return Task.FromResult(true);
    }

    /// <summary>
    /// ユーザーからロールを削除
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="roleId">ロールID</param>
    /// <returns>削除に成功した場合true</returns>
    public Task<bool> RemoveRoleFromUserAsync(string userId, int roleId)
    {
        var userRole = _dataStore.UserRoles.Values
            .FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole != null)
        {
            return Task.FromResult(_dataStore.UserRoles.TryRemove(userRole.Id, out _));
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// ロール名が既に存在するかチェック
    /// </summary>
    /// <param name="name">ロール名</param>
    /// <param name="excludeId">チェックから除外するロールID</param>
    /// <returns>存在する場合true</returns>
    public Task<bool> ExistsAsync(string name, int? excludeId = null)
    {
        var exists = _dataStore.Roles.Values
            .Any(r => r.Id != excludeId && 
                     string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
        
        return Task.FromResult(exists);
    }

    /// <summary>
    /// ロールに割り当てられたユーザー数を取得
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <returns>ユーザー数</returns>
    public Task<int> GetUserCountAsync(int roleId)
    {
        var count = _dataStore.UserRoles.Values
            .Count(ur => ur.RoleId == roleId && ur.IsActive);
        
        return Task.FromResult(count);
    }

    /// <summary>
    /// アクティブなロール数を取得
    /// </summary>
    /// <returns>アクティブなロール数</returns>
    public Task<int> GetActiveRoleCountAsync()
    {
        var count = _dataStore.Roles.Values.Count(r => r.IsActive);
        return Task.FromResult(count);
    }
}
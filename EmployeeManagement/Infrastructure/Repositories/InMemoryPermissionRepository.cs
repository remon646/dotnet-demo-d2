using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Infrastructure.DataStores;
using System.Collections.Concurrent;

namespace EmployeeManagement.Infrastructure.Repositories;

/// <summary>
/// 権限のインメモリリポジトリ実装
/// </summary>
public class InMemoryPermissionRepository : IPermissionRepository
{
    private readonly ConcurrentInMemoryDataStore _dataStore;

    public InMemoryPermissionRepository(ConcurrentInMemoryDataStore dataStore)
    {
        _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
    }

    /// <summary>
    /// 全ての権限を取得
    /// </summary>
    /// <param name="includeInactive">非アクティブな権限も含めるか</param>
    /// <returns>権限一覧</returns>
    public Task<List<Permission>> GetAllAsync(bool includeInactive = false)
    {
        var permissions = _dataStore.Permissions.Values.ToList();
        
        if (!includeInactive)
        {
            permissions = permissions.Where(p => p.IsActive).ToList();
        }
        
        return Task.FromResult(permissions.OrderBy(p => p.Module).ThenBy(p => p.Action).ToList());
    }

    /// <summary>
    /// IDで権限を取得
    /// </summary>
    /// <param name="id">権限ID</param>
    /// <returns>権限（見つからない場合はnull）</returns>
    public Task<Permission?> GetByIdAsync(int id)
    {
        _dataStore.Permissions.TryGetValue(id, out var permission);
        return Task.FromResult(permission);
    }

    /// <summary>
    /// 名前で権限を取得
    /// </summary>
    /// <param name="name">権限名</param>
    /// <returns>権限（見つからない場合はnull）</returns>
    public Task<Permission?> GetByNameAsync(string name)
    {
        var permission = _dataStore.Permissions.Values
            .FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        
        return Task.FromResult(permission);
    }

    /// <summary>
    /// モジュール別に権限を取得
    /// </summary>
    /// <param name="module">モジュール名</param>
    /// <param name="includeInactive">非アクティブな権限も含めるか</param>
    /// <returns>権限一覧</returns>
    public Task<List<Permission>> GetByModuleAsync(string module, bool includeInactive = false)
    {
        var permissions = _dataStore.Permissions.Values
            .Where(p => string.Equals(p.Module, module, StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        if (!includeInactive)
        {
            permissions = permissions.Where(p => p.IsActive).ToList();
        }
        
        return Task.FromResult(permissions.OrderBy(p => p.Action).ToList());
    }

    /// <summary>
    /// 操作種別で権限を取得
    /// </summary>
    /// <param name="action">操作種別</param>
    /// <param name="includeInactive">非アクティブな権限も含めるか</param>
    /// <returns>権限一覧</returns>
    public Task<List<Permission>> GetByActionAsync(PermissionAction action, bool includeInactive = false)
    {
        var permissions = _dataStore.Permissions.Values
            .Where(p => p.Action == action)
            .ToList();
        
        if (!includeInactive)
        {
            permissions = permissions.Where(p => p.IsActive).ToList();
        }
        
        return Task.FromResult(permissions.OrderBy(p => p.Module).ToList());
    }

    /// <summary>
    /// ロールの権限一覧を取得
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <param name="includeInactive">非アクティブな権限も含めるか</param>
    /// <returns>ロール権限一覧</returns>
    public Task<List<RolePermission>> GetRolePermissionsAsync(int roleId, bool includeInactive = false)
    {
        var rolePermissions = _dataStore.RolePermissions.Values
            .Where(rp => rp.RoleId == roleId)
            .ToList();

        if (!includeInactive)
        {
            rolePermissions = rolePermissions.Where(rp => rp.IsCurrentlyValid()).ToList();
        }

        // 権限情報を読み込み
        foreach (var rolePermission in rolePermissions)
        {
            if (_dataStore.Permissions.TryGetValue(rolePermission.PermissionId, out var permission))
            {
                rolePermission.Permission = permission;
            }
            if (_dataStore.Roles.TryGetValue(rolePermission.RoleId, out var role))
            {
                rolePermission.Role = role;
            }
        }

        return Task.FromResult(rolePermissions.OrderBy(rp => rp.Permission?.Module).ThenBy(rp => rp.Permission?.Action).ToList());
    }

    /// <summary>
    /// ユーザーの権限一覧を取得（全ロールから）
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="includeInactive">非アクティブな権限も含めるか</param>
    /// <returns>権限一覧</returns>
    public async Task<List<Permission>> GetUserPermissionsAsync(string userId, bool includeInactive = false)
    {
        // ユーザーのロールを取得
        var userRoles = _dataStore.UserRoles.Values
            .Where(ur => ur.UserId == userId && (includeInactive || ur.IsCurrentlyValid()))
            .ToList();

        var permissions = new List<Permission>();

        foreach (var userRole in userRoles)
        {
            var rolePermissions = await GetRolePermissionsAsync(userRole.RoleId, includeInactive);
            foreach (var rolePermission in rolePermissions.Where(rp => rp.IsGranted))
            {
                if (rolePermission.Permission != null && !permissions.Any(p => p.Id == rolePermission.Permission.Id))
                {
                    permissions.Add(rolePermission.Permission);
                }
            }
        }

        return permissions.OrderBy(p => p.Module).ThenBy(p => p.Action).ToList();
    }

    /// <summary>
    /// ユーザーが指定した権限を持っているかチェック
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="permissionName">権限名</param>
    /// <returns>権限を持っている場合true</returns>
    public async Task<bool> UserHasPermissionAsync(string userId, string permissionName)
    {
        var permissions = await GetUserPermissionsAsync(userId, false);
        return permissions.Any(p => string.Equals(p.Name, permissionName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// ユーザーが指定したモジュール・アクションの権限を持っているかチェック
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="module">モジュール名</param>
    /// <param name="action">操作種別</param>
    /// <param name="resource">リソース名（オプション）</param>
    /// <returns>権限を持っている場合true</returns>
    public async Task<bool> UserHasPermissionAsync(string userId, string module, PermissionAction action, string? resource = null)
    {
        var permissions = await GetUserPermissionsAsync(userId, false);
        
        return permissions.Any(p => 
            string.Equals(p.Module, module, StringComparison.OrdinalIgnoreCase) &&
            p.Action == action &&
            (string.IsNullOrEmpty(resource) || string.Equals(p.Resource, resource, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// 権限を作成
    /// </summary>
    /// <param name="permission">作成する権限</param>
    /// <returns>作成された権限のID</returns>
    public Task<int> CreateAsync(Permission permission)
    {
        if (permission == null) throw new ArgumentNullException(nameof(permission));

        var id = _dataStore.GetNextPermissionId();
        permission.Id = id;
        permission.CreatedAt = DateTime.Now;

        _dataStore.Permissions.TryAdd(id, permission);
        
        return Task.FromResult(id);
    }

    /// <summary>
    /// 権限を更新
    /// </summary>
    /// <param name="permission">更新する権限</param>
    /// <returns>更新に成功した場合true</returns>
    public Task<bool> UpdateAsync(Permission permission)
    {
        if (permission == null) throw new ArgumentNullException(nameof(permission));

        if (_dataStore.Permissions.TryGetValue(permission.Id, out var existingPermission))
        {
            // システム権限の名前変更制限
            if (existingPermission.IsSystemPermission && existingPermission.Name != permission.Name)
            {
                throw new InvalidOperationException($"システム権限 '{existingPermission.Name}' の名前は変更できません。");
            }

            permission.UpdatedAt = DateTime.Now;
            _dataStore.Permissions.TryUpdate(permission.Id, permission, existingPermission);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// 権限を削除
    /// </summary>
    /// <param name="id">削除する権限のID</param>
    /// <returns>削除に成功した場合true</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        if (!_dataStore.Permissions.TryGetValue(id, out var permission))
        {
            return false;
        }

        // システム権限は削除不可
        if (permission.IsSystemPermission)
        {
            throw new InvalidOperationException($"システム権限 '{permission.Name}' は削除できません。");
        }

        // ロールに割り当てられている場合は削除不可
        var roleCount = await GetRelatedRoleCountAsync(id);
        if (roleCount > 0)
        {
            throw new InvalidOperationException($"権限 '{permission.Name}' はロールに割り当てられているため削除できません。");
        }

        return _dataStore.Permissions.TryRemove(id, out _);
    }

    /// <summary>
    /// ロールに権限を割り当て
    /// </summary>
    /// <param name="rolePermission">ロール権限</param>
    /// <returns>割り当てに成功した場合true</returns>
    public Task<bool> AssignPermissionToRoleAsync(RolePermission rolePermission)
    {
        if (rolePermission == null) throw new ArgumentNullException(nameof(rolePermission));

        // 既存の割り当てをチェック
        var existingAssignment = _dataStore.RolePermissions.Values
            .FirstOrDefault(rp => rp.RoleId == rolePermission.RoleId && rp.PermissionId == rolePermission.PermissionId);

        if (existingAssignment != null)
        {
            // 既存の割り当てがある場合は更新
            existingAssignment.IsGranted = rolePermission.IsGranted;
            existingAssignment.GrantedAt = DateTime.Now;
            existingAssignment.GrantedBy = rolePermission.GrantedBy;
            existingAssignment.Comment = rolePermission.Comment;
            return Task.FromResult(true);
        }

        // 新規割り当て
        var id = _dataStore.GetNextRolePermissionId();
        rolePermission.Id = id;
        rolePermission.GrantedAt = DateTime.Now;

        _dataStore.RolePermissions.TryAdd(id, rolePermission);
        return Task.FromResult(true);
    }

    /// <summary>
    /// ロールから権限を削除
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <param name="permissionId">権限ID</param>
    /// <returns>削除に成功した場合true</returns>
    public Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId)
    {
        var rolePermission = _dataStore.RolePermissions.Values
            .FirstOrDefault(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (rolePermission != null)
        {
            return Task.FromResult(_dataStore.RolePermissions.TryRemove(rolePermission.Id, out _));
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// ロールの権限を一括更新
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <param name="permissionIds">権限ID一覧</param>
    /// <param name="updatedBy">更新者</param>
    /// <returns>更新に成功した場合true</returns>
    public async Task<bool> UpdateRolePermissionsAsync(int roleId, List<int> permissionIds, string updatedBy)
    {
        try
        {
            // 既存の権限を全て削除
            var existingPermissions = await GetRolePermissionsAsync(roleId, true);
            foreach (var existing in existingPermissions)
            {
                _dataStore.RolePermissions.TryRemove(existing.Id, out _);
            }

            // 新しい権限を追加
            foreach (var permissionId in permissionIds)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    IsGranted = true,
                    GrantedBy = updatedBy,
                    GrantedAt = DateTime.Now
                };

                await AssignPermissionToRoleAsync(rolePermission);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 権限名が既に存在するかチェック
    /// </summary>
    /// <param name="name">権限名</param>
    /// <param name="excludeId">チェックから除外する権限ID</param>
    /// <returns>存在する場合true</returns>
    public Task<bool> ExistsAsync(string name, int? excludeId = null)
    {
        var exists = _dataStore.Permissions.Values
            .Any(p => p.Id != excludeId && 
                     string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        
        return Task.FromResult(exists);
    }

    /// <summary>
    /// モジュール一覧を取得
    /// </summary>
    /// <param name="includeInactive">非アクティブな権限のモジュールも含めるか</param>
    /// <returns>モジュール名一覧</returns>
    public Task<List<string>> GetModulesAsync(bool includeInactive = false)
    {
        var query = _dataStore.Permissions.Values.AsEnumerable();
        
        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        var modules = query
            .Select(p => p.Module)
            .Distinct()
            .OrderBy(m => m)
            .ToList();

        return Task.FromResult(modules);
    }

    /// <summary>
    /// 権限に関連するロール数を取得
    /// </summary>
    /// <param name="permissionId">権限ID</param>
    /// <returns>関連ロール数</returns>
    public Task<int> GetRelatedRoleCountAsync(int permissionId)
    {
        var count = _dataStore.RolePermissions.Values
            .Count(rp => rp.PermissionId == permissionId && rp.IsGranted);
        
        return Task.FromResult(count);
    }

    /// <summary>
    /// アクティブな権限数を取得
    /// </summary>
    /// <returns>アクティブな権限数</returns>
    public Task<int> GetActivePermissionCountAsync()
    {
        var count = _dataStore.Permissions.Values.Count(p => p.IsActive);
        return Task.FromResult(count);
    }

    /// <summary>
    /// システム権限を取得
    /// </summary>
    /// <param name="includeInactive">非アクティブな権限も含めるか</param>
    /// <returns>システム権限一覧</returns>
    public Task<List<Permission>> GetSystemPermissionsAsync(bool includeInactive = false)
    {
        var permissions = _dataStore.Permissions.Values
            .Where(p => p.IsSystemPermission)
            .ToList();
        
        if (!includeInactive)
        {
            permissions = permissions.Where(p => p.IsActive).ToList();
        }
        
        return Task.FromResult(permissions.OrderBy(p => p.Module).ThenBy(p => p.Action).ToList());
    }

    /// <summary>
    /// カスタム権限を取得
    /// </summary>
    /// <param name="includeInactive">非アクティブな権限も含めるか</param>
    /// <returns>カスタム権限一覧</returns>
    public Task<List<Permission>> GetCustomPermissionsAsync(bool includeInactive = false)
    {
        var permissions = _dataStore.Permissions.Values
            .Where(p => !p.IsSystemPermission)
            .ToList();
        
        if (!includeInactive)
        {
            permissions = permissions.Where(p => p.IsActive).ToList();
        }
        
        return Task.FromResult(permissions.OrderBy(p => p.Module).ThenBy(p => p.Action).ToList());
    }
}
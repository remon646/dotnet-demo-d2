using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Constants;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Application.Services;

/// <summary>
/// 認可サービスの実装
/// ユーザーの権限チェックとロール管理機能を提供
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly Dictionary<string, List<Permission>> _permissionCache = new();
    private readonly Dictionary<string, List<Role>> _roleCache = new();
    private readonly object _cacheLock = new object();

    public AuthorizationService(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository)
    {
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
    }

    #region 権限チェック

    /// <summary>
    /// ユーザーが指定した権限を持っているかチェック
    /// </summary>
    public async Task<bool> HasPermissionAsync(string userId, string permissionName)
    {
        try
        {
            return await _permissionRepository.UserHasPermissionAsync(userId, permissionName);
        }
        catch (Exception)
        {
            // エラー時は安全側に倒してfalseを返す
            return false;
        }
    }

    /// <summary>
    /// ユーザーが指定したモジュール・アクションの権限を持っているかチェック
    /// </summary>
    public async Task<bool> HasPermissionAsync(string userId, string module, PermissionAction action, string? resource = null)
    {
        try
        {
            return await _permissionRepository.UserHasPermissionAsync(userId, module, action, resource);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// ユーザーがいずれかの権限を持っているかチェック
    /// </summary>
    public async Task<bool> HasAnyPermissionAsync(string userId, params string[] permissionNames)
    {
        foreach (var permissionName in permissionNames)
        {
            if (await HasPermissionAsync(userId, permissionName))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// ユーザーがすべての権限を持っているかチェック
    /// </summary>
    public async Task<bool> HasAllPermissionsAsync(string userId, params string[] permissionNames)
    {
        foreach (var permissionName in permissionNames)
        {
            if (!await HasPermissionAsync(userId, permissionName))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// リソースへのアクセス権限をチェック
    /// </summary>
    public async Task<bool> CanAccessAsync(string userId, string resource, PermissionAction action)
    {
        try
        {
            // リソース固有の権限をチェック
            var hasSpecificPermission = await _permissionRepository.UserHasPermissionAsync(userId, resource, action);
            if (hasSpecificPermission) return true;

            // 管理権限をチェック
            var hasManagePermission = await _permissionRepository.UserHasPermissionAsync(userId, resource, PermissionAction.Manage);
            return hasManagePermission;
        }
        catch (Exception)
        {
            return false;
        }
    }

    #endregion

    #region ロールチェック

    /// <summary>
    /// ユーザーが指定したロールを持っているかチェック
    /// </summary>
    public async Task<bool> IsInRoleAsync(string userId, string roleName)
    {
        try
        {
            return await _roleRepository.UserHasRoleAsync(userId, roleName);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// ユーザーが指定したシステムロールを持っているかチェック
    /// </summary>
    public async Task<bool> IsInRoleAsync(string userId, SystemRole systemRole)
    {
        return await IsInRoleAsync(userId, systemRole.GetName());
    }

    /// <summary>
    /// ユーザーがいずれかのロールを持っているかチェック
    /// </summary>
    public async Task<bool> IsInAnyRoleAsync(string userId, params string[] roleNames)
    {
        foreach (var roleName in roleNames)
        {
            if (await IsInRoleAsync(userId, roleName))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// ユーザーがすべてのロールを持っているかチェック
    /// </summary>
    public async Task<bool> IsInAllRolesAsync(string userId, params string[] roleNames)
    {
        foreach (var roleName in roleNames)
        {
            if (!await IsInRoleAsync(userId, roleName))
            {
                return false;
            }
        }
        return true;
    }

    #endregion

    #region ユーザー情報取得

    /// <summary>
    /// ユーザーの権限一覧を取得
    /// </summary>
    public async Task<List<Permission>> GetUserPermissionsAsync(string userId)
    {
        try
        {
            // キャッシュチェック
            lock (_cacheLock)
            {
                if (_permissionCache.TryGetValue(userId, out var cachedPermissions))
                {
                    return cachedPermissions;
                }
            }

            var permissions = await _permissionRepository.GetUserPermissionsAsync(userId, false);

            // キャッシュに保存
            lock (_cacheLock)
            {
                _permissionCache[userId] = permissions;
            }

            return permissions;
        }
        catch (Exception)
        {
            return new List<Permission>();
        }
    }

    /// <summary>
    /// ユーザーのロール一覧を取得
    /// </summary>
    public async Task<List<Role>> GetUserRolesAsync(string userId)
    {
        try
        {
            // キャッシュチェック
            lock (_cacheLock)
            {
                if (_roleCache.TryGetValue(userId, out var cachedRoles))
                {
                    return cachedRoles;
                }
            }

            var userRoles = await _roleRepository.GetUserRolesAsync(userId, false);
            var roles = userRoles.Select(ur => ur.Role).Where(r => r != null).Cast<Role>().ToList();

            // キャッシュに保存
            lock (_cacheLock)
            {
                _roleCache[userId] = roles;
            }

            return roles;
        }
        catch (Exception)
        {
            return new List<Role>();
        }
    }

    /// <summary>
    /// ユーザーのプライマリロールを取得
    /// </summary>
    public async Task<Role?> GetUserPrimaryRoleAsync(string userId)
    {
        try
        {
            var userRoles = await _roleRepository.GetUserRolesAsync(userId, false);
            var primaryUserRole = userRoles.FirstOrDefault(ur => ur.IsPrimary);
            return primaryUserRole?.Role;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// ユーザーの最高権限ロールを取得
    /// </summary>
    public async Task<Role?> GetUserHighestRoleAsync(string userId)
    {
        try
        {
            var roles = await GetUserRolesAsync(userId);
            return roles.OrderByDescending(r => r.Priority).FirstOrDefault();
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// ユーザーのロール割り当て詳細を取得
    /// </summary>
    public async Task<List<UserRole>> GetUserRoleAssignmentsAsync(string userId)
    {
        try
        {
            return await _roleRepository.GetUserRolesAsync(userId, false);
        }
        catch (Exception)
        {
            return new List<UserRole>();
        }
    }

    #endregion

    #region データフィルタリング

    /// <summary>
    /// 権限に基づいてデータをフィルタリング
    /// </summary>
    public async Task<List<T>> FilterByPermissionAsync<T>(string userId, List<T> data, string permissionName) where T : class
    {
        var hasPermission = await HasPermissionAsync(userId, permissionName);
        return hasPermission ? data : new List<T>();
    }

    /// <summary>
    /// ユーザーがアクセス可能なデータのIDを取得
    /// </summary>
    public async Task<List<string>> GetAccessibleResourceIdsAsync(string userId, string resourceType, PermissionAction action)
    {
        // 基本実装：権限があれば全てにアクセス可能
        var hasPermission = await HasPermissionAsync(userId, resourceType, action);
        if (!hasPermission)
        {
            return new List<string>();
        }

        // TODO: より細かい制御が必要な場合はここで実装
        return new List<string> { "*" }; // 全てアクセス可能を示す
    }

    #endregion

    #region 権限階層・継承

    /// <summary>
    /// 権限が他の権限を包含しているかチェック
    /// </summary>
    public async Task<bool> IsPermissionIncludedAsync(string parentPermission, string childPermission)
    {
        // 管理権限は対象モジュールの全ての権限を包含する
        if (parentPermission.EndsWith(".Manage"))
        {
            var parentModule = parentPermission.Split('.')[0];
            var childModule = childPermission.Split('.')[0];
            return string.Equals(parentModule, childModule, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    /// <summary>
    /// ロールが他のロールを包含しているかチェック
    /// </summary>
    public async Task<bool> IsRoleIncludedAsync(string parentRole, string childRole)
    {
        var parentRoleEntity = await _roleRepository.GetByNameAsync(parentRole);
        var childRoleEntity = await _roleRepository.GetByNameAsync(childRole);

        if (parentRoleEntity == null || childRoleEntity == null)
            return false;

        // 優先度による階層チェック
        return parentRoleEntity.Priority > childRoleEntity.Priority;
    }

    /// <summary>
    /// ユーザーの有効な権限を階層を考慮して取得
    /// </summary>
    public async Task<List<Permission>> GetEffectivePermissionsAsync(string userId)
    {
        return await GetUserPermissionsAsync(userId);
    }

    #endregion

    #region セキュリティ・監査

    /// <summary>
    /// ユーザーの権限使用履歴を記録
    /// </summary>
    public async Task<bool> LogPermissionUsageAsync(string userId, string permissionName, string? resource = null, bool success = true)
    {
        // TODO: 監査ログシステムが実装されたら連携
        return await Task.FromResult(true);
    }

    /// <summary>
    /// セキュリティコンテキストを取得
    /// </summary>
    public async Task<SecurityContext> GetSecurityContextAsync(string userId)
    {
        var roles = await GetUserRolesAsync(userId);
        var permissions = await GetUserPermissionsAsync(userId);
        var primaryRole = await GetUserPrimaryRoleAsync(userId);
        var isSystemAdmin = await IsInRoleAsync(userId, SystemRole.SystemAdmin);

        return new SecurityContext
        {
            UserId = userId,
            UserName = userId, // TODO: 実際のユーザー名を取得
            PrimaryRole = primaryRole,
            Roles = roles,
            Permissions = permissions,
            HighestPrivilegeLevel = roles.Max(r => r?.Priority ?? 0),
            IsSystemAdmin = isSystemAdmin,
            Clearance = DetermineSecurityClearance(roles),
            LastPermissionCheck = DateTime.Now
        };
    }

    /// <summary>
    /// ユーザーの権限変更を検証
    /// </summary>
    public async Task<bool> CanChangeUserRolesAsync(string userId, string requesterId, List<string> newRoles)
    {
        // 要求者がシステム管理者かどうかチェック
        var isRequesterAdmin = await IsInRoleAsync(requesterId, SystemRole.SystemAdmin);
        if (!isRequesterAdmin)
        {
            return false;
        }

        // 自分自身の権限変更は制限
        if (userId == requesterId)
        {
            return false;
        }

        return true;
    }

    #endregion

    #region キャッシュ・パフォーマンス

    /// <summary>
    /// ユーザーの権限キャッシュをクリア
    /// </summary>
    public Task ClearUserPermissionCacheAsync(string userId)
    {
        lock (_cacheLock)
        {
            _permissionCache.Remove(userId);
            _roleCache.Remove(userId);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 全権限キャッシュをクリア
    /// </summary>
    public Task ClearAllPermissionCacheAsync()
    {
        lock (_cacheLock)
        {
            _permissionCache.Clear();
            _roleCache.Clear();
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 権限キャッシュを更新
    /// </summary>
    public async Task RefreshUserPermissionCacheAsync(string userId)
    {
        await ClearUserPermissionCacheAsync(userId);
        // 次回アクセス時に自動的に再キャッシュされる
    }

    #endregion

    #region プライベートメソッド

    /// <summary>
    /// ロールからセキュリティクリアランスを決定
    /// </summary>
    private SecurityClearance DetermineSecurityClearance(List<Role> roles)
    {
        if (roles.Any(r => r.Name == "SystemAdmin"))
            return SecurityClearance.System;
        
        if (roles.Any(r => r.Name == "HRManager"))
            return SecurityClearance.Administrative;
        
        if (roles.Any(r => r.Name == "DepartmentManager"))
            return SecurityClearance.Advanced;
        
        if (roles.Any(r => r.Name == "User"))
            return SecurityClearance.Standard;
        
        return SecurityClearance.Basic;
    }

    #endregion
}
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Constants;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Application.Services;

/// <summary>
/// ロール初期化サービス
/// システム起動時にデフォルトロールと権限を設定する
/// </summary>
public class RoleInitializationService : IRoleInitializationService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;

    public RoleInitializationService(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository)
    {
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
    }

    /// <summary>
    /// システムロールを初期化
    /// </summary>
    public async Task<bool> InitializeSystemRolesAsync(string createdBy)
    {
        try
        {
            // 権限を初期化
            await InitializeSystemPermissionsAsync(createdBy);

            // ロールを初期化
            await CreateSystemRolesAsync(createdBy);

            // ロールに権限を割り当て
            await AssignPermissionsToRolesAsync(createdBy);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// デフォルトユーザーロールを設定
    /// </summary>
    public async Task<bool> AssignDefaultUserRoleAsync(string userId, string assignedBy)
    {
        try
        {
            var userRole = await _roleRepository.GetByNameAsync("User");
            if (userRole == null) return false;

            var userRoleAssignment = new UserRole
            {
                UserId = userId,
                RoleId = userRole.Id,
                AssignedBy = assignedBy,
                IsPrimary = true,
                IsActive = true
            };

            return await _roleRepository.AssignRoleToUserAsync(userRoleAssignment);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 管理者ユーザーを作成
    /// </summary>
    public async Task<bool> CreateAdminUserAsync(string userId, string createdBy)
    {
        try
        {
            var adminRole = await _roleRepository.GetSystemRoleAsync(SystemRole.SystemAdmin);
            if (adminRole == null) return false;

            var userRoleAssignment = new UserRole
            {
                UserId = userId,
                RoleId = adminRole.Id,
                AssignedBy = createdBy,
                IsPrimary = true,
                IsActive = true
            };

            return await _roleRepository.AssignRoleToUserAsync(userRoleAssignment);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// システム権限を初期化
    /// </summary>
    private async Task InitializeSystemPermissionsAsync(string createdBy)
    {
        var allPermissions = SystemPermissions.GetAllPermissions();

        foreach (var permissionName in allPermissions)
        {
            var existingPermission = await _permissionRepository.GetByNameAsync(permissionName);
            if (existingPermission != null) continue;

            var (module, action, resource) = Permission.ParsePermissionName(permissionName);
            
            var permission = new Permission
            {
                Name = permissionName,
                Description = SystemPermissions.GetPermissionDescription(permissionName),
                Module = module,
                Action = action,
                Resource = resource,
                IsSystemPermission = true,
                IsActive = true,
                CreatedBy = createdBy
            };

            await _permissionRepository.CreateAsync(permission);
        }
    }

    /// <summary>
    /// システムロールを作成
    /// </summary>
    private async Task CreateSystemRolesAsync(string createdBy)
    {
        var systemRoles = new[]
        {
            SystemRole.SystemAdmin,
            SystemRole.HRManager,
            SystemRole.DepartmentManager,
            SystemRole.User,
            SystemRole.Guest
        };

        foreach (var systemRole in systemRoles)
        {
            var existingRole = await _roleRepository.GetSystemRoleAsync(systemRole);
            if (existingRole != null) continue;

            var role = new Role
            {
                Name = systemRole.GetName(),
                Description = systemRole.GetDescription(),
                Priority = systemRole.GetPriority(),
                IsSystemRole = true,
                IsActive = true,
                CreatedBy = createdBy
            };

            await _roleRepository.CreateAsync(role);
        }
    }

    /// <summary>
    /// ロールに権限を割り当て
    /// </summary>
    private async Task AssignPermissionsToRolesAsync(string assignedBy)
    {
        await AssignPermissionsToRoleAsync("SystemAdmin", SystemPermissions.GetAdminPermissions(), assignedBy);
        await AssignPermissionsToRoleAsync("HRManager", SystemPermissions.GetHRManagerPermissions(), assignedBy);
        await AssignPermissionsToRoleAsync("DepartmentManager", SystemPermissions.GetDepartmentManagerPermissions(), assignedBy);
        await AssignPermissionsToRoleAsync("User", SystemPermissions.GetUserPermissions(), assignedBy);
        await AssignPermissionsToRoleAsync("Guest", SystemPermissions.GetGuestPermissions(), assignedBy);
    }

    /// <summary>
    /// 特定のロールに権限を割り当て
    /// </summary>
    private async Task AssignPermissionsToRoleAsync(string roleName, string[] permissionNames, string assignedBy)
    {
        var role = await _roleRepository.GetByNameAsync(roleName);
        if (role == null) return;

        foreach (var permissionName in permissionNames)
        {
            var permission = await _permissionRepository.GetByNameAsync(permissionName);
            if (permission == null) continue;

            var rolePermission = new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id,
                IsGranted = true,
                GrantedBy = assignedBy
            };

            await _permissionRepository.AssignPermissionToRoleAsync(rolePermission);
        }
    }
}

/// <summary>
/// ロール初期化サービスのインターフェース
/// </summary>
public interface IRoleInitializationService
{
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
}
# 権限管理システム実装計画

## 概要
ロールベースアクセス制御（RBAC）システムを実装し、ユーザーの役割に応じた機能・データへのアクセス制御を提供します。セキュリティを強化し、適切な権限分離を実現します。

## 機能要件

### 基本権限システム
- ✅ ロール（役割）管理
- ✅ 権限（Permission）管理
- ✅ ユーザーへのロール割り当て
- ✅ 機能別アクセス制御
- ✅ データレベルアクセス制御
- ✅ 権限継承システム

### 事前定義ロール
- ✅ **システム管理者**: 全機能アクセス
- ✅ **人事管理者**: 社員・部署管理
- ✅ **部門管理者**: 所属部署の社員管理
- ✅ **一般ユーザー**: 閲覧権限のみ
- ✅ **ゲストユーザー**: 限定的な閲覧権限

### 権限制御対象
- ✅ メニュー・ページアクセス
- ✅ CRUD操作権限
- ✅ データフィルタリング
- ✅ エクスポート機能
- ✅ レポート生成
- ✅ 設定変更権限

### 管理機能
- ✅ ロール・権限管理画面
- ✅ ユーザー権限一覧
- ✅ 権限監査ログ
- ✅ 権限テスト機能

## 技術実装詳細

### 1. 権限モデル

#### ロールモデル
```csharp
public class Role
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Priority { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public List<RolePermission> Permissions { get; set; } = new();
    public List<UserRole> UserRoles { get; set; } = new();
}

public enum SystemRole
{
    SystemAdmin = 1,
    HRManager = 2,
    DepartmentManager = 3,
    User = 4,
    Guest = 5
}
```

#### 権限モデル
```csharp
public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Module { get; set; }        // Employee, Department, Report, etc.
    public PermissionAction Action { get; set; }
    public string Resource { get; set; }       // 対象リソース（任意）
    public bool IsSystemPermission { get; set; }
    public List<RolePermission> RolePermissions { get; set; } = new();
}

public enum PermissionAction
{
    View = 1,
    Create = 2,
    Update = 3,
    Delete = 4,
    Export = 5,
    Import = 6,
    Manage = 7,
    Execute = 8
}

public class RolePermission
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    public bool IsGranted { get; set; } = true;
    public DateTime GrantedAt { get; set; }
    public string GrantedBy { get; set; }
    public Role Role { get; set; }
    public Permission Permission { get; set; }
}
```

#### ユーザーロール
```csharp
public class UserRole
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int RoleId { get; set; }
    public DateTime AssignedAt { get; set; }
    public string AssignedBy { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public Role Role { get; set; }
    public User User { get; set; }
}
```

### 2. 権限チェックシステム

#### 権限サービス
```csharp
public interface IAuthorizationService
{
    Task<bool> HasPermissionAsync(string userId, string permissionName);
    Task<bool> HasPermissionAsync(string userId, string module, PermissionAction action, string resource = null);
    Task<bool> IsInRoleAsync(string userId, string roleName);
    Task<bool> IsInRoleAsync(string userId, SystemRole systemRole);
    Task<List<Permission>> GetUserPermissionsAsync(string userId);
    Task<List<Role>> GetUserRolesAsync(string userId);
    Task<bool> CanAccessAsync(string userId, string resource, PermissionAction action);
    Task<List<T>> FilterByPermissionAsync<T>(string userId, List<T> data, string permissionName) where T : class;
}
```

#### データフィルタリングサービス
```csharp
public interface IDataAuthorizationService
{
    Task<List<Employee>> FilterEmployeesByPermissionAsync(string userId, List<Employee> employees);
    Task<List<DepartmentMaster>> FilterDepartmentsByPermissionAsync(string userId, List<DepartmentMaster> departments);
    Task<bool> CanViewEmployeeAsync(string userId, string employeeId);
    Task<bool> CanEditEmployeeAsync(string userId, string employeeId);
    Task<bool> CanDeleteEmployeeAsync(string userId, string employeeId);
    Task<bool> CanViewDepartmentAsync(string userId, string departmentId);
    Task<bool> CanManageDepartmentAsync(string userId, string departmentId);
}
```

### 3. 権限チェック属性

#### カスタム認可属性
```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute, IAuthorizationRequirement
{
    public string PermissionName { get; }
    public string Module { get; }
    public PermissionAction Action { get; }
    
    public RequirePermissionAttribute(string permissionName)
    {
        PermissionName = permissionName;
    }
    
    public RequirePermissionAttribute(string module, PermissionAction action)
    {
        Module = module;
        Action = action;
    }
}

public class PermissionAuthorizationHandler : AuthorizationHandler<RequirePermissionAttribute>
{
    private readonly IAuthorizationService _authService;
    
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RequirePermissionAttribute requirement)
    {
        var userId = context.User?.Identity?.Name;
        if (userId == null)
        {
            context.Fail();
            return;
        }
        
        bool hasPermission;
        if (!string.IsNullOrEmpty(requirement.PermissionName))
        {
            hasPermission = await _authService.HasPermissionAsync(userId, requirement.PermissionName);
        }
        else
        {
            hasPermission = await _authService.HasPermissionAsync(userId, requirement.Module, requirement.Action);
        }
        
        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
```

### 4. UI コンポーネント統合

#### 権限チェックコンポーネント
```razor
<!-- AuthorizedView.razor -->
@inherits ComponentBase

@if (_hasPermission)
{
    @ChildContent
}
else if (NotAuthorized != null)
{
    @NotAuthorized
}

@code {
    [Parameter] public string Permission { get; set; }
    [Parameter] public string Module { get; set; }
    [Parameter] public PermissionAction? Action { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }
    [Parameter] public RenderFragment NotAuthorized { get; set; }
    
    [Inject] private IAuthorizationService AuthService { get; set; }
    [Inject] private IAuthenticationService AuthenticationService { get; set; }
    
    private bool _hasPermission = false;
    
    protected override async Task OnInitializedAsync()
    {
        var user = await AuthenticationService.GetCurrentUserAsync();
        if (user != null)
        {
            if (!string.IsNullOrEmpty(Permission))
            {
                _hasPermission = await AuthService.HasPermissionAsync(user.Id, Permission);
            }
            else if (!string.IsNullOrEmpty(Module) && Action.HasValue)
            {
                _hasPermission = await AuthService.HasPermissionAsync(user.Id, Module, Action.Value);
            }
        }
    }
}
```

#### メニュー権限制御
```razor
<!-- NavMenu.razor の一部 -->
<AuthorizedView Permission="Employee.View">
    <MudNavLink Href="/employees" Icon="@Icons.Material.Filled.People">
        社員管理
    </MudNavLink>
</AuthorizedView>

<AuthorizedView Module="Department" Action="PermissionAction.Manage">
    <MudNavLink Href="/departments" Icon="@Icons.Material.Filled.Business">
        部署管理
    </MudNavLink>
</AuthorizedView>

<AuthorizedView Permission="Report.Generate">
    <MudNavLink Href="/reports" Icon="@Icons.Material.Filled.Assessment">
        レポート
    </MudNavLink>
</AuthorizedView>
```

### 5. 事前定義権限設定

#### システム権限定義
```csharp
public static class SystemPermissions
{
    // 社員管理権限
    public const string EmployeeView = "Employee.View";
    public const string EmployeeCreate = "Employee.Create";
    public const string EmployeeUpdate = "Employee.Update";
    public const string EmployeeDelete = "Employee.Delete";
    public const string EmployeeExport = "Employee.Export";
    
    // 部署管理権限
    public const string DepartmentView = "Department.View";
    public const string DepartmentCreate = "Department.Create";
    public const string DepartmentUpdate = "Department.Update";
    public const string DepartmentDelete = "Department.Delete";
    public const string DepartmentManage = "Department.Manage";
    
    // レポート権限
    public const string ReportView = "Report.View";
    public const string ReportGenerate = "Report.Generate";
    public const string ReportExport = "Report.Export";
    public const string ReportManage = "Report.Manage";
    
    // システム管理権限
    public const string SystemManage = "System.Manage";
    public const string UserManage = "User.Manage";
    public const string RoleManage = "Role.Manage";
    public const string AuditLogView = "AuditLog.View";
}
```

#### 初期ロール設定
```csharp
public class RoleInitializationService : IRoleInitializationService
{
    public async Task InitializeDefaultRolesAsync()
    {
        // システム管理者
        await CreateRoleWithPermissionsAsync("SystemAdmin", "システム管理者", new[]
        {
            SystemPermissions.SystemManage,
            SystemPermissions.UserManage,
            SystemPermissions.RoleManage,
            SystemPermissions.EmployeeView,
            SystemPermissions.EmployeeCreate,
            SystemPermissions.EmployeeUpdate,
            SystemPermissions.EmployeeDelete,
            SystemPermissions.EmployeeExport,
            SystemPermissions.DepartmentView,
            SystemPermissions.DepartmentCreate,
            SystemPermissions.DepartmentUpdate,
            SystemPermissions.DepartmentDelete,
            SystemPermissions.DepartmentManage,
            SystemPermissions.ReportView,
            SystemPermissions.ReportGenerate,
            SystemPermissions.ReportExport,
            SystemPermissions.ReportManage,
            SystemPermissions.AuditLogView
        });
        
        // 人事管理者
        await CreateRoleWithPermissionsAsync("HRManager", "人事管理者", new[]
        {
            SystemPermissions.EmployeeView,
            SystemPermissions.EmployeeCreate,
            SystemPermissions.EmployeeUpdate,
            SystemPermissions.EmployeeDelete,
            SystemPermissions.EmployeeExport,
            SystemPermissions.DepartmentView,
            SystemPermissions.DepartmentCreate,
            SystemPermissions.DepartmentUpdate,
            SystemPermissions.ReportView,
            SystemPermissions.ReportGenerate,
            SystemPermissions.ReportExport
        });
        
        // 部門管理者
        await CreateRoleWithPermissionsAsync("DepartmentManager", "部門管理者", new[]
        {
            SystemPermissions.EmployeeView,
            SystemPermissions.EmployeeUpdate,
            SystemPermissions.ReportView,
            SystemPermissions.ReportGenerate
        });
        
        // 一般ユーザー
        await CreateRoleWithPermissionsAsync("User", "一般ユーザー", new[]
        {
            SystemPermissions.EmployeeView,
            SystemPermissions.DepartmentView
        });
        
        // ゲストユーザー
        await CreateRoleWithPermissionsAsync("Guest", "ゲストユーザー", new[]
        {
            SystemPermissions.EmployeeView
        });
    }
}
```

## ファイル変更計画

### 新規作成ファイル

1. **ドメイン層**
   - `Domain/Models/Role.cs`
   - `Domain/Models/Permission.cs`
   - `Domain/Models/RolePermission.cs`
   - `Domain/Models/UserRole.cs`
   - `Domain/Interfaces/IRoleRepository.cs`
   - `Domain/Interfaces/IPermissionRepository.cs`

2. **アプリケーション層**
   - `Application/Interfaces/IAuthorizationService.cs`
   - `Application/Interfaces/IDataAuthorizationService.cs`
   - `Application/Interfaces/IRoleManagementService.cs`
   - `Application/Services/AuthorizationService.cs`
   - `Application/Services/DataAuthorizationService.cs`
   - `Application/Services/RoleManagementService.cs`
   - `Application/Services/RoleInitializationService.cs`

3. **インフラストラクチャー層**
   - `Infrastructure/Repositories/InMemoryRoleRepository.cs`
   - `Infrastructure/Repositories/InMemoryPermissionRepository.cs`
   - `Infrastructure/Authorization/PermissionAuthorizationHandler.cs`
   - `Infrastructure/Authorization/RequirePermissionAttribute.cs`

4. **UI コンポーネント**
   - `Components/Shared/AuthorizedView.razor`
   - `Components/Pages/RoleManagement.razor`
   - `Components/Pages/UserRoleManagement.razor`
   - `Components/Dialogs/RoleEditDialog.razor`
   - `Components/Dialogs/PermissionSelectionDialog.razor`
   - `Components/Dialogs/UserRoleAssignDialog.razor`

5. **定数・設定**
   - `Constants/SystemPermissions.cs`
   - `Constants/SystemRoles.cs`

### 既存ファイル変更

1. **ユーザーモデル拡張**
   - `Domain/Models/User.cs` (ロール関連プロパティ追加)

2. **認証サービス拡張**
   - `Application/Services/AuthenticationService.cs`

3. **全ページ・コンポーネント**
   - 権限チェック統合 (AuthorizedView適用)
   - ボタン・メニューの権限制御

4. **データストア拡張**
   - `Infrastructure/DataStores/ConcurrentInMemoryDataStore.cs`

5. **依存性注入**
   - `Program.cs` (権限関連サービス登録)

6. **レイアウト**
   - `Components/Layout/NavMenu.razor`
   - `Components/Layout/MainLayout.razor`

## 実装手順

### Phase 1: 権限基盤実装（優先度：最高）
1. Role、Permission ドメインモデル
2. 基本的な認可サービス
3. InMemory リポジトリ実装
4. 初期データ設定

### Phase 2: 認可システム統合（優先度：最高）
1. カスタム認可属性実装
2. 権限チェック サービス
3. データフィルタリング機能
4. AuthorizedView コンポーネント

### Phase 3: UI統合（優先度：高）
1. 既存画面への権限制御適用
2. メニュー・ボタンの権限制御
3. エラーハンドリング
4. ユーザビリティ向上

### Phase 4: 管理機能（優先度：中）
1. ロール管理画面
2. ユーザーロール管理
3. 権限テスト機能
4. 権限監査機能

### Phase 5: 高度機能（優先度：低）
1. 動的権限設定
2. 権限継承システム
3. 時限権限機能
4. 権限レポート

## テスト項目

### 単体テスト
- 権限チェック ロジック
- ロール・権限 サービス
- データフィルタリング
- 認可属性動作

### 統合テスト
- 権限制御フロー
- UI 権限制御
- データアクセス制御
- 複数ロール組み合わせ

### セキュリティテスト
- 権限昇格テスト
- 認可バイパステスト
- データ漏洩防止テスト

## セキュリティ考慮事項

### 認可設計原則
- **デフォルト拒否**: 明示的な許可がない限り拒否
- **最小権限の原則**: 必要最小限の権限のみ付与
- **職責分離**: 重要操作の権限分散
- **定期的な権限レビュー**: 権限の適切性確認

### セキュリティ対策
- 権限チェックの漏れ防止
- クライアントサイド制御の補完
- サーバーサイド検証の徹底
- セッション管理の強化

## リスクと対策

### 技術的リスク
- **権限制御の複雑化**: 保守性の低下
- **パフォーマンス影響**: 権限チェックのオーバーヘッド
- **セキュリティホール**: 権限制御の漏れ

### 対策
- 権限設計の簡潔性維持
- 効率的な権限キャッシュ
- 包括的なセキュリティテスト
- 定期的なセキュリティ監査

## 見積もり
- **開発工数**: 5-6日
- **テスト工数**: 3-4日
- **総工数**: 8-10日

## 依存関係
- 監査ログ（権限操作の記録）
- 通知システム（権限変更通知）
- すべての既存機能（権限制御適用）

## 成功指標
- ✅ 権限制御100%実装（漏れなし）
- ✅ 権限チェック レスポンス時間 < 100ms
- ✅ セキュリティテスト合格率100%
- ✅ ユーザビリティ（権限エラー の分かりやすさ）
- ✅ 管理者の権限管理効率性

## 将来拡張
- 動的権限生成
- 外部ID プロバイダー統合
- 機械学習による異常検知
- 権限の時系列分析
- マイクロサービス対応権限管理
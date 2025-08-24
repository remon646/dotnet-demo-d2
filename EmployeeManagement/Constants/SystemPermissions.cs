namespace EmployeeManagement.Constants;

/// <summary>
/// システムで定義されている権限の定数
/// </summary>
public static class SystemPermissions
{
    #region 社員管理権限

    /// <summary>
    /// 社員情報の閲覧権限
    /// </summary>
    public const string EmployeeView = "Employee.View";

    /// <summary>
    /// 社員情報の作成権限
    /// </summary>
    public const string EmployeeCreate = "Employee.Create";

    /// <summary>
    /// 社員情報の更新権限
    /// </summary>
    public const string EmployeeUpdate = "Employee.Update";

    /// <summary>
    /// 社員情報の削除権限
    /// </summary>
    public const string EmployeeDelete = "Employee.Delete";

    /// <summary>
    /// 社員情報のエクスポート権限
    /// </summary>
    public const string EmployeeExport = "Employee.Export";

    /// <summary>
    /// 社員情報のインポート権限
    /// </summary>
    public const string EmployeeImport = "Employee.Import";

    /// <summary>
    /// 社員情報の管理権限（包括的）
    /// </summary>
    public const string EmployeeManage = "Employee.Manage";

    #endregion

    #region 部署管理権限

    /// <summary>
    /// 部署情報の閲覧権限
    /// </summary>
    public const string DepartmentView = "Department.View";

    /// <summary>
    /// 部署情報の作成権限
    /// </summary>
    public const string DepartmentCreate = "Department.Create";

    /// <summary>
    /// 部署情報の更新権限
    /// </summary>
    public const string DepartmentUpdate = "Department.Update";

    /// <summary>
    /// 部署情報の削除権限
    /// </summary>
    public const string DepartmentDelete = "Department.Delete";

    /// <summary>
    /// 部署情報の管理権限（包括的）
    /// </summary>
    public const string DepartmentManage = "Department.Manage";

    #endregion

    #region レポート権限

    /// <summary>
    /// レポートの閲覧権限
    /// </summary>
    public const string ReportView = "Report.View";

    /// <summary>
    /// レポートの生成権限
    /// </summary>
    public const string ReportGenerate = "Report.Generate";

    /// <summary>
    /// レポートのエクスポート権限
    /// </summary>
    public const string ReportExport = "Report.Export";

    /// <summary>
    /// カスタムレポートの作成権限
    /// </summary>
    public const string ReportCreate = "Report.Create";

    /// <summary>
    /// レポートの管理権限（包括的）
    /// </summary>
    public const string ReportManage = "Report.Manage";

    #endregion

    #region ファイル管理権限

    /// <summary>
    /// ファイルのアップロード権限
    /// </summary>
    public const string FileUpload = "File.Upload";

    /// <summary>
    /// ファイルのダウンロード権限
    /// </summary>
    public const string FileDownload = "File.Download";

    /// <summary>
    /// ファイルの削除権限
    /// </summary>
    public const string FileDelete = "File.Delete";

    /// <summary>
    /// ファイルの管理権限（包括的）
    /// </summary>
    public const string FileManage = "File.Manage";

    #endregion

    #region 検索・フィルタリング権限

    /// <summary>
    /// 基本検索権限
    /// </summary>
    public const string SearchBasic = "Search.Basic";

    /// <summary>
    /// 高度検索権限
    /// </summary>
    public const string SearchAdvanced = "Search.Advanced";

    /// <summary>
    /// 検索結果のエクスポート権限
    /// </summary>
    public const string SearchExport = "Search.Export";

    /// <summary>
    /// 保存された検索条件の管理権限
    /// </summary>
    public const string SearchManage = "Search.Manage";

    #endregion

    #region 通知権限

    /// <summary>
    /// 通知の閲覧権限
    /// </summary>
    public const string NotificationView = "Notification.View";

    /// <summary>
    /// システム通知の送信権限
    /// </summary>
    public const string NotificationSend = "Notification.Send";

    /// <summary>
    /// 通知設定の管理権限
    /// </summary>
    public const string NotificationManage = "Notification.Manage";

    #endregion

    #region 監査ログ権限

    /// <summary>
    /// 監査ログの閲覧権限
    /// </summary>
    public const string AuditLogView = "AuditLog.View";

    /// <summary>
    /// 監査ログの検索権限
    /// </summary>
    public const string AuditLogSearch = "AuditLog.Search";

    /// <summary>
    /// 監査ログのエクスポート権限
    /// </summary>
    public const string AuditLogExport = "AuditLog.Export";

    /// <summary>
    /// 監査ログの管理権限（包括的）
    /// </summary>
    public const string AuditLogManage = "AuditLog.Manage";

    #endregion

    #region ユーザー管理権限

    /// <summary>
    /// ユーザー情報の閲覧権限
    /// </summary>
    public const string UserView = "User.View";

    /// <summary>
    /// ユーザー情報の作成権限
    /// </summary>
    public const string UserCreate = "User.Create";

    /// <summary>
    /// ユーザー情報の更新権限
    /// </summary>
    public const string UserUpdate = "User.Update";

    /// <summary>
    /// ユーザー情報の削除権限
    /// </summary>
    public const string UserDelete = "User.Delete";

    /// <summary>
    /// ユーザー情報の管理権限（包括的）
    /// </summary>
    public const string UserManage = "User.Manage";

    #endregion

    #region ロール・権限管理権限

    /// <summary>
    /// ロール情報の閲覧権限
    /// </summary>
    public const string RoleView = "Role.View";

    /// <summary>
    /// ロール情報の作成権限
    /// </summary>
    public const string RoleCreate = "Role.Create";

    /// <summary>
    /// ロール情報の更新権限
    /// </summary>
    public const string RoleUpdate = "Role.Update";

    /// <summary>
    /// ロール情報の削除権限
    /// </summary>
    public const string RoleDelete = "Role.Delete";

    /// <summary>
    /// ロール情報の管理権限（包括的）
    /// </summary>
    public const string RoleManage = "Role.Manage";

    /// <summary>
    /// ユーザーロール割り当て権限
    /// </summary>
    public const string UserRoleAssign = "UserRole.Assign";

    /// <summary>
    /// 権限情報の閲覧権限
    /// </summary>
    public const string PermissionView = "Permission.View";

    /// <summary>
    /// 権限情報の管理権限
    /// </summary>
    public const string PermissionManage = "Permission.Manage";

    #endregion

    #region システム管理権限

    /// <summary>
    /// システム設定の閲覧権限
    /// </summary>
    public const string SystemView = "System.View";

    /// <summary>
    /// システム設定の変更権限
    /// </summary>
    public const string SystemConfigure = "System.Configure";

    /// <summary>
    /// システム管理権限（包括的）
    /// </summary>
    public const string SystemManage = "System.Manage";

    /// <summary>
    /// システムメンテナンス権限
    /// </summary>
    public const string SystemMaintenance = "System.Maintenance";

    #endregion

    #region データベース・バックアップ権限

    /// <summary>
    /// データのバックアップ権限
    /// </summary>
    public const string DataBackup = "Data.Backup";

    /// <summary>
    /// データの復元権限
    /// </summary>
    public const string DataRestore = "Data.Restore";

    /// <summary>
    /// データベース管理権限
    /// </summary>
    public const string DatabaseManage = "Database.Manage";

    #endregion

    /// <summary>
    /// 全権限の一覧を取得
    /// </summary>
    /// <returns>権限名の配列</returns>
    public static string[] GetAllPermissions()
    {
        return new[]
        {
            // 社員管理
            EmployeeView, EmployeeCreate, EmployeeUpdate, EmployeeDelete, 
            EmployeeExport, EmployeeImport, EmployeeManage,
            
            // 部署管理
            DepartmentView, DepartmentCreate, DepartmentUpdate, DepartmentDelete, DepartmentManage,
            
            // レポート
            ReportView, ReportGenerate, ReportExport, ReportCreate, ReportManage,
            
            // ファイル管理
            FileUpload, FileDownload, FileDelete, FileManage,
            
            // 検索・フィルタリング
            SearchBasic, SearchAdvanced, SearchExport, SearchManage,
            
            // 通知
            NotificationView, NotificationSend, NotificationManage,
            
            // 監査ログ
            AuditLogView, AuditLogSearch, AuditLogExport, AuditLogManage,
            
            // ユーザー管理
            UserView, UserCreate, UserUpdate, UserDelete, UserManage,
            
            // ロール・権限管理
            RoleView, RoleCreate, RoleUpdate, RoleDelete, RoleManage,
            UserRoleAssign, PermissionView, PermissionManage,
            
            // システム管理
            SystemView, SystemConfigure, SystemManage, SystemMaintenance,
            
            // データベース・バックアップ
            DataBackup, DataRestore, DatabaseManage
        };
    }

    /// <summary>
    /// モジュール別の権限を取得
    /// </summary>
    /// <param name="module">モジュール名</param>
    /// <returns>そのモジュールの権限一覧</returns>
    public static string[] GetPermissionsByModule(string module)
    {
        return module.ToLower() switch
        {
            "employee" => new[] { EmployeeView, EmployeeCreate, EmployeeUpdate, EmployeeDelete, EmployeeExport, EmployeeImport, EmployeeManage },
            "department" => new[] { DepartmentView, DepartmentCreate, DepartmentUpdate, DepartmentDelete, DepartmentManage },
            "report" => new[] { ReportView, ReportGenerate, ReportExport, ReportCreate, ReportManage },
            "file" => new[] { FileUpload, FileDownload, FileDelete, FileManage },
            "search" => new[] { SearchBasic, SearchAdvanced, SearchExport, SearchManage },
            "notification" => new[] { NotificationView, NotificationSend, NotificationManage },
            "auditlog" => new[] { AuditLogView, AuditLogSearch, AuditLogExport, AuditLogManage },
            "user" => new[] { UserView, UserCreate, UserUpdate, UserDelete, UserManage },
            "role" => new[] { RoleView, RoleCreate, RoleUpdate, RoleDelete, RoleManage, UserRoleAssign },
            "permission" => new[] { PermissionView, PermissionManage },
            "system" => new[] { SystemView, SystemConfigure, SystemManage, SystemMaintenance },
            "data" => new[] { DataBackup, DataRestore, DatabaseManage },
            _ => Array.Empty<string>()
        };
    }

    /// <summary>
    /// 管理者権限を取得
    /// </summary>
    /// <returns>管理者が持つべき権限一覧</returns>
    public static string[] GetAdminPermissions()
    {
        return GetAllPermissions();
    }

    /// <summary>
    /// 人事管理者権限を取得
    /// </summary>
    /// <returns>人事管理者が持つべき権限一覧</returns>
    public static string[] GetHRManagerPermissions()
    {
        return new[]
        {
            // 社員管理（全権限）
            EmployeeView, EmployeeCreate, EmployeeUpdate, EmployeeDelete, EmployeeExport, EmployeeImport,
            
            // 部署管理（削除以外）
            DepartmentView, DepartmentCreate, DepartmentUpdate,
            
            // レポート（生成・エクスポート）
            ReportView, ReportGenerate, ReportExport,
            
            // ファイル管理（基本権限）
            FileUpload, FileDownload,
            
            // 検索（高度検索含む）
            SearchBasic, SearchAdvanced, SearchExport,
            
            // 通知（閲覧）
            NotificationView,
            
            // 監査ログ（閲覧・検索）
            AuditLogView, AuditLogSearch
        };
    }

    /// <summary>
    /// 部門管理者権限を取得
    /// </summary>
    /// <returns>部門管理者が持つべき権限一覧</returns>
    public static string[] GetDepartmentManagerPermissions()
    {
        return new[]
        {
            // 社員管理（閲覧・更新のみ）
            EmployeeView, EmployeeUpdate,
            
            // 部署管理（閲覧のみ）
            DepartmentView,
            
            // レポート（閲覧・生成）
            ReportView, ReportGenerate,
            
            // 検索（基本検索）
            SearchBasic,
            
            // 通知（閲覧）
            NotificationView
        };
    }

    /// <summary>
    /// 一般ユーザー権限を取得
    /// </summary>
    /// <returns>一般ユーザーが持つべき権限一覧</returns>
    public static string[] GetUserPermissions()
    {
        return new[]
        {
            // 社員管理（閲覧のみ）
            EmployeeView,
            
            // 部署管理（閲覧のみ）
            DepartmentView,
            
            // レポート（閲覧のみ）
            ReportView,
            
            // 検索（基本検索）
            SearchBasic,
            
            // 通知（閲覧）
            NotificationView
        };
    }

    /// <summary>
    /// ゲストユーザー権限を取得
    /// </summary>
    /// <returns>ゲストユーザーが持つべき権限一覧</returns>
    public static string[] GetGuestPermissions()
    {
        return new[]
        {
            // 社員管理（閲覧のみ）
            EmployeeView,
            
            // 部署管理（閲覧のみ）
            DepartmentView
        };
    }

    /// <summary>
    /// 権限の説明を取得
    /// </summary>
    /// <param name="permission">権限名</param>
    /// <returns>権限の説明</returns>
    public static string GetPermissionDescription(string permission)
    {
        return permission switch
        {
            EmployeeView => "社員情報を閲覧できます",
            EmployeeCreate => "新しい社員を登録できます",
            EmployeeUpdate => "社員情報を更新できます",
            EmployeeDelete => "社員情報を削除できます",
            EmployeeExport => "社員データをエクスポートできます",
            EmployeeImport => "社員データをインポートできます",
            EmployeeManage => "社員情報を包括的に管理できます",
            
            DepartmentView => "部署情報を閲覧できます",
            DepartmentCreate => "新しい部署を作成できます",
            DepartmentUpdate => "部署情報を更新できます",
            DepartmentDelete => "部署情報を削除できます",
            DepartmentManage => "部署情報を包括的に管理できます",
            
            ReportView => "レポートを閲覧できます",
            ReportGenerate => "レポートを生成できます",
            ReportExport => "レポートをエクスポートできます",
            ReportCreate => "カスタムレポートを作成できます",
            ReportManage => "レポート機能を包括的に管理できます",
            
            FileUpload => "ファイルをアップロードできます",
            FileDownload => "ファイルをダウンロードできます",
            FileDelete => "ファイルを削除できます",
            FileManage => "ファイルを包括的に管理できます",
            
            SearchBasic => "基本検索を実行できます",
            SearchAdvanced => "高度な検索を実行できます",
            SearchExport => "検索結果をエクスポートできます",
            SearchManage => "検索設定を管理できます",
            
            NotificationView => "通知を閲覧できます",
            NotificationSend => "システム通知を送信できます",
            NotificationManage => "通知設定を管理できます",
            
            AuditLogView => "監査ログを閲覧できます",
            AuditLogSearch => "監査ログを検索できます",
            AuditLogExport => "監査ログをエクスポートできます",
            AuditLogManage => "監査ログを包括的に管理できます",
            
            UserView => "ユーザー情報を閲覧できます",
            UserCreate => "新しいユーザーを作成できます",
            UserUpdate => "ユーザー情報を更新できます",
            UserDelete => "ユーザー情報を削除できます",
            UserManage => "ユーザー情報を包括的に管理できます",
            
            RoleView => "ロール情報を閲覧できます",
            RoleCreate => "新しいロールを作成できます",
            RoleUpdate => "ロール情報を更新できます",
            RoleDelete => "ロール情報を削除できます",
            RoleManage => "ロール情報を包括的に管理できます",
            UserRoleAssign => "ユーザーにロールを割り当てできます",
            PermissionView => "権限情報を閲覧できます",
            PermissionManage => "権限情報を管理できます",
            
            SystemView => "システム設定を閲覧できます",
            SystemConfigure => "システム設定を変更できます",
            SystemManage => "システムを包括的に管理できます",
            SystemMaintenance => "システムメンテナンスを実行できます",
            
            DataBackup => "データをバックアップできます",
            DataRestore => "データを復元できます",
            DatabaseManage => "データベースを管理できます",
            
            _ => "不明な権限です"
        };
    }
}
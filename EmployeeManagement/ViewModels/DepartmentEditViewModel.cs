using EmployeeManagement.Domain.Models;
using MudBlazor;

namespace EmployeeManagement.ViewModels;

/// <summary>
/// 部門編集画面のUI状態を管理するViewModel
/// UI表示ロジックとビジネスロジックを分離
/// 画面の状態管理とデータバインディングに特化
/// </summary>
public class DepartmentEditViewModel
{
    #region Core Properties - 部門データ関連
    
    /// <summary>
    /// 現在編集中の部門情報
    /// フォームの入力データとして使用
    /// </summary>
    public DepartmentMaster? CurrentDepartment { get; set; }
    
    /// <summary>
    /// 設立日（画面表示用）
    /// DatePickerコンポーネントとのバインディング用
    /// </summary>
    public DateTime? EstablishedDate { get; set; }
    
    /// <summary>
    /// 選択された責任者社員
    /// オートコンプリートコンポーネントとのバインディング用
    /// </summary>
    public Employee? SelectedManager { get; set; }
    
    #endregion
    
    #region State Management - 画面状態管理
    
    /// <summary>
    /// 読み込み状態フラグ
    /// ローディング表示の制御に使用
    /// </summary>
    public bool IsLoading { get; set; } = true;
    
    /// <summary>
    /// 保存処理実行中フラグ
    /// 保存ボタンの無効化と進行状況表示に使用
    /// </summary>
    public bool IsSaving { get; set; } = false;
    
    /// <summary>
    /// 新規作成モードフラグ
    /// 作成・編集の画面表示切り替えに使用
    /// </summary>
    public bool IsNewDepartment { get; set; } = false;
    
    /// <summary>
    /// ページタイトル
    /// ブラウザタイトルとヘッダー表示に使用
    /// </summary>
    public string PageTitle { get; set; } = string.Empty;
    
    #endregion
    
    #region Validation State - バリデーション状態
    
    /// <summary>
    /// 責任者バリデーションエラー状態
    /// 責任者関連のエラー表示制御に使用
    /// </summary>
    public bool HasManagerValidationError { get; set; } = false;
    
    /// <summary>
    /// 責任者バリデーションエラーメッセージ
    /// エラー詳細の表示に使用
    /// </summary>
    public string ManagerValidationErrorText { get; set; } = string.Empty;
    
    /// <summary>
    /// フォーム全体のバリデーションエラーリスト
    /// 複数エラー表示の制御に使用
    /// </summary>
    public List<string> FormErrors { get; set; } = new();
    
    /// <summary>
    /// フォームにエラーがあるかどうか
    /// 保存ボタンの活性制御に使用
    /// </summary>
    public bool HasFormErrors => FormErrors.Any() || HasManagerValidationError;
    
    #endregion
    
    #region Data Collections - データコレクション
    
    /// <summary>
    /// 全社員一覧（キャッシュ用）
    /// オートコンプリート検索で使用
    /// </summary>
    public List<Employee> AllEmployees { get; set; } = new();
    
    /// <summary>
    /// パンくずナビゲーション項目リスト
    /// ナビゲーション表示に使用
    /// </summary>
    public List<BreadcrumbItem> BreadcrumbItems { get; set; } = new();
    
    #endregion
    
    #region Utility Properties - ユーティリティプロパティ
    
    /// <summary>
    /// 責任者が設定されているかどうか
    /// UI表示の制御に使用
    /// </summary>
    public bool HasManager => 
        CurrentDepartment != null && 
        !string.IsNullOrWhiteSpace(CurrentDepartment.ManagerEmployeeNumber);
    
    /// <summary>
    /// 責任者名表示用プロパティ
    /// 責任者情報の安全な表示に使用
    /// </summary>
    public string ManagerDisplayName => 
        CurrentDepartment?.ManagerName ?? "未設定";
    
    /// <summary>
    /// 責任者社員番号表示用プロパティ
    /// 責任者情報の安全な表示に使用
    /// </summary>
    public string ManagerEmployeeNumber => 
        CurrentDepartment?.ManagerEmployeeNumber ?? string.Empty;
    
    /// <summary>
    /// 部門作成日表示用プロパティ
    /// 作成日の安全な表示に使用（編集時のみ）
    /// </summary>
    public string CreatedAtDisplay => 
        CurrentDepartment?.CreatedAt.ToString("yyyy/MM/dd HH:mm") ?? string.Empty;
    
    /// <summary>
    /// 部門更新日表示用プロパティ
    /// 更新日の安全な表示に使用（編集時のみ）
    /// </summary>
    public string UpdatedAtDisplay => 
        CurrentDepartment?.UpdatedAt.ToString("yyyy/MM/dd HH:mm") ?? string.Empty;
    
    #endregion
    
    #region Initialization Methods - 初期化メソッド
    
    /// <summary>
    /// 新規作成モード用の初期化
    /// 新規部門作成時の初期状態設定
    /// </summary>
    public void InitializeForNewDepartment()
    {
        IsNewDepartment = true;
        PageTitle = "新規部門作成";
        IsLoading = false;
        
        // 新規部門のデフォルト値設定
        CurrentDepartment = new DepartmentMaster
        {
            DepartmentCode = string.Empty,
            DepartmentName = string.Empty,
            IsActive = true,
            EstablishedDate = DateTime.Today,
            DepartmentType = Domain.Enums.Department.Sales // デフォルト値
        };
        
        EstablishedDate = DateTime.Today;
        SelectedManager = null;
        
        // パンくずナビゲーション設定
        BreadcrumbItems = new List<BreadcrumbItem>
        {
            new BreadcrumbItem("ホーム", href: "/", icon: Icons.Material.Filled.Home),
            new BreadcrumbItem("部門マスタ管理", href: "/departments", icon: Icons.Material.Filled.Business),
            new BreadcrumbItem(PageTitle, href: null, disabled: true)
        };
    }
    
    /// <summary>
    /// 編集モード用の初期化
    /// 既存部門編集時の状態設定
    /// </summary>
    /// <param name="department">編集対象の部門</param>
    public void InitializeForEditDepartment(DepartmentMaster department)
    {
        if (department == null)
        {
            throw new ArgumentNullException(nameof(department));
        }
        
        IsNewDepartment = false;
        PageTitle = $"部門編集: {department.DepartmentName}";
        IsLoading = false;
        
        // 部門データの設定
        CurrentDepartment = department;
        EstablishedDate = department.EstablishedDate;
        
        // パンくずナビゲーション設定
        BreadcrumbItems = new List<BreadcrumbItem>
        {
            new BreadcrumbItem("ホーム", href: "/", icon: Icons.Material.Filled.Home),
            new BreadcrumbItem("部門マスタ管理", href: "/departments", icon: Icons.Material.Filled.Business),
            new BreadcrumbItem(PageTitle, href: null, disabled: true)
        };
    }
    
    #endregion
    
    #region Error Management - エラー管理
    
    /// <summary>
    /// フォームエラーをクリア
    /// バリデーション成功時やリセット時に使用
    /// </summary>
    public void ClearFormErrors()
    {
        FormErrors.Clear();
        HasManagerValidationError = false;
        ManagerValidationErrorText = string.Empty;
    }
    
    /// <summary>
    /// フォームエラーを追加
    /// バリデーション失敗時のエラー蓄積に使用
    /// </summary>
    /// <param name="errorMessage">追加するエラーメッセージ</param>
    public void AddFormError(string errorMessage)
    {
        if (!string.IsNullOrWhiteSpace(errorMessage) && !FormErrors.Contains(errorMessage))
        {
            FormErrors.Add(errorMessage);
        }
    }
    
    /// <summary>
    /// 責任者バリデーションエラーを設定
    /// 責任者関連エラーの専用設定
    /// </summary>
    /// <param name="errorMessage">エラーメッセージ</param>
    public void SetManagerValidationError(string errorMessage)
    {
        HasManagerValidationError = true;
        ManagerValidationErrorText = errorMessage;
    }
    
    /// <summary>
    /// 責任者バリデーションエラーをクリア
    /// 責任者関連エラーの専用クリア
    /// </summary>
    public void ClearManagerValidationError()
    {
        HasManagerValidationError = false;
        ManagerValidationErrorText = string.Empty;
    }
    
    #endregion
    
    #region Manager Management - 責任者管理
    
    /// <summary>
    /// 責任者情報をクリア
    /// 責任者削除時の状態リセット
    /// </summary>
    public void ClearManager()
    {
        if (CurrentDepartment != null)
        {
            CurrentDepartment.ManagerEmployeeNumber = string.Empty;
            CurrentDepartment.ManagerName = string.Empty;
        }
        SelectedManager = null;
        ClearManagerValidationError();
    }
    
    /// <summary>
    /// 責任者情報を設定
    /// 責任者選択時の状態更新
    /// </summary>
    /// <param name="employee">選択された社員</param>
    public void SetManager(Employee employee)
    {
        if (employee == null || CurrentDepartment == null)
        {
            return;
        }
        
        CurrentDepartment.ManagerEmployeeNumber = employee.EmployeeNumber;
        CurrentDepartment.ManagerName = employee.Name;
        SelectedManager = employee;
        ClearManagerValidationError();
    }
    
    #endregion
    
    #region Data Synchronization - データ同期
    
    /// <summary>
    /// 設立日をCurrentDepartmentに同期
    /// DatePickerの値変更時に使用
    /// </summary>
    public void SyncEstablishedDate()
    {
        if (CurrentDepartment != null && EstablishedDate.HasValue)
        {
            CurrentDepartment.EstablishedDate = EstablishedDate.Value;
        }
    }
    
    /// <summary>
    /// 保存前のデータ準備
    /// 画面入力値をCurrentDepartmentに反映
    /// </summary>
    public void PrepareForSave()
    {
        if (CurrentDepartment == null)
        {
            return;
        }
        
        // 設立日の同期
        SyncEstablishedDate();
        
        // 新規作成時のタイムスタンプ設定
        if (IsNewDepartment)
        {
            CurrentDepartment.CreatedAt = DateTime.Now;
            CurrentDepartment.UpdatedAt = DateTime.Now;
        }
        else
        {
            CurrentDepartment.UpdatedAt = DateTime.Now;
        }
    }
    
    #endregion
}
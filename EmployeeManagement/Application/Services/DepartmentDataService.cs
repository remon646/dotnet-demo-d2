using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Models;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Application.Services;

/// <summary>
/// 部門データ操作を担当するサービス実装クラス
/// 複数のリポジトリを組み合わせた複合操作を提供
/// UIコンポーネントから複雑なデータ処理を分離
/// </summary>
public class DepartmentDataService : IDepartmentDataService
{
    #region Private Fields
    
    /// <summary>
    /// 部門データアクセス用リポジトリ
    /// 基本的なCRUD操作を提供
    /// </summary>
    private readonly IDepartmentRepository _departmentRepository;
    
    /// <summary>
    /// 社員データアクセス用リポジトリ
    /// 部門削除時の制約チェックに使用
    /// </summary>
    private readonly IEmployeeRepository _employeeRepository;
    
    /// <summary>
    /// 部門バリデーションサービス
    /// データ検証処理を委譲
    /// </summary>
    private readonly IDepartmentValidationService _validationService;
    
    /// <summary>
    /// 責任者バリデーションサービス
    /// 責任者関連の検証処理を委譲
    /// </summary>
    private readonly IManagerValidationService _managerValidationService;
    
    /// <summary>
    /// ログ出力用インスタンス
    /// データ操作の詳細ログとエラー追跡に使用
    /// </summary>
    private readonly ILogger<DepartmentDataService> _logger;
    
    #endregion
    
    #region Constructor
    
    /// <summary>
    /// コンストラクタ - 依存性注入によるインスタンス初期化
    /// </summary>
    /// <param name="departmentRepository">部門リポジトリ</param>
    /// <param name="employeeRepository">社員リポジトリ</param>
    /// <param name="validationService">部門バリデーションサービス</param>
    /// <param name="managerValidationService">責任者バリデーションサービス</param>
    /// <param name="logger">ログ出力インスタンス</param>
    public DepartmentDataService(
        IDepartmentRepository departmentRepository,
        IEmployeeRepository employeeRepository,
        IDepartmentValidationService validationService,
        IManagerValidationService managerValidationService,
        ILogger<DepartmentDataService> logger)
    {
        _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _managerValidationService = managerValidationService ?? throw new ArgumentNullException(nameof(managerValidationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// 部門を作成する（バリデーションと重複チェック含む）
    /// </summary>
    public async Task<ValidationResult> CreateDepartmentAsync(DepartmentMaster department)
    {
        if (department == null)
        {
            _logger.LogWarning("CreateDepartmentAsync: 部門データがnullです");
            return ValidationResult.Failure("部門データが正しく設定されていません。");
        }
        
        _logger.LogInformation("部門作成開始: {DepartmentCode} - {DepartmentName}", 
            department.DepartmentCode, department.DepartmentName);
        
        try
        {
            // 1. 包括的バリデーション実行
            var validationResult = await _validationService.ValidateDepartmentAsync(department, isNewDepartment: true);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("部門作成バリデーション失敗: {DepartmentCode}, エラー数: {ErrorCount}", 
                    department.DepartmentCode, validationResult.ErrorMessages.Count);
                return validationResult;
            }
            
            // 2. 責任者バリデーション（設定されている場合のみ）
            if (!string.IsNullOrWhiteSpace(department.ManagerEmployeeNumber))
            {
                var managerValidation = await _managerValidationService.ValidateManagerAsync(department.ManagerEmployeeNumber);
                if (!managerValidation.IsValid)
                {
                    _logger.LogWarning("責任者バリデーション失敗: {ManagerEmployeeNumber}", 
                        department.ManagerEmployeeNumber);
                    return ValidationResult.Failure(managerValidation.ErrorMessage, "責任者");
                }
                
                // 責任者情報の同期
                if (managerValidation.Employee != null)
                {
                    department.ManagerName = managerValidation.Employee.Name;
                }
            }
            
            // 3. 作成日時の設定
            department.CreatedAt = DateTime.Now;
            department.UpdatedAt = DateTime.Now;
            
            // 4. データベースへの登録
            var success = await _departmentRepository.AddAsync(department);
            if (!success)
            {
                _logger.LogError("部門作成失敗: {DepartmentCode} - データベース登録エラー", 
                    department.DepartmentCode);
                return ValidationResult.Failure("部門の作成に失敗しました。データベースエラーが発生しました。");
            }
            
            // 5. 作成後の整合性確認
            var createdDepartment = await _departmentRepository.GetByIdAsync(department.DepartmentCode);
            if (createdDepartment == null)
            {
                _logger.LogError("部門作成後の確認失敗: {DepartmentCode} - 作成されたデータが見つかりません", 
                    department.DepartmentCode);
                return ValidationResult.Failure("部門は作成されましたが、データの確認に失敗しました。");
            }
            
            _logger.LogInformation("部門作成成功: {DepartmentCode} - {DepartmentName}", 
                department.DepartmentCode, department.DepartmentName);
            
            return ValidationResult.Success($"部門「{department.DepartmentName}」を作成しました。");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "部門作成中にエラーが発生: {DepartmentCode}", department.DepartmentCode);
            return ValidationResult.Failure($"部門作成処理中にエラーが発生しました: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 部門を更新する（バリデーションと整合性チェック含む）
    /// </summary>
    public async Task<ValidationResult> UpdateDepartmentAsync(DepartmentMaster department)
    {
        if (department == null)
        {
            _logger.LogWarning("UpdateDepartmentAsync: 部門データがnullです");
            return ValidationResult.Failure("部門データが正しく設定されていません。");
        }
        
        _logger.LogInformation("部門更新開始: {DepartmentCode} - {DepartmentName}", 
            department.DepartmentCode, department.DepartmentName);
        
        try
        {
            // 1. 更新対象部門の存在確認
            var existingDepartment = await _departmentRepository.GetByIdAsync(department.DepartmentCode);
            if (existingDepartment == null)
            {
                _logger.LogWarning("更新対象部門が見つかりません: {DepartmentCode}", department.DepartmentCode);
                return ValidationResult.Failure("更新対象の部門が見つかりません。");
            }
            
            // 2. 包括的バリデーション実行
            var validationResult = await _validationService.ValidateDepartmentAsync(department, isNewDepartment: false);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("部門更新バリデーション失敗: {DepartmentCode}, エラー数: {ErrorCount}", 
                    department.DepartmentCode, validationResult.ErrorMessages.Count);
                return validationResult;
            }
            
            // 3. 責任者バリデーション（設定されている場合のみ）
            if (!string.IsNullOrWhiteSpace(department.ManagerEmployeeNumber))
            {
                var managerValidation = await _managerValidationService.ValidateManagerAsync(department.ManagerEmployeeNumber);
                if (!managerValidation.IsValid)
                {
                    _logger.LogWarning("責任者バリデーション失敗: {ManagerEmployeeNumber}", 
                        department.ManagerEmployeeNumber);
                    return ValidationResult.Failure(managerValidation.ErrorMessage, "責任者");
                }
                
                // 責任者情報の同期
                if (managerValidation.Employee != null)
                {
                    department.ManagerName = managerValidation.Employee.Name;
                }
            }
            else
            {
                // 責任者がクリアされた場合
                department.ManagerName = string.Empty;
            }
            
            // 4. 更新日時の設定（作成日時は保持）
            department.CreatedAt = existingDepartment.CreatedAt; // 元の作成日時を保持
            department.UpdatedAt = DateTime.Now;
            
            // 5. データベースの更新
            var success = await _departmentRepository.UpdateAsync(department);
            if (!success)
            {
                _logger.LogError("部門更新失敗: {DepartmentCode} - データベース更新エラー", 
                    department.DepartmentCode);
                return ValidationResult.Failure("部門の更新に失敗しました。データベースエラーが発生しました。");
            }
            
            _logger.LogInformation("部門更新成功: {DepartmentCode} - {DepartmentName}", 
                department.DepartmentCode, department.DepartmentName);
            
            return ValidationResult.Success($"部門「{department.DepartmentName}」の情報を更新しました。");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "部門更新中にエラーが発生: {DepartmentCode}", department.DepartmentCode);
            return ValidationResult.Failure($"部門更新処理中にエラーが発生しました: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 部門を削除する（制約チェック含む）
    /// </summary>
    public async Task<ValidationResult> DeleteDepartmentAsync(string departmentCode)
    {
        if (string.IsNullOrWhiteSpace(departmentCode))
        {
            _logger.LogWarning("DeleteDepartmentAsync: 部署コードが空です");
            return ValidationResult.Failure("部署コードが指定されていません。");
        }
        
        _logger.LogInformation("部門削除開始: {DepartmentCode}", departmentCode);
        
        try
        {
            // 1. 削除制約の確認
            var deletionValidation = await _validationService.ValidateDepartmentDeletionAsync(departmentCode);
            if (!deletionValidation.IsValid)
            {
                _logger.LogWarning("部門削除制約違反: {DepartmentCode}, 理由数: {ReasonCount}", 
                    departmentCode, deletionValidation.ErrorMessages.Count);
                return deletionValidation;
            }
            
            // 2. データベースからの削除
            var success = await _departmentRepository.DeleteAsync(departmentCode);
            if (!success)
            {
                _logger.LogError("部門削除失敗: {DepartmentCode} - データベース削除エラー", departmentCode);
                return ValidationResult.Failure("部門の削除に失敗しました。データベースエラーが発生しました。");
            }
            
            _logger.LogInformation("部門削除成功: {DepartmentCode}", departmentCode);
            return ValidationResult.Success($"部門「{departmentCode}」を削除しました。");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "部門削除中にエラーが発生: {DepartmentCode}", departmentCode);
            return ValidationResult.Failure($"部門削除処理中にエラーが発生しました: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 部門データを取得する（エラーハンドリング付き）
    /// </summary>
    public async Task<DepartmentOperationResult> GetDepartmentAsync(string departmentCode)
    {
        if (string.IsNullOrWhiteSpace(departmentCode))
        {
            _logger.LogWarning("GetDepartmentAsync: 部署コードが空です");
            return DepartmentOperationResult.Failure("部署コードが指定されていません。", "取得");
        }
        
        _logger.LogDebug("部門データ取得開始: {DepartmentCode}", departmentCode);
        
        try
        {
            var department = await _departmentRepository.GetByIdAsync(departmentCode);
            if (department == null)
            {
                _logger.LogInformation("部門データが見つかりません: {DepartmentCode}", departmentCode);
                return DepartmentOperationResult.Failure("指定された部門が見つかりません。", "取得");
            }
            
            _logger.LogDebug("部門データ取得成功: {DepartmentCode} - {DepartmentName}", 
                department.DepartmentCode, department.DepartmentName);
            
            return DepartmentOperationResult.Success(department, "取得");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "部門データ取得中にエラーが発生: {DepartmentCode}", departmentCode);
            return DepartmentOperationResult.Failure($"部門データ取得中にエラーが発生しました: {ex.Message}", "取得");
        }
    }
    
    /// <summary>
    /// 全部門データを取得する（エラーハンドリング付き）
    /// </summary>
    public async Task<DepartmentListOperationResult> GetAllDepartmentsAsync()
    {
        _logger.LogDebug("全部門データ取得開始");
        
        try
        {
            var departments = await _departmentRepository.GetAllAsync();
            var departmentList = departments.ToList();
            
            _logger.LogInformation("全部門データ取得成功: {Count}件", departmentList.Count);
            return DepartmentListOperationResult.Success(departmentList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "全部門データ取得中にエラーが発生");
            return DepartmentListOperationResult.Failure($"全部門データ取得中にエラーが発生しました: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 部門データの整合性チェックを実行
    /// </summary>
    public async Task<ValidationResult> ValidateDataIntegrityAsync(string? departmentCode = null)
    {
        _logger.LogInformation("部門データ整合性チェック開始: {Target}", 
            departmentCode ?? "全部門");
        
        var errors = new List<string>();
        
        try
        {
            IEnumerable<DepartmentMaster> targetDepartments;
            
            // チェック対象部門の特定
            if (!string.IsNullOrWhiteSpace(departmentCode))
            {
                var department = await _departmentRepository.GetByIdAsync(departmentCode);
                targetDepartments = department != null ? new[] { department } : Enumerable.Empty<DepartmentMaster>();
            }
            else
            {
                targetDepartments = await _departmentRepository.GetAllAsync();
            }
            
            // 各部門の整合性チェック
            foreach (var department in targetDepartments)
            {
                await CheckDepartmentIntegrity(department, errors);
            }
            
            if (errors.Any())
            {
                _logger.LogWarning("部門データ整合性エラー検出: {ErrorCount}件", errors.Count);
                return ValidationResult.Failure(errors, "整合性チェック");
            }
            
            _logger.LogInformation("部門データ整合性チェック完了: 問題なし");
            return ValidationResult.Success("部門データの整合性に問題ありません。", "整合性チェック");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "部門データ整合性チェック中にエラーが発生");
            return ValidationResult.Failure($"整合性チェック中にエラーが発生しました: {ex.Message}");
        }
    }
    
    #endregion
    
    #region Private Helper Methods
    
    /// <summary>
    /// 個別部門の整合性チェック
    /// </summary>
    /// <param name="department">チェック対象部門</param>
    /// <param name="errors">エラーリスト</param>
    private async Task CheckDepartmentIntegrity(DepartmentMaster department, List<string> errors)
    {
        // 1. 基本データの整合性
        if (string.IsNullOrWhiteSpace(department.DepartmentCode))
        {
            errors.Add("部署コードが空の部門が存在します。");
        }
        
        if (string.IsNullOrWhiteSpace(department.DepartmentName))
        {
            errors.Add($"部門「{department.DepartmentCode}」: 部署名が空です。");
        }
        
        // 2. 責任者情報の整合性
        if (!string.IsNullOrWhiteSpace(department.ManagerEmployeeNumber))
        {
            try
            {
                var manager = await _employeeRepository.GetByEmployeeNumberAsync(department.ManagerEmployeeNumber);
                if (manager == null)
                {
                    errors.Add($"部門「{department.DepartmentCode}」: 責任者社員番号「{department.ManagerEmployeeNumber}」の社員が見つかりません。");
                }
                else if (string.IsNullOrWhiteSpace(department.ManagerName))
                {
                    errors.Add($"部門「{department.DepartmentCode}」: 責任者社員番号は設定されていますが、責任者名が空です。");
                }
                else if (!department.ManagerName.Equals(manager.Name, StringComparison.Ordinal))
                {
                    errors.Add($"部門「{department.DepartmentCode}」: 責任者名「{department.ManagerName}」が実際の社員名「{manager.Name}」と一致しません。");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"部門「{department.DepartmentCode}」: 責任者情報の確認中にエラーが発生しました: {ex.Message}");
            }
        }
        else if (!string.IsNullOrWhiteSpace(department.ManagerName))
        {
            errors.Add($"部門「{department.DepartmentCode}」: 責任者社員番号が空ですが、責任者名が設定されています。");
        }
        
        // 3. 日付データの整合性
        if (department.CreatedAt > DateTime.Now.AddMinutes(1)) // 1分の誤差は許容
        {
            errors.Add($"部門「{department.DepartmentCode}」: 作成日時が未来日になっています。");
        }
        
        if (department.UpdatedAt < department.CreatedAt)
        {
            errors.Add($"部門「{department.DepartmentCode}」: 更新日時が作成日時より古くなっています。");
        }
    }
    
    #endregion
}
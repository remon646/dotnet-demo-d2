using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Constants;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace EmployeeManagement.Application.Services;

/// <summary>
/// 責任者バリデーション処理を担当するサービス実装クラス
/// UIコンポーネントからビジネスロジックを分離
/// 責任者設定に関する全てのバリデーションロジックを集約実装
/// </summary>
public class ManagerValidationService : IManagerValidationService
{
    #region Private Fields
    
    /// <summary>
    /// 社員データアクセス用リポジトリ
    /// 社員の存在確認と詳細情報取得に使用
    /// </summary>
    private readonly IEmployeeRepository _employeeRepository;
    
    /// <summary>
    /// 部門データアクセス用リポジトリ
    /// 他部門での責任者重複確認に使用
    /// </summary>
    private readonly IDepartmentRepository _departmentRepository;
    
    /// <summary>
    /// ログ出力用インスタンス
    /// バリデーション処理の詳細ログとエラー追跡に使用
    /// </summary>
    private readonly ILogger<ManagerValidationService> _logger;
    
    #endregion
    
    #region Constructor
    
    /// <summary>
    /// コンストラクタ - 依存性注入によるインスタンス初期化
    /// </summary>
    /// <param name="employeeRepository">社員リポジトリ</param>
    /// <param name="departmentRepository">部門リポジトリ</param>
    /// <param name="logger">ログ出力インスタンス</param>
    public ManagerValidationService(
        IEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository,
        ILogger<ManagerValidationService> logger)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// 責任者の社員番号を検証し詳細な結果を返却
    /// </summary>
    public async Task<ManagerValidationResult> ValidateManagerAsync(string? employeeNumber)
    {
        _logger.LogDebug("責任者バリデーション開始: {EmployeeNumber}", employeeNumber ?? "null");
        
        try
        {
            // 1. 空文字・null チェック（責任者未設定は有効）
            if (string.IsNullOrWhiteSpace(employeeNumber))
            {
                _logger.LogDebug("責任者未設定のため有効と判定");
                return ManagerValidationResult.Empty();
            }
            
            // 2. 社員番号形式チェック
            var formatValidation = ValidateEmployeeNumberFormat(employeeNumber);
            if (!formatValidation.IsValid)
            {
                _logger.LogWarning("社員番号形式不正: {EmployeeNumber}", employeeNumber);
                return ManagerValidationResult.Failure(string.Join(", ", formatValidation.ErrorMessages));
            }
            
            // 3. 社員の存在確認
            var employee = await _employeeRepository.GetByEmployeeNumberAsync(employeeNumber);
            if (employee == null)
            {
                _logger.LogWarning("指定された社員が見つかりません: {EmployeeNumber}", employeeNumber);
                return ManagerValidationResult.Failure("指定された社員番号の社員が見つかりません。");
            }
            
            // 4. 社員の有効性チェック
            var eligibilityValidation = ValidateEmployeeEligibility(employee);
            if (!eligibilityValidation.IsValid)
            {
                _logger.LogWarning("社員が責任者として不適格: {EmployeeNumber}, 理由: {Reason}", 
                    employeeNumber, string.Join(", ", eligibilityValidation.ErrorMessages));
                return ManagerValidationResult.Failure(string.Join(", ", eligibilityValidation.ErrorMessages));
            }
            
            // 5. 他部門での責任者重複チェック（警告レベル）
            var isDuplicateManager = await IsManagerInOtherDepartmentAsync(employeeNumber);
            if (isDuplicateManager)
            {
                _logger.LogInformation("他部門で既に責任者の社員: {EmployeeNumber}", employeeNumber);
                return ManagerValidationResult.SuccessWithWarning(
                    employee, 
                    "この社員は他の部門でも責任者として設定されています。",
                    $"責任者を設定しました: {employee.Name} ({employee.EmployeeNumber})"
                );
            }
            
            // 6. バリデーション成功
            _logger.LogInformation("責任者バリデーション成功: {EmployeeNumber} - {EmployeeName}", 
                employeeNumber, employee.Name);
            return ManagerValidationResult.Success(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "責任者バリデーション中にエラーが発生: {EmployeeNumber}", employeeNumber);
            return ManagerValidationResult.Failure($"責任者検証中にエラーが発生しました: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 責任者設定をクリアする際のバリデーション
    /// </summary>
    public async Task<ManagerValidationResult> ValidateClearManagerAsync()
    {
        _logger.LogDebug("責任者クリアバリデーション開始");
        
        try
        {
            // 現在は制約なしでクリアを許可
            // 将来的に「必須責任者部門」制約がある場合はここで判定
            
            _logger.LogInformation("責任者クリアバリデーション成功");
            return ManagerValidationResult.Empty();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "責任者クリアバリデーション中にエラーが発生");
            return ManagerValidationResult.Failure($"責任者クリア検証中にエラーが発生しました: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 責任者として設定可能な社員の一覧を取得
    /// </summary>
    public async Task<IEnumerable<Employee>> GetEligibleManagersAsync()
    {
        _logger.LogDebug("責任者候補社員一覧取得開始");
        
        try
        {
            // 全社員を取得し、責任者として適格な社員のみフィルタリング
            var allEmployees = await _employeeRepository.GetAllAsync();
            
            var eligibleManagers = allEmployees.Where(employee =>
            {
                // 有効性チェック
                var validation = ValidateEmployeeEligibility(employee);
                return validation.IsValid;
            }).ToList();
            
            _logger.LogInformation("責任者候補社員一覧取得完了: {Count}件", eligibleManagers.Count);
            return eligibleManagers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "責任者候補社員一覧取得中にエラーが発生");
            return Enumerable.Empty<Employee>();
        }
    }
    
    /// <summary>
    /// 指定した社員が他部門で責任者になっているかチェック
    /// </summary>
    public async Task<bool> IsManagerInOtherDepartmentAsync(string employeeNumber, string? excludeDepartmentCode = null)
    {
        if (string.IsNullOrWhiteSpace(employeeNumber))
        {
            return false;
        }
        
        _logger.LogDebug("他部門責任者重複チェック開始: {EmployeeNumber}", employeeNumber);
        
        try
        {
            // 全部門を取得
            var allDepartments = await _departmentRepository.GetAllAsync();
            
            // 指定社員が責任者になっている部門をチェック
            var managerDepartments = allDepartments.Where(dept =>
                !string.IsNullOrWhiteSpace(dept.ManagerEmployeeNumber) &&
                dept.ManagerEmployeeNumber.Equals(employeeNumber, StringComparison.OrdinalIgnoreCase) &&
                (excludeDepartmentCode == null || !dept.DepartmentCode.Equals(excludeDepartmentCode, StringComparison.OrdinalIgnoreCase))
            ).ToList();
            
            var isDuplicate = managerDepartments.Any();
            
            if (isDuplicate)
            {
                _logger.LogInformation("他部門責任者重複検出: {EmployeeNumber}, 部門数: {Count}", 
                    employeeNumber, managerDepartments.Count);
            }
            
            return isDuplicate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "他部門責任者重複チェック中にエラーが発生: {EmployeeNumber}", employeeNumber);
            // エラー時は安全側に倒して重複なしとして扱う
            return false;
        }
    }
    
    #endregion
    
    #region Private Helper Methods
    
    /// <summary>
    /// 社員番号の形式を検証
    /// </summary>
    /// <param name="employeeNumber">検証する社員番号</param>
    /// <returns>形式チェック結果</returns>
    private ValidationResult ValidateEmployeeNumberFormat(string employeeNumber)
    {
        // 長さチェック
        if (employeeNumber.Length < ValidationConstants.EMPLOYEE_NUMBER_MIN_LENGTH)
        {
            return ValidationResult.Failure(
                string.Format(ValidationConstants.MIN_LENGTH_ERROR, 
                    "社員番号", 
                    ValidationConstants.EMPLOYEE_NUMBER_MIN_LENGTH));
        }
        
        if (employeeNumber.Length > DepartmentEditConstants.EMPLOYEE_NUMBER_MAX_LENGTH)
        {
            return ValidationResult.Failure($"社員番号は{DepartmentEditConstants.EMPLOYEE_NUMBER_MAX_LENGTH}文字以内で入力してください。");
        }
        
        // 形式チェック（英数字のみ）
        if (!Regex.IsMatch(employeeNumber, ValidationConstants.EMPLOYEE_NUMBER_PATTERN))
        {
            return ValidationResult.Failure(
                string.Format(ValidationConstants.FORMAT_ERROR, "社員番号"));
        }
        
        return ValidationResult.Success();
    }
    
    /// <summary>
    /// 社員が責任者として適格かどうかを検証
    /// </summary>
    /// <param name="employee">検証対象の社員</param>
    /// <returns>適格性チェック結果</returns>
    private ValidationResult ValidateEmployeeEligibility(Employee employee)
    {
        // null チェック
        if (employee == null)
        {
            return ValidationResult.Failure("社員情報が取得できません。");
        }
        
        // 将来的な拡張ポイント:
        // - 在職状況チェック
        // - 役職レベルチェック
        // - 部門制約チェック
        // など
        
        // 現在は基本的に全社員を責任者として適格とする
        return ValidationResult.Success();
    }
    
    #endregion
}
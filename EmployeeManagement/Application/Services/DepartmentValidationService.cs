using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Constants;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace EmployeeManagement.Application.Services;

/// <summary>
/// 部門バリデーション処理を担当するサービス実装クラス
/// UIコンポーネントからビジネスロジックを分離
/// 部門マスタに関する全てのバリデーションロジックを集約実装
/// </summary>
public class DepartmentValidationService : IDepartmentValidationService
{
    #region Private Fields
    
    /// <summary>
    /// 部門データアクセス用リポジトリ
    /// 重複チェックや削除可能性確認に使用
    /// </summary>
    private readonly IDepartmentRepository _departmentRepository;
    
    /// <summary>
    /// 社員データアクセス用リポジトリ
    /// 部門削除時の所属社員確認に使用
    /// </summary>
    private readonly IEmployeeRepository _employeeRepository;
    
    /// <summary>
    /// ログ出力用インスタンス
    /// バリデーション処理の詳細ログとエラー追跡に使用
    /// </summary>
    private readonly ILogger<DepartmentValidationService> _logger;
    
    #endregion
    
    #region Constructor
    
    /// <summary>
    /// コンストラクタ - 依存性注入によるインスタンス初期化
    /// </summary>
    /// <param name="departmentRepository">部門リポジトリ</param>
    /// <param name="employeeRepository">社員リポジトリ</param>
    /// <param name="logger">ログ出力インスタンス</param>
    public DepartmentValidationService(
        IDepartmentRepository departmentRepository,
        IEmployeeRepository employeeRepository,
        ILogger<DepartmentValidationService> logger)
    {
        _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// 部門マスタの包括的バリデーションを実行
    /// </summary>
    public async Task<ValidationResult> ValidateDepartmentAsync(DepartmentMaster department, bool isNewDepartment)
    {
        _logger.LogDebug("部門バリデーション開始: {DepartmentCode} (新規: {IsNew})", 
            department?.DepartmentCode ?? "null", isNewDepartment);
        
        var errors = new List<string>();
        
        try
        {
            // null チェック
            if (department == null)
            {
                _logger.LogWarning("バリデーション対象の部門がnullです");
                return ValidationResult.Failure("部門情報が正しく設定されていません。");
            }
            
            // 1. 必須項目チェック
            ValidateRequiredFields(department, errors);
            
            // 2. 部署コード形式チェック
            if (!string.IsNullOrWhiteSpace(department.DepartmentCode))
            {
                var codeValidation = ValidateDepartmentCodeFormat(department.DepartmentCode);
                if (!codeValidation.IsValid)
                {
                    errors.AddRange(codeValidation.ErrorMessages);
                }
            }
            
            // 3. 部署名形式チェック
            if (!string.IsNullOrWhiteSpace(department.DepartmentName))
            {
                var nameValidation = ValidateDepartmentNameFormat(department.DepartmentName);
                if (!nameValidation.IsValid)
                {
                    errors.AddRange(nameValidation.ErrorMessages);
                }
            }
            
            // 4. 設立日チェック
            var dateValidation = ValidateEstablishedDate(department.EstablishedDate);
            if (!dateValidation.IsValid)
            {
                errors.AddRange(dateValidation.ErrorMessages);
            }
            
            // 5. 新規作成時の重複チェック
            if (isNewDepartment && !string.IsNullOrWhiteSpace(department.DepartmentCode))
            {
                var isDuplicate = await IsDepartmentCodeDuplicateAsync(department.DepartmentCode);
                if (isDuplicate)
                {
                    errors.Add($"部署コード「{department.DepartmentCode}」は既に使用されています。");
                }
            }
            
            // 6. 結果判定
            if (errors.Any())
            {
                _logger.LogWarning("部門バリデーション失敗: {DepartmentCode}, エラー数: {ErrorCount}", 
                    department.DepartmentCode, errors.Count);
                return ValidationResult.Failure(errors);
            }
            
            _logger.LogInformation("部門バリデーション成功: {DepartmentCode} - {DepartmentName}", 
                department.DepartmentCode, department.DepartmentName);
            return ValidationResult.Success("部門情報は有効です。");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "部門バリデーション中にエラーが発生: {DepartmentCode}", 
                department?.DepartmentCode);
            return ValidationResult.Failure($"バリデーション処理中にエラーが発生しました: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 部署コードの重複チェックを実行
    /// </summary>
    public async Task<bool> IsDepartmentCodeDuplicateAsync(string departmentCode, string? excludeCurrentDepartment = null)
    {
        if (string.IsNullOrWhiteSpace(departmentCode))
        {
            return false;
        }
        
        _logger.LogDebug("部署コード重複チェック開始: {DepartmentCode}", departmentCode);
        
        try
        {
            var existingDepartment = await _departmentRepository.GetByIdAsync(departmentCode);
            
            // 部門が存在しない場合は重複なし
            if (existingDepartment == null)
            {
                return false;
            }
            
            // 除外対象の場合は重複なし（編集時に自分自身を除外）
            if (!string.IsNullOrWhiteSpace(excludeCurrentDepartment) &&
                existingDepartment.DepartmentCode.Equals(excludeCurrentDepartment, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            
            _logger.LogInformation("部署コード重複検出: {DepartmentCode}", departmentCode);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "部署コード重複チェック中にエラーが発生: {DepartmentCode}", departmentCode);
            // エラー時は安全側に倒して重複ありとして扱う
            return true;
        }
    }
    
    /// <summary>
    /// 部署コードの形式バリデーション
    /// </summary>
    public ValidationResult ValidateDepartmentCodeFormat(string? departmentCode)
    {
        var errors = new List<string>();
        
        // null・空文字チェック
        if (string.IsNullOrWhiteSpace(departmentCode))
        {
            errors.Add(string.Format(ValidationConstants.REQUIRED_FIELD_ERROR, "部署コード"));
            return ValidationResult.Failure(errors, "部署コード");
        }
        
        // 長さチェック（最小）
        if (departmentCode.Length < ValidationConstants.DEPARTMENT_CODE_MIN_LENGTH)
        {
            errors.Add(string.Format(ValidationConstants.MIN_LENGTH_ERROR, 
                "部署コード", ValidationConstants.DEPARTMENT_CODE_MIN_LENGTH));
        }
        
        // 長さチェック（最大）
        if (departmentCode.Length > DepartmentEditConstants.DEPARTMENT_CODE_MAX_LENGTH)
        {
            errors.Add($"部署コードは{DepartmentEditConstants.DEPARTMENT_CODE_MAX_LENGTH}文字以内で入力してください。");
        }
        
        // 形式チェック（英数字のみ）
        if (!Regex.IsMatch(departmentCode, ValidationConstants.DEPARTMENT_CODE_PATTERN))
        {
            errors.Add(string.Format(ValidationConstants.FORMAT_ERROR, "部署コード") + "英数字のみ使用できます。");
        }
        
        return errors.Any() 
            ? ValidationResult.Failure(errors, "部署コード")
            : ValidationResult.Success("部署コードの形式は有効です。", "部署コード");
    }
    
    /// <summary>
    /// 部署名の妥当性バリデーション
    /// </summary>
    public ValidationResult ValidateDepartmentNameFormat(string? departmentName)
    {
        var errors = new List<string>();
        
        // null・空文字チェック
        if (string.IsNullOrWhiteSpace(departmentName))
        {
            errors.Add(string.Format(ValidationConstants.REQUIRED_FIELD_ERROR, "部署名"));
            return ValidationResult.Failure(errors, "部署名");
        }
        
        // 長さチェック（最小）
        if (departmentName.Length < ValidationConstants.DEPARTMENT_NAME_MIN_LENGTH)
        {
            errors.Add(string.Format(ValidationConstants.MIN_LENGTH_ERROR, 
                "部署名", ValidationConstants.DEPARTMENT_NAME_MIN_LENGTH));
        }
        
        // 長さチェック（最大）
        if (departmentName.Length > DepartmentEditConstants.DEPARTMENT_NAME_MAX_LENGTH)
        {
            errors.Add($"部署名は{DepartmentEditConstants.DEPARTMENT_NAME_MAX_LENGTH}文字以内で入力してください。");
        }
        
        return errors.Any() 
            ? ValidationResult.Failure(errors, "部署名")
            : ValidationResult.Success("部署名の形式は有効です。", "部署名");
    }
    
    /// <summary>
    /// 設立日の妥当性バリデーション
    /// </summary>
    public ValidationResult ValidateEstablishedDate(DateTime? establishedDate)
    {
        var errors = new List<string>();
        
        // null チェック（設立日は任意項目）
        if (!establishedDate.HasValue)
        {
            return ValidationResult.Success("設立日は未設定です。", "設立日");
        }
        
        var today = DateTime.Today;
        var date = establishedDate.Value.Date;
        
        // 未来日チェック
        if (date > today)
        {
            errors.Add(string.Format(ValidationConstants.FUTURE_DATE_ERROR, "設立日"));
        }
        
        // 極端な過去日チェック（200年以上前）
        var minDate = today.AddYears(-ValidationConstants.ESTABLISHED_DATE_MAX_PAST_YEARS);
        if (date < minDate)
        {
            errors.Add($"設立日は{ValidationConstants.ESTABLISHED_DATE_MAX_PAST_YEARS}年以内で入力してください。");
        }
        
        return errors.Any() 
            ? ValidationResult.Failure(errors, "設立日")
            : ValidationResult.Success("設立日は有効です。", "設立日");
    }
    
    /// <summary>
    /// 部門削除可能性のチェック
    /// </summary>
    public async Task<ValidationResult> ValidateDepartmentDeletionAsync(string departmentCode)
    {
        if (string.IsNullOrWhiteSpace(departmentCode))
        {
            return ValidationResult.Failure("部署コードが指定されていません。");
        }
        
        _logger.LogDebug("部門削除可能性チェック開始: {DepartmentCode}", departmentCode);
        
        var errors = new List<string>();
        
        try
        {
            // 1. 部門の存在確認
            var department = await _departmentRepository.GetByIdAsync(departmentCode);
            if (department == null)
            {
                return ValidationResult.Failure("指定された部門が見つかりません。");
            }
            
            // 2. 所属社員の存在確認
            // TODO: DepartmentHistoryとDepartmentMasterの関連付けが必要
            // 現在のモックデータベース構造では部門コードでの検索が困難
            // 将来的にデータベーススキーマの改善が必要
            /*
            var allEmployees = await _employeeRepository.GetAllAsync();
            var departmentEmployees = allEmployees.Where(emp => 
                emp.CurrentDepartmentHistory?.Department.ToString() == departmentCode // 仮実装
            ).ToList();
            
            if (departmentEmployees.Any())
            {
                errors.Add($"この部門には{departmentEmployees.Count}名の社員が所属しているため削除できません。");
            }
            */
            
            // 3. 他部門の責任者として設定されている場合のチェック
            // （将来実装予定の機能）
            
            if (errors.Any())
            {
                _logger.LogWarning("部門削除不可: {DepartmentCode}, 理由数: {ReasonCount}", 
                    departmentCode, errors.Count);
                return ValidationResult.Failure(errors, "部門削除");
            }
            
            _logger.LogInformation("部門削除可能: {DepartmentCode}", departmentCode);
            return ValidationResult.Success("部門は削除可能です。", "部門削除");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "部門削除可能性チェック中にエラーが発生: {DepartmentCode}", departmentCode);
            return ValidationResult.Failure($"削除可能性チェック中にエラーが発生しました: {ex.Message}");
        }
    }
    
    #endregion
    
    #region Private Helper Methods
    
    /// <summary>
    /// 必須項目のバリデーション
    /// </summary>
    /// <param name="department">検証対象の部門</param>
    /// <param name="errors">エラーメッセージリスト</param>
    private void ValidateRequiredFields(DepartmentMaster department, List<string> errors)
    {
        // 部署コード必須チェック
        if (string.IsNullOrWhiteSpace(department.DepartmentCode))
        {
            errors.Add(string.Format(ValidationConstants.REQUIRED_FIELD_ERROR, "部署コード"));
        }
        
        // 部署名必須チェック
        if (string.IsNullOrWhiteSpace(department.DepartmentName))
        {
            errors.Add(string.Format(ValidationConstants.REQUIRED_FIELD_ERROR, "部署名"));
        }
        
        // 部署区分必須チェック（Enumは非nullable型なのでチェック不要）
        // if (department.DepartmentType == null) // Enumは非nullable型のためチェック不要
    }
    
    #endregion
}
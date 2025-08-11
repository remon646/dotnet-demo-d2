using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Models;

/// <summary>
/// 責任者バリデーション結果を格納するクラス
/// 責任者設定処理に特化したバリデーション結果
/// UI表示とビジネスロジックの疎結合を実現
/// </summary>
public class ManagerValidationResult
{
    /// <summary>
    /// バリデーション成功フラグ
    /// true: 責任者設定有効, false: 責任者設定無効
    /// </summary>
    public bool IsValid { get; init; }
    
    /// <summary>
    /// 検証された社員情報
    /// バリデーション成功時のみ設定される
    /// 責任者として設定可能な社員の詳細情報
    /// </summary>
    public Employee? Employee { get; init; }
    
    /// <summary>
    /// エラーメッセージ
    /// バリデーション失敗時の詳細情報
    /// ユーザーへのフィードバックとして使用
    /// </summary>
    public string ErrorMessage { get; init; } = string.Empty;
    
    /// <summary>
    /// 警告メッセージ
    /// エラーではないが注意が必要な情報
    /// 例: 既に他部門の責任者になっている場合の警告
    /// </summary>
    public string WarningMessage { get; init; } = string.Empty;
    
    /// <summary>
    /// 成功時のメッセージ
    /// 責任者設定成功時のユーザー向けフィードバック
    /// </summary>
    public string SuccessMessage { get; init; } = string.Empty;
    
    /// <summary>
    /// 責任者が未設定かどうか
    /// 空文字や null の場合に true となる
    /// </summary>
    public bool IsEmpty { get; init; }
    
    #region Static Factory Methods
    
    /// <summary>
    /// 責任者未設定（有効）の結果を生成
    /// 責任者の設定が任意の場合に使用
    /// </summary>
    /// <returns>未設定を示すManagerValidationResult</returns>
    public static ManagerValidationResult Empty()
    {
        return new ManagerValidationResult
        {
            IsValid = true,
            IsEmpty = true,
            SuccessMessage = "責任者は設定されていません"
        };
    }
    
    /// <summary>
    /// 責任者設定成功の結果を生成
    /// 有効な社員が責任者として設定された場合
    /// </summary>
    /// <param name="employee">責任者として設定された社員</param>
    /// <param name="successMessage">成功メッセージ（任意）</param>
    /// <returns>成功を示すManagerValidationResult</returns>
    public static ManagerValidationResult Success(Employee employee, string? successMessage = null)
    {
        var message = successMessage ?? $"責任者を設定しました: {employee.Name} ({employee.EmployeeNumber})";
        
        return new ManagerValidationResult
        {
            IsValid = true,
            Employee = employee,
            IsEmpty = false,
            SuccessMessage = message
        };
    }
    
    /// <summary>
    /// 責任者設定失敗の結果を生成
    /// 無効な社員番号や存在しない社員の場合
    /// </summary>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <returns>失敗を示すManagerValidationResult</returns>
    public static ManagerValidationResult Failure(string errorMessage)
    {
        return new ManagerValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage,
            IsEmpty = false
        };
    }
    
    /// <summary>
    /// 警告付きの責任者設定成功結果を生成
    /// 設定は可能だが注意点がある場合
    /// </summary>
    /// <param name="employee">責任者として設定された社員</param>
    /// <param name="warningMessage">警告メッセージ</param>
    /// <param name="successMessage">成功メッセージ（任意）</param>
    /// <returns>警告付き成功を示すManagerValidationResult</returns>
    public static ManagerValidationResult SuccessWithWarning(Employee employee, string warningMessage, string? successMessage = null)
    {
        var message = successMessage ?? $"責任者を設定しました: {employee.Name} ({employee.EmployeeNumber})";
        
        return new ManagerValidationResult
        {
            IsValid = true,
            Employee = employee,
            IsEmpty = false,
            SuccessMessage = message,
            WarningMessage = warningMessage
        };
    }
    
    #endregion
    
    #region Utility Properties
    
    /// <summary>
    /// 責任者名を取得
    /// Employeeオブジェクトから名前を安全に取得
    /// </summary>
    public string ManagerName => Employee?.Name ?? string.Empty;
    
    /// <summary>
    /// 責任者社員番号を取得
    /// Employeeオブジェクトから社員番号を安全に取得
    /// </summary>
    public string ManagerEmployeeNumber => Employee?.EmployeeNumber ?? string.Empty;
    
    /// <summary>
    /// エラーメッセージの有無を確認
    /// UI表示でエラー状態を判定する際に使用
    /// </summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    
    /// <summary>
    /// 警告メッセージの有無を確認
    /// UI表示で警告状態を判定する際に使用
    /// </summary>
    public bool HasWarning => !string.IsNullOrWhiteSpace(WarningMessage);
    
    /// <summary>
    /// 成功メッセージの有無を確認
    /// UI表示で成功状態を判定する際に使用
    /// </summary>
    public bool HasSuccess => !string.IsNullOrWhiteSpace(SuccessMessage);
    
    #endregion
}
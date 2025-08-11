namespace EmployeeManagement.Models;

/// <summary>
/// 汎用バリデーション結果を格納するクラス
/// UI表示とビジネスロジックの疎結合を実現
/// 各種バリデーション処理の統一的な結果表現
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// バリデーション成功フラグ
    /// true: バリデーション成功, false: バリデーション失敗
    /// </summary>
    public bool IsValid { get; init; }
    
    /// <summary>
    /// エラーメッセージリスト
    /// バリデーション失敗時の詳細情報を格納
    /// 複数のエラーを同時に返却可能
    /// </summary>
    public List<string> ErrorMessages { get; init; } = new();
    
    /// <summary>
    /// 警告メッセージリスト
    /// エラーではないが注意が必要な情報
    /// ユーザーへの補足情報として利用
    /// </summary>
    public List<string> WarningMessages { get; init; } = new();
    
    /// <summary>
    /// 成功時のメッセージ
    /// バリデーション成功時のユーザー向けフィードバック
    /// </summary>
    public string? SuccessMessage { get; init; }
    
    /// <summary>
    /// バリデーション対象のフィールド名
    /// エラー特定とUI表示で使用
    /// </summary>
    public string? FieldName { get; init; }
    
    #region Static Factory Methods
    
    /// <summary>
    /// 成功結果を生成する静的ファクトリーメソッド
    /// </summary>
    /// <param name="successMessage">成功時のメッセージ（任意）</param>
    /// <param name="fieldName">対象フィールド名（任意）</param>
    /// <returns>成功を示すValidationResult</returns>
    public static ValidationResult Success(string? successMessage = null, string? fieldName = null)
    {
        return new ValidationResult
        {
            IsValid = true,
            SuccessMessage = successMessage,
            FieldName = fieldName
        };
    }
    
    /// <summary>
    /// 失敗結果を生成する静的ファクトリーメソッド
    /// </summary>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <param name="fieldName">対象フィールド名（任意）</param>
    /// <returns>失敗を示すValidationResult</returns>
    public static ValidationResult Failure(string errorMessage, string? fieldName = null)
    {
        return new ValidationResult
        {
            IsValid = false,
            ErrorMessages = new List<string> { errorMessage },
            FieldName = fieldName
        };
    }
    
    /// <summary>
    /// 複数エラーでの失敗結果を生成する静的ファクトリーメソッド
    /// </summary>
    /// <param name="errorMessages">エラーメッセージリスト</param>
    /// <param name="fieldName">対象フィールド名（任意）</param>
    /// <returns>失敗を示すValidationResult</returns>
    public static ValidationResult Failure(List<string> errorMessages, string? fieldName = null)
    {
        return new ValidationResult
        {
            IsValid = false,
            ErrorMessages = errorMessages,
            FieldName = fieldName
        };
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// エラーメッセージを追加
    /// 既存のバリデーション結果に追加のエラー情報を付加
    /// </summary>
    /// <param name="errorMessage">追加するエラーメッセージ</param>
    public void AddError(string errorMessage)
    {
        ErrorMessages.Add(errorMessage);
    }
    
    /// <summary>
    /// 警告メッセージを追加
    /// 既存のバリデーション結果に警告情報を付加
    /// </summary>
    /// <param name="warningMessage">追加する警告メッセージ</param>
    public void AddWarning(string warningMessage)
    {
        WarningMessages.Add(warningMessage);
    }
    
    /// <summary>
    /// 全てのエラーメッセージを改行区切りで結合
    /// UI表示用の統合メッセージを生成
    /// </summary>
    /// <returns>結合されたエラーメッセージ</returns>
    public string GetCombinedErrorMessage()
    {
        return string.Join("\n", ErrorMessages);
    }
    
    /// <summary>
    /// エラーの有無を確認
    /// ErrorMessagesリストが空でない場合はエラーありとして判定
    /// </summary>
    /// <returns>エラーが存在する場合true</returns>
    public bool HasErrors => ErrorMessages.Any();
    
    /// <summary>
    /// 警告の有無を確認
    /// WarningMessagesリストが空でない場合は警告ありとして判定
    /// </summary>
    /// <returns>警告が存在する場合true</returns>
    public bool HasWarnings => WarningMessages.Any();
    
    #endregion
}
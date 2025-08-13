using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Application.Interfaces;

/// <summary>
/// 社員削除に関するビジネスロジックを提供するサービス
/// 削除前検証、削除実行、削除後処理を統合的に管理
/// </summary>
public interface IEmployeeDeleteService
{
    /// <summary>
    /// 社員削除の事前検証を実行する
    /// 削除制約（関連データの存在確認等）をチェック
    /// </summary>
    /// <param name="employeeNumber">削除対象の社員番号</param>
    /// <returns>検証結果（成功/失敗とメッセージ）</returns>
    Task<EmployeeValidationResult> ValidateDeleteAsync(string employeeNumber);
    
    /// <summary>  
    /// 社員削除を安全に実行する
    /// 事前検証、削除実行、削除後処理を統合的に行い、
    /// 関連データの整合性を保ちながら削除を実行
    /// </summary>
    /// <param name="employeeNumber">削除対象の社員番号</param>
    /// <returns>削除結果（成功/失敗とメッセージ）</returns>
    Task<Result<bool>> DeleteEmployeeAsync(string employeeNumber);

    /// <summary>
    /// 社員が部門責任者として設定されているかチェックする
    /// 将来的な削除制約の拡張に備えた検証メソッド
    /// </summary>
    /// <param name="employeeNumber">チェック対象の社員番号</param>
    /// <returns>責任者として設定されている場合はtrue</returns>
    Task<bool> IsManagerInAnyDepartmentAsync(string employeeNumber);
}

/// <summary>
/// 検証結果を表すクラス
/// バリデーション処理の結果と詳細メッセージを保持
/// </summary>
public class EmployeeValidationResult
{
    /// <summary>
    /// 検証が成功したかどうか
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// 検証結果のメッセージ
    /// エラーメッセージまたは成功メッセージ
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 詳細なエラーメッセージのリスト
    /// 複数の検証エラーがある場合に使用
    /// </summary>
    public List<string> ErrorMessages { get; set; } = new();

    /// <summary>
    /// 成功結果を作成する
    /// </summary>
    /// <param name="message">成功メッセージ</param>
    /// <returns>成功を表すEmployeeValidationResult</returns>
    public static EmployeeValidationResult Success(string message = "") => new() 
    { 
        IsValid = true, 
        Message = message 
    };

    /// <summary>
    /// 失敗結果を作成する
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    /// <returns>失敗を表すEmployeeValidationResult</returns>
    public static EmployeeValidationResult Failure(string message) => new() 
    { 
        IsValid = false, 
        Message = message,
        ErrorMessages = { message }
    };
}

/// <summary>
/// 処理結果を表すクラス
/// Result パターンによる安全なエラー伝播を実現
/// </summary>
/// <typeparam name="T">結果の型</typeparam>
public class Result<T>
{
    /// <summary>
    /// 処理が成功したかどうか
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 処理結果の値
    /// 成功時のみ有効
    /// </summary>
    public T Value { get; set; } = default!;

    /// <summary>
    /// エラーメッセージ
    /// 失敗時のみ有効
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// 成功結果を作成する
    /// </summary>
    /// <param name="value">成功時の値</param>
    /// <returns>成功を表すResult</returns>
    public static Result<T> Success(T value) => new() 
    { 
        IsSuccess = true, 
        Value = value 
    };

    /// <summary>
    /// 失敗結果を作成する
    /// </summary>
    /// <param name="error">エラーメッセージ</param>
    /// <returns>失敗を表すResult</returns>
    public static Result<T> Failure(string error) => new() 
    { 
        IsSuccess = false, 
        Error = error 
    };
}
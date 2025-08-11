using EmployeeManagement.Domain.Models;
using EmployeeManagement.Models;

namespace EmployeeManagement.Application.Interfaces;

/// <summary>
/// 部門データ操作を担当するサービスインターフェース
/// 複数のリポジトリを組み合わせた複合操作を提供
/// UIコンポーネントから複雑なデータ処理を分離し疎結合を実現
/// </summary>
public interface IDepartmentDataService
{
    /// <summary>
    /// 部門を作成する（バリデーションと重複チェック含む）
    /// </summary>
    /// <param name="department">作成する部門マスタ</param>
    /// <returns>
    /// 作成処理の結果
    /// - IsValid: 作成成功フラグ
    /// - ErrorMessages: 失敗時のエラーメッセージリスト
    /// - SuccessMessage: 成功時のメッセージ
    /// </returns>
    /// <remarks>
    /// このメソッドは以下の処理を実行します:
    /// 1. 部門データの包括的バリデーション
    /// 2. 部署コードの重複チェック
    /// 3. データベースへの登録処理
    /// 4. 登録後の整合性確認
    /// </remarks>
    Task<ValidationResult> CreateDepartmentAsync(DepartmentMaster department);
    
    /// <summary>
    /// 部門を更新する（バリデーションと整合性チェック含む）
    /// </summary>
    /// <param name="department">更新する部門マスタ</param>
    /// <returns>
    /// 更新処理の結果
    /// - IsValid: 更新成功フラグ
    /// - ErrorMessages: 失敗時のエラーメッセージリスト
    /// - SuccessMessage: 成功時のメッセージ
    /// </returns>
    /// <remarks>
    /// このメソッドは以下の処理を実行します:
    /// 1. 更新対象部門の存在確認
    /// 2. 部門データの包括的バリデーション
    /// 3. データベースの更新処理
    /// 4. 更新後の整合性確認
    /// </remarks>
    Task<ValidationResult> UpdateDepartmentAsync(DepartmentMaster department);
    
    /// <summary>
    /// 部門を削除する（制約チェック含む）
    /// </summary>
    /// <param name="departmentCode">削除対象の部署コード</param>
    /// <returns>
    /// 削除処理の結果
    /// - IsValid: 削除成功フラグ
    /// - ErrorMessages: 削除できない理由
    /// - SuccessMessage: 削除成功メッセージ
    /// </returns>
    /// <remarks>
    /// このメソッドは以下の処理を実行します:
    /// 1. 削除対象部門の存在確認
    /// 2. 削除制約の確認（所属社員の有無等）
    /// 3. 関連データの整理
    /// 4. データベースからの削除処理
    /// </remarks>
    Task<ValidationResult> DeleteDepartmentAsync(string departmentCode);
    
    /// <summary>
    /// 部門データを取得する（エラーハンドリング付き）
    /// </summary>
    /// <param name="departmentCode">取得対象の部署コード</param>
    /// <returns>
    /// 取得結果とエラー情報を含む結果オブジェクト
    /// - Success: 部門データ
    /// - Failure: エラーメッセージ
    /// </returns>
    /// <remarks>
    /// 単純な取得処理だが、エラーハンドリングと
    /// ログ出力を含めた安全な取得処理を提供
    /// </remarks>
    Task<DepartmentOperationResult> GetDepartmentAsync(string departmentCode);
    
    /// <summary>
    /// 全部門データを取得する（エラーハンドリング付き）
    /// </summary>
    /// <returns>
    /// 取得結果とエラー情報を含む結果オブジェクト
    /// - Success: 部門データリスト
    /// - Failure: エラーメッセージ
    /// </returns>
    /// <remarks>
    /// 大量データ処理の考慮とエラーハンドリングを含む
    /// 全件取得処理を提供
    /// </remarks>
    Task<DepartmentListOperationResult> GetAllDepartmentsAsync();
    
    /// <summary>
    /// 部門データの整合性チェックを実行
    /// </summary>
    /// <param name="departmentCode">チェック対象の部署コード（nullの場合は全部門）</param>
    /// <returns>
    /// 整合性チェック結果
    /// - IsValid: 整合性OK
    /// - ErrorMessages: 不整合項目のリスト
    /// </returns>
    /// <remarks>
    /// 定期的なデータ品質チェックや、
    /// 重要な処理前の事前確認で使用
    /// </remarks>
    Task<ValidationResult> ValidateDataIntegrityAsync(string? departmentCode = null);
}

/// <summary>
/// 部門操作結果を格納するクラス
/// 単一部門データの操作結果を表現
/// </summary>
public class DepartmentOperationResult
{
    /// <summary>操作成功フラグ</summary>
    public bool IsSuccess { get; init; }
    
    /// <summary>取得された部門データ（成功時のみ）</summary>
    public DepartmentMaster? Department { get; init; }
    
    /// <summary>エラーメッセージ（失敗時のみ）</summary>
    public string ErrorMessage { get; init; } = string.Empty;
    
    /// <summary>操作種別（ログ用）</summary>
    public string OperationType { get; init; } = string.Empty;
    
    /// <summary>
    /// 成功結果を生成する静的ファクトリーメソッド
    /// </summary>
    public static DepartmentOperationResult Success(DepartmentMaster department, string operationType = "取得")
    {
        return new DepartmentOperationResult
        {
            IsSuccess = true,
            Department = department,
            OperationType = operationType
        };
    }
    
    /// <summary>
    /// 失敗結果を生成する静的ファクトリーメソッド
    /// </summary>
    public static DepartmentOperationResult Failure(string errorMessage, string operationType = "操作")
    {
        return new DepartmentOperationResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            OperationType = operationType
        };
    }
}

/// <summary>
/// 部門リスト操作結果を格納するクラス
/// 複数部門データの操作結果を表現
/// </summary>
public class DepartmentListOperationResult
{
    /// <summary>操作成功フラグ</summary>
    public bool IsSuccess { get; init; }
    
    /// <summary>取得された部門データリスト（成功時のみ）</summary>
    public IEnumerable<DepartmentMaster> Departments { get; init; } = Enumerable.Empty<DepartmentMaster>();
    
    /// <summary>エラーメッセージ（失敗時のみ）</summary>
    public string ErrorMessage { get; init; } = string.Empty;
    
    /// <summary>取得件数</summary>
    public int Count => Departments?.Count() ?? 0;
    
    /// <summary>
    /// 成功結果を生成する静的ファクトリーメソッド
    /// </summary>
    public static DepartmentListOperationResult Success(IEnumerable<DepartmentMaster> departments)
    {
        return new DepartmentListOperationResult
        {
            IsSuccess = true,
            Departments = departments ?? Enumerable.Empty<DepartmentMaster>()
        };
    }
    
    /// <summary>
    /// 失敗結果を生成する静的ファクトリーメソッド
    /// </summary>
    public static DepartmentListOperationResult Failure(string errorMessage)
    {
        return new DepartmentListOperationResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Models;

namespace EmployeeManagement.Application.Interfaces;

/// <summary>
/// 責任者バリデーション処理を提供するサービスインターフェース
/// UIコンポーネントからビジネスロジックを分離し疎結合を実現
/// 責任者設定に関する全てのバリデーションロジックを集約
/// </summary>
public interface IManagerValidationService
{
    /// <summary>
    /// 責任者の社員番号を検証し詳細な結果を返却
    /// </summary>
    /// <param name="employeeNumber">検証する社員番号（nullまたは空文字可）</param>
    /// <returns>
    /// バリデーション結果を含むManagerValidationResult
    /// - 成功時: Employee情報と成功メッセージ
    /// - 失敗時: エラーメッセージ
    /// - 未設定時: 空状態として成功扱い
    /// </returns>
    /// <remarks>
    /// このメソッドは以下の検証を実行します:
    /// 1. 社員番号の形式チェック
    /// 2. 社員の存在確認
    /// 3. 責任者として適格かどうかの確認
    /// 4. 他部門での責任者重複チェック（警告レベル）
    /// </remarks>
    Task<ManagerValidationResult> ValidateManagerAsync(string? employeeNumber);
    
    /// <summary>
    /// 責任者設定をクリアする際のバリデーション
    /// </summary>
    /// <returns>
    /// クリア処理の妥当性を示すManagerValidationResult
    /// 通常は成功を返すが、業務制約がある場合はエラーとなる
    /// </returns>
    /// <remarks>
    /// 将来的に「必須責任者部門」などの制約がある場合に使用
    /// 現在の実装では常に成功を返す
    /// </remarks>
    Task<ManagerValidationResult> ValidateClearManagerAsync();
    
    /// <summary>
    /// 責任者として設定可能な社員の一覧を取得
    /// </summary>
    /// <returns>
    /// 責任者として設定可能な社員のリスト
    /// 退職者や無効な社員は除外される
    /// </returns>
    /// <remarks>
    /// オートコンプリート機能やダイアログ選択で使用
    /// パフォーマンス考慮のため結果をキャッシュ化
    /// </remarks>
    Task<IEnumerable<Employee>> GetEligibleManagersAsync();
    
    /// <summary>
    /// 指定した社員が他部門で責任者になっているかチェック
    /// </summary>
    /// <param name="employeeNumber">チェック対象の社員番号</param>
    /// <param name="excludeDepartmentCode">除外する部門コード（自部門など）</param>
    /// <returns>
    /// 他部門での責任者状況
    /// - true: 他部門で責任者になっている
    /// - false: 他部門では責任者ではない
    /// </returns>
    /// <remarks>
    /// 責任者重複の警告表示や制約チェックで使用
    /// ビジネスルールに応じて重複許可/不許可を判定
    /// </remarks>
    Task<bool> IsManagerInOtherDepartmentAsync(string employeeNumber, string? excludeDepartmentCode = null);
}
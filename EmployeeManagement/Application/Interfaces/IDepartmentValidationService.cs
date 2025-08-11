using EmployeeManagement.Domain.Models;
using EmployeeManagement.Models;

namespace EmployeeManagement.Application.Interfaces;

/// <summary>
/// 部門バリデーション処理を提供するサービスインターフェース
/// UIコンポーネントからビジネスロジックを分離し疎結合を実現
/// 部門マスタに関する全てのバリデーションロジックを集約
/// </summary>
public interface IDepartmentValidationService
{
    /// <summary>
    /// 部門マスタの包括的バリデーションを実行
    /// </summary>
    /// <param name="department">検証対象の部門マスタ</param>
    /// <param name="isNewDepartment">新規作成フラグ</param>
    /// <returns>
    /// バリデーション結果を含むValidationResult
    /// 複数のエラーがある場合は全て含めて返却
    /// </returns>
    /// <remarks>
    /// このメソッドは以下の検証を実行します:
    /// 1. 必須項目チェック（部署コード、部署名）
    /// 2. 部署コード形式・長さチェック
    /// 3. 部署名長さチェック
    /// 4. 設立日妥当性チェック
    /// 5. 新規作成時の重複チェック
    /// </remarks>
    Task<ValidationResult> ValidateDepartmentAsync(DepartmentMaster department, bool isNewDepartment);
    
    /// <summary>
    /// 部署コードの重複チェックを実行
    /// </summary>
    /// <param name="departmentCode">チェック対象の部署コード</param>
    /// <param name="excludeCurrentDepartment">除外する部門（編集時に自分自身を除外）</param>
    /// <returns>
    /// 重複チェック結果
    /// - true: 重複あり, false: 重複なし
    /// </returns>
    /// <remarks>
    /// 新規作成時と編集時の両方で使用
    /// 編集時は自分自身を除外してチェック
    /// </remarks>
    Task<bool> IsDepartmentCodeDuplicateAsync(string departmentCode, string? excludeCurrentDepartment = null);
    
    /// <summary>
    /// 部署コードの形式バリデーション
    /// </summary>
    /// <param name="departmentCode">検証対象の部署コード</param>
    /// <returns>
    /// 形式チェック結果
    /// 英数字のみ、長さ制限等を検証
    /// </returns>
    /// <remarks>
    /// UI入力時のリアルタイム検証で使用
    /// データベースアクセスを伴わない高速な検証
    /// </remarks>
    ValidationResult ValidateDepartmentCodeFormat(string? departmentCode);
    
    /// <summary>
    /// 部署名の妥当性バリデーション
    /// </summary>
    /// <param name="departmentName">検証対象の部署名</param>
    /// <returns>
    /// 部署名チェック結果
    /// 必須性、長さ制限等を検証
    /// </returns>
    /// <remarks>
    /// UI入力時のリアルタイム検証で使用
    /// データベースアクセスを伴わない高速な検証
    /// </remarks>
    ValidationResult ValidateDepartmentNameFormat(string? departmentName);
    
    /// <summary>
    /// 設立日の妥当性バリデーション
    /// </summary>
    /// <param name="establishedDate">検証対象の設立日</param>
    /// <returns>
    /// 設立日チェック結果
    /// 未来日制限、過去日制限等を検証
    /// </returns>
    /// <remarks>
    /// 設立日は過去日のみ有効とする
    /// 極端な過去日（200年以上前）も制限
    /// </remarks>
    ValidationResult ValidateEstablishedDate(DateTime? establishedDate);
    
    /// <summary>
    /// 部門削除可能性のチェック
    /// </summary>
    /// <param name="departmentCode">削除対象の部署コード</param>
    /// <returns>
    /// 削除可能性チェック結果
    /// 所属社員の存在、履歴データの有無等を検証
    /// </returns>
    /// <remarks>
    /// 部門削除機能で使用（将来実装予定）
    /// 業務制約により削除できない場合の検証
    /// </remarks>
    Task<ValidationResult> ValidateDepartmentDeletionAsync(string departmentCode);
}
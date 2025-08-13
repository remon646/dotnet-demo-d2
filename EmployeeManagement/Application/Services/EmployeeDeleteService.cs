using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Application.Services;

/// <summary>
/// 社員削除に関するビジネスロジックを実装するサービス
/// 削除前検証、削除実行、削除後処理を統合的に管理し、
/// データ整合性を保ちながら安全な削除処理を提供
/// </summary>
public class EmployeeDeleteService : IEmployeeDeleteService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<EmployeeDeleteService> _logger;

    // 定数定義 - マジックナンバー撲滅
    private const string EMPLOYEE_NOT_FOUND_MESSAGE = "指定された社員が見つかりません";
    private const string DELETE_SUCCESS_MESSAGE = "社員を削除しました";
    private const string DELETE_FAILED_MESSAGE = "社員の削除に失敗しました";
    private const string MANAGER_CONSTRAINT_MESSAGE = "この社員は部門責任者として設定されているため削除できません";
    private const string VALIDATION_ERROR_MESSAGE = "削除前検証でエラーが発生しました";
    private const string DELETE_ERROR_MESSAGE = "削除処理中にエラーが発生しました";

    /// <summary>
    /// EmployeeDeleteServiceのコンストラクタ
    /// 依存性注入により必要なサービスを受け取る
    /// </summary>
    /// <param name="employeeRepository">社員データアクセス用リポジトリ</param>
    /// <param name="departmentRepository">部門データアクセス用リポジトリ</param>
    /// <param name="logger">ログ出力用サービス</param>
    public EmployeeDeleteService(
        IEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository,
        ILogger<EmployeeDeleteService> logger)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 社員削除の事前検証を実行する
    /// 削除制約をチェックし、削除可能かどうかを判定
    /// </summary>
    /// <param name="employeeNumber">削除対象の社員番号</param>
    /// <returns>検証結果（成功/失敗とメッセージ）</returns>
    public async Task<EmployeeValidationResult> ValidateDeleteAsync(string employeeNumber)
    {
        _logger.LogDebug("社員削除前検証開始: {EmployeeNumber}", employeeNumber);

        try
        {
            // 入力値検証
            if (string.IsNullOrWhiteSpace(employeeNumber))
            {
                _logger.LogWarning("社員番号が空のため検証失敗");
                return EmployeeValidationResult.Failure("社員番号が指定されていません");
            }

            // 社員存在確認
            var employee = await _employeeRepository.GetByIdAsync(employeeNumber);
            if (employee == null)
            {
                _logger.LogWarning("社員が見つからないため検証失敗: {EmployeeNumber}", employeeNumber);
                return EmployeeValidationResult.Failure(EMPLOYEE_NOT_FOUND_MESSAGE);
            }

            // 部門責任者制約チェック（将来拡張）
            // 現在の実装では部門責任者の制約はないが、将来の拡張に備えて準備
            var isManager = await IsManagerInAnyDepartmentAsync(employeeNumber);
            if (isManager)
            {
                _logger.LogWarning("部門責任者のため削除制約に引っかかりました: {EmployeeNumber}", employeeNumber);
                return EmployeeValidationResult.Failure(MANAGER_CONSTRAINT_MESSAGE);
            }

            // TODO: 将来的な制約チェック
            // - プロジェクトメンバーとしてアサインされているかチェック
            // - 承認待ちの申請書の申請者/承認者になっているかチェック
            // - その他業務固有の制約チェック

            _logger.LogInformation("社員削除前検証成功: {EmployeeNumber} - {EmployeeName}", 
                employeeNumber, employee.Name);
                
            return EmployeeValidationResult.Success("削除可能です");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "社員削除前検証中にエラーが発生: {EmployeeNumber}", employeeNumber);
            return EmployeeValidationResult.Failure(VALIDATION_ERROR_MESSAGE);
        }
    }

    /// <summary>
    /// 社員削除を安全に実行する
    /// 事前検証、削除実行、削除後処理を統合的に行う
    /// </summary>
    /// <param name="employeeNumber">削除対象の社員番号</param>
    /// <returns>削除結果（成功/失敗とメッセージ）</returns>
    public async Task<Result<bool>> DeleteEmployeeAsync(string employeeNumber)
    {
        _logger.LogInformation("社員削除処理開始: {EmployeeNumber}", employeeNumber);

        try
        {
            // フェーズ1: 削除前検証
            var validationResult = await ValidateDeleteAsync(employeeNumber);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("削除前検証失敗: {EmployeeNumber} - {Error}", 
                    employeeNumber, validationResult.Message);
                return Result<bool>.Failure(validationResult.Message);
            }

            // フェーズ2: 関連データの取得（削除前ログ用）
            var employee = await _employeeRepository.GetByIdAsync(employeeNumber);
            if (employee == null)
            {
                // この時点での null は異常（検証段階で存在確認済み）
                _logger.LogError("削除実行時に社員が見つかりません: {EmployeeNumber}", employeeNumber);
                return Result<bool>.Failure(EMPLOYEE_NOT_FOUND_MESSAGE);
            }

            // フェーズ3: 削除実行
            _logger.LogInformation("社員削除実行: {EmployeeNumber} - {EmployeeName}", 
                employee.EmployeeNumber, employee.Name);
                
            var deleteSuccess = await _employeeRepository.DeleteAsync(employeeNumber);
            
            if (!deleteSuccess)
            {
                _logger.LogError("社員削除実行失敗: {EmployeeNumber}", employeeNumber);
                return Result<bool>.Failure(DELETE_FAILED_MESSAGE);
            }

            // フェーズ4: 削除後処理
            await PostDeleteProcessingAsync(employee);

            _logger.LogInformation("社員削除処理完了: {EmployeeNumber} - {EmployeeName}", 
                employee.EmployeeNumber, employee.Name);
                
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "社員削除処理中にエラーが発生: {EmployeeNumber}", employeeNumber);
            return Result<bool>.Failure(DELETE_ERROR_MESSAGE);
        }
    }

    /// <summary>
    /// 社員が部門責任者として設定されているかチェックする
    /// 将来的な削除制約の拡張に備えた検証メソッド
    /// </summary>
    /// <param name="employeeNumber">チェック対象の社員番号</param>
    /// <returns>責任者として設定されている場合はtrue</returns>
    public async Task<bool> IsManagerInAnyDepartmentAsync(string employeeNumber)
    {
        _logger.LogDebug("部門責任者チェック開始: {EmployeeNumber}", employeeNumber);

        try
        {
            // 現在の実装では部門責任者の概念がないため常にfalseを返す
            // 将来的に部門マスタに責任者フィールドが追加された場合はここで実装

            // TODO: 将来実装 - 部門マスタから責任者フィールドをチェック
            // var departments = await _departmentRepository.GetAllAsync();
            // var isManager = departments.Any(d => d.ManagerEmployeeNumber == employeeNumber);
            // return isManager;

            _logger.LogDebug("部門責任者チェック完了（現在制約なし）: {EmployeeNumber}", employeeNumber);
            return await Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "部門責任者チェック中にエラーが発生: {EmployeeNumber}", employeeNumber);
            // エラー時は安全側に倒して制約ありとみなす
            return true;
        }
    }

    /// <summary>
    /// 削除後処理を実行する
    /// 削除ログの記録、関連データのクリーンアップ等を行う
    /// </summary>
    /// <param name="deletedEmployee">削除された社員情報</param>
    private async Task PostDeleteProcessingAsync(Employee deletedEmployee)
    {
        _logger.LogDebug("削除後処理開始: {EmployeeNumber}", deletedEmployee.EmployeeNumber);

        try
        {
            // 削除ログの記録（構造化ログで運用監視に適した形式）
            _logger.LogInformation(
                "社員削除実行 - 社員番号: {EmployeeNumber}, 氏名: {Name}, 部署: {Department}, 役職: {Position}, 入社日: {JoinDate}",
                deletedEmployee.EmployeeNumber,
                deletedEmployee.Name,
                deletedEmployee.CurrentDepartmentDisplayName,
                deletedEmployee.CurrentPositionDisplayName,
                deletedEmployee.JoinDate.ToString("yyyy-MM-dd"));

            // TODO: 将来的な削除後処理
            // - 削除監査ログテーブルへの記録
            // - 関連する申請書類のステータス更新
            // - 外部システムへの削除通知
            // - キャッシュのクリア

            _logger.LogDebug("削除後処理完了: {EmployeeNumber}", deletedEmployee.EmployeeNumber);
        }
        catch (Exception ex)
        {
            // 削除後処理のエラーは削除成功に影響させない
            _logger.LogWarning(ex, "削除後処理中にエラーが発生（削除は成功）: {EmployeeNumber}", 
                deletedEmployee.EmployeeNumber);
        }

        await Task.CompletedTask;
    }
}
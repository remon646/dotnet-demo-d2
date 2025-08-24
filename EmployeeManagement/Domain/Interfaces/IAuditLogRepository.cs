using EmployeeManagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmployeeManagement.Domain.Interfaces
{
    /// <summary>
    /// 監査ログリポジトリのインターフェース
    /// 監査ログの永続化と取得に関する操作を定義します
    /// </summary>
    public interface IAuditLogRepository
    {
        /// <summary>
        /// 監査ログを追加します
        /// </summary>
        /// <param name="auditLog">追加する監査ログ</param>
        /// <returns>追加された監査ログ</returns>
        Task<AuditLog> AddAsync(AuditLog auditLog);

        /// <summary>
        /// 指定されたIDの監査ログを取得します
        /// </summary>
        /// <param name="id">監査ログID</param>
        /// <returns>監査ログ（存在しない場合はnull）</returns>
        Task<AuditLog?> GetByIdAsync(int id);

        /// <summary>
        /// 全ての監査ログを取得します
        /// </summary>
        /// <returns>監査ログのコレクション</returns>
        Task<IEnumerable<AuditLog>> GetAllAsync();

        /// <summary>
        /// 指定された条件で監査ログを検索します
        /// </summary>
        /// <param name="userId">ユーザーID（省略可）</param>
        /// <param name="action">操作種別（省略可）</param>
        /// <param name="entityType">エンティティ種別（省略可）</param>
        /// <param name="fromDate">開始日時（省略可）</param>
        /// <param name="toDate">終了日時（省略可）</param>
        /// <param name="level">ログレベル（省略可）</param>
        /// <returns>検索条件にマッチする監査ログのコレクション</returns>
        Task<IEnumerable<AuditLog>> SearchAsync(
            string? userId = null,
            string? action = null,
            string? entityType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            AuditLogLevel? level = null);

        /// <summary>
        /// 指定された日時より古い監査ログを削除します
        /// 自動削除機能で使用されます
        /// </summary>
        /// <param name="olderThan">削除対象となる日時</param>
        /// <returns>削除された監査ログ数</returns>
        Task<int> DeleteOlderThanAsync(DateTime olderThan);

        /// <summary>
        /// 指定されたエンティティの監査ログを取得します
        /// </summary>
        /// <param name="entityType">エンティティ種別</param>
        /// <param name="entityId">エンティティID</param>
        /// <returns>指定されたエンティティの監査ログのコレクション</returns>
        Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId);

        /// <summary>
        /// 監査ログの総数を取得します
        /// </summary>
        /// <returns>監査ログの総数</returns>
        Task<int> GetCountAsync();

        /// <summary>
        /// ページネーション対応の監査ログ取得
        /// </summary>
        /// <param name="skip">スキップする件数</param>
        /// <param name="take">取得する件数</param>
        /// <returns>指定されたページの監査ログのコレクション</returns>
        Task<IEnumerable<AuditLog>> GetPagedAsync(int skip, int take);
    }
}
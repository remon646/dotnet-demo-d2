using EmployeeManagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmployeeManagement.Application.Interfaces
{
    /// <summary>
    /// 監査ログサービスのインターフェース
    /// 監査ログの記録と取得に関するビジネスロジックを定義します
    /// </summary>
    public interface IAuditLogService
    {
        /// <summary>
        /// 監査ログを記録します
        /// </summary>
        /// <param name="userId">操作したユーザーID</param>
        /// <param name="userName">操作したユーザー名</param>
        /// <param name="action">操作種別</param>
        /// <param name="entityType">エンティティ種別</param>
        /// <param name="entityId">エンティティID</param>
        /// <param name="oldValues">変更前の値（JSON形式）</param>
        /// <param name="newValues">変更後の値（JSON形式）</param>
        /// <param name="ipAddress">IPアドレス</param>
        /// <param name="level">ログレベル</param>
        /// <param name="details">詳細情報</param>
        /// <returns>記録された監査ログ</returns>
        Task<AuditLog> LogAsync(
            string userId,
            string userName,
            string action,
            string entityType,
            string entityId,
            string? oldValues = null,
            string? newValues = null,
            string? ipAddress = null,
            AuditLogLevel level = AuditLogLevel.Info,
            string? details = null);

        /// <summary>
        /// エンティティの作成をログに記録します
        /// </summary>
        /// <typeparam name="T">エンティティの型</typeparam>
        /// <param name="userId">操作したユーザーID</param>
        /// <param name="userName">操作したユーザー名</param>
        /// <param name="entity">作成されたエンティティ</param>
        /// <param name="ipAddress">IPアドレス</param>
        /// <returns>記録された監査ログ</returns>
        Task<AuditLog> LogCreateAsync<T>(string userId, string userName, T entity, string? ipAddress = null);

        /// <summary>
        /// エンティティの更新をログに記録します
        /// </summary>
        /// <typeparam name="T">エンティティの型</typeparam>
        /// <param name="userId">操作したユーザーID</param>
        /// <param name="userName">操作したユーザー名</param>
        /// <param name="oldEntity">更新前のエンティティ</param>
        /// <param name="newEntity">更新後のエンティティ</param>
        /// <param name="ipAddress">IPアドレス</param>
        /// <returns>記録された監査ログ</returns>
        Task<AuditLog> LogUpdateAsync<T>(string userId, string userName, T oldEntity, T newEntity, string? ipAddress = null);

        /// <summary>
        /// エンティティの削除をログに記録します
        /// </summary>
        /// <typeparam name="T">エンティティの型</typeparam>
        /// <param name="userId">操作したユーザーID</param>
        /// <param name="userName">操作したユーザー名</param>
        /// <param name="entity">削除されたエンティティ</param>
        /// <param name="ipAddress">IPアドレス</param>
        /// <returns>記録された監査ログ</returns>
        Task<AuditLog> LogDeleteAsync<T>(string userId, string userName, T entity, string? ipAddress = null);

        /// <summary>
        /// ログインをログに記録します
        /// </summary>
        /// <param name="userId">ログインしたユーザーID</param>
        /// <param name="userName">ログインしたユーザー名</param>
        /// <param name="ipAddress">IPアドレス</param>
        /// <param name="isSuccess">ログイン成功フラグ</param>
        /// <returns>記録された監査ログ</returns>
        Task<AuditLog> LogLoginAsync(string userId, string userName, string? ipAddress = null, bool isSuccess = true);

        /// <summary>
        /// ログアウトをログに記録します
        /// </summary>
        /// <param name="userId">ログアウトしたユーザーID</param>
        /// <param name="userName">ログアウトしたユーザー名</param>
        /// <param name="ipAddress">IPアドレス</param>
        /// <returns>記録された監査ログ</returns>
        Task<AuditLog> LogLogoutAsync(string userId, string userName, string? ipAddress = null);

        /// <summary>
        /// 監査ログを取得します
        /// </summary>
        /// <param name="id">監査ログID</param>
        /// <returns>監査ログ（存在しない場合はnull）</returns>
        Task<AuditLog?> GetAuditLogAsync(int id);

        /// <summary>
        /// 全ての監査ログを取得します
        /// </summary>
        /// <returns>監査ログのコレクション</returns>
        Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync();

        /// <summary>
        /// 監査ログを検索します
        /// </summary>
        /// <param name="userId">ユーザーID（省略可）</param>
        /// <param name="action">操作種別（省略可）</param>
        /// <param name="entityType">エンティティ種別（省略可）</param>
        /// <param name="fromDate">開始日時（省略可）</param>
        /// <param name="toDate">終了日時（省略可）</param>
        /// <param name="level">ログレベル（省略可）</param>
        /// <returns>検索条件にマッチする監査ログのコレクション</returns>
        Task<IEnumerable<AuditLog>> SearchAuditLogsAsync(
            string? userId = null,
            string? action = null,
            string? entityType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            AuditLogLevel? level = null);

        /// <summary>
        /// 指定されたエンティティの監査ログを取得します
        /// </summary>
        /// <param name="entityType">エンティティ種別</param>
        /// <param name="entityId">エンティティID</param>
        /// <returns>指定されたエンティティの監査ログのコレクション</returns>
        Task<IEnumerable<AuditLog>> GetEntityAuditLogsAsync(string entityType, string entityId);

        /// <summary>
        /// ページネーション対応の監査ログ取得
        /// </summary>
        /// <param name="page">ページ番号（1から開始）</param>
        /// <param name="pageSize">1ページあたりの件数</param>
        /// <returns>指定されたページの監査ログと総件数</returns>
        Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedAuditLogsAsync(int page, int pageSize);

        /// <summary>
        /// 古い監査ログを自動削除します
        /// </summary>
        /// <param name="retentionDays">保存期間（日数）</param>
        /// <returns>削除された監査ログ数</returns>
        Task<int> CleanupOldLogsAsync(int retentionDays);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Domain.Interfaces
{
    /// <summary>
    /// 通知のデータアクセスを提供するリポジトリインターフェース
    /// </summary>
    public interface INotificationRepository
    {
        /// <summary>
        /// 通知を作成します
        /// </summary>
        /// <param name="notification">作成する通知</param>
        /// <returns>作成された通知のID</returns>
        Task<int> CreateAsync(Notification notification);

        /// <summary>
        /// 通知を更新します
        /// </summary>
        /// <param name="notification">更新する通知</param>
        /// <returns>更新成功の場合true</returns>
        Task<bool> UpdateAsync(Notification notification);

        /// <summary>
        /// 通知をIDで取得します
        /// </summary>
        /// <param name="id">通知ID</param>
        /// <returns>通知（存在しない場合はnull）</returns>
        Task<Notification?> GetByIdAsync(int id);

        /// <summary>
        /// ユーザーの通知一覧を取得します
        /// </summary>
        /// <param name="userId">ユーザーID（nullの場合は全ユーザー向け通知）</param>
        /// <param name="includeRead">既読通知を含むかどうか</param>
        /// <param name="includeExpired">期限切れ通知を含むかどうか</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <param name="page">ページ番号</param>
        /// <returns>通知のリスト</returns>
        Task<List<Notification>> GetUserNotificationsAsync(
            string? userId = null, 
            bool includeRead = true, 
            bool includeExpired = false,
            int pageSize = 50, 
            int page = 1);

        /// <summary>
        /// ユーザーの未読通知数を取得します
        /// </summary>
        /// <param name="userId">ユーザーID（nullの場合は全ユーザー向け通知）</param>
        /// <returns>未読通知数</returns>
        Task<int> GetUnreadCountAsync(string? userId = null);

        /// <summary>
        /// 通知を既読にマークします
        /// </summary>
        /// <param name="notificationId">通知ID</param>
        /// <param name="userId">ユーザーID（権限確認用）</param>
        /// <returns>更新成功の場合true</returns>
        Task<bool> MarkAsReadAsync(int notificationId, string? userId = null);

        /// <summary>
        /// ユーザーの全通知を既読にマークします
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>更新件数</returns>
        Task<int> MarkAllAsReadAsync(string userId);

        /// <summary>
        /// 通知を削除（論理削除）します
        /// </summary>
        /// <param name="notificationId">通知ID</param>
        /// <param name="userId">ユーザーID（権限確認用）</param>
        /// <returns>削除成功の場合true</returns>
        Task<bool> DeleteAsync(int notificationId, string? userId = null);

        /// <summary>
        /// 期限切れの通知を削除します
        /// </summary>
        /// <returns>削除件数</returns>
        Task<int> CleanupExpiredAsync();

        /// <summary>
        /// 既読の通知を削除します（ユーザー指定）
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>削除件数</returns>
        Task<int> DeleteAllReadAsync(string userId);

        /// <summary>
        /// 通知を検索します
        /// </summary>
        /// <param name="searchText">検索テキスト</param>
        /// <param name="type">通知種類（指定なしの場合は全種類）</param>
        /// <param name="priority">優先度（指定なしの場合は全優先度）</param>
        /// <param name="dateFrom">作成日時From</param>
        /// <param name="dateTo">作成日時To</param>
        /// <param name="userId">ユーザーID（指定なしの場合は全ユーザー）</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <param name="page">ページ番号</param>
        /// <returns>検索結果のリスト</returns>
        Task<List<Notification>> SearchAsync(
            string? searchText = null,
            NotificationType? type = null,
            NotificationPriority? priority = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            string? userId = null,
            int pageSize = 50,
            int page = 1);

        /// <summary>
        /// 通知統計を取得します
        /// </summary>
        /// <param name="dateFrom">集計期間From</param>
        /// <param name="dateTo">集計期間To</param>
        /// <param name="userId">ユーザーID（指定なしの場合は全ユーザー）</param>
        /// <returns>統計情報</returns>
        Task<NotificationStatistics> GetStatisticsAsync(
            DateTime dateFrom,
            DateTime dateTo,
            string? userId = null);
    }

    /// <summary>
    /// 通知統計情報を表すクラス
    /// </summary>
    public class NotificationStatistics
    {
        /// <summary>
        /// 総通知数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 未読通知数
        /// </summary>
        public int UnreadCount { get; set; }

        /// <summary>
        /// 既読率（％）
        /// </summary>
        public decimal ReadRate => TotalCount > 0 ? (decimal)(TotalCount - UnreadCount) / TotalCount * 100 : 0;

        /// <summary>
        /// 種類別統計
        /// </summary>
        public Dictionary<NotificationType, int> CountByType { get; set; } = new();

        /// <summary>
        /// 優先度別統計
        /// </summary>
        public Dictionary<NotificationPriority, int> CountByPriority { get; set; } = new();

        /// <summary>
        /// 日別統計
        /// </summary>
        public Dictionary<DateTime, int> CountByDay { get; set; } = new();
    }
}
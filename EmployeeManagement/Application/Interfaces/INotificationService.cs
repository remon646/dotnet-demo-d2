using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Application.Interfaces
{
    /// <summary>
    /// 通知管理サービスのインターフェース
    /// 通知の作成・配信・管理機能を提供します
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// 通知を作成します
        /// </summary>
        /// <param name="notification">作成する通知</param>
        /// <returns>作成された通知のID</returns>
        Task<int> CreateNotificationAsync(Notification notification);

        /// <summary>
        /// テンプレートから通知を作成します
        /// </summary>
        /// <param name="templateName">テンプレート名</param>
        /// <param name="parameters">テンプレートパラメーター</param>
        /// <param name="userId">対象ユーザーID（null の場合は全ユーザー）</param>
        /// <param name="createdBy">作成者</param>
        /// <returns>作成された通知のID</returns>
        Task<int> CreateFromTemplateAsync(string templateName, Dictionary<string, string> parameters, string? userId = null, string createdBy = "System");

        /// <summary>
        /// 通知をリアルタイム配信します
        /// </summary>
        /// <param name="notification">配信する通知</param>
        /// <returns>配信成功の場合true</returns>
        Task<bool> SendNotificationAsync(Notification notification);

        /// <summary>
        /// 指定ユーザーに通知を送信します
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="title">通知タイトル</param>
        /// <param name="message">通知メッセージ</param>
        /// <param name="type">通知種類</param>
        /// <param name="priority">優先度</param>
        /// <param name="actionUrl">アクションURL</param>
        /// <param name="actionText">アクションボタンテキスト</param>
        /// <param name="icon">アイコン名</param>
        /// <param name="createdBy">作成者</param>
        /// <returns>作成された通知のID</returns>
        Task<int> SendToUserAsync(
            string userId, 
            string title, 
            string message, 
            NotificationType type = NotificationType.System,
            NotificationPriority priority = NotificationPriority.Normal,
            string? actionUrl = null,
            string? actionText = null,
            string? icon = null,
            string createdBy = "System");

        /// <summary>
        /// 全ユーザーに通知を送信します
        /// </summary>
        /// <param name="title">通知タイトル</param>
        /// <param name="message">通知メッセージ</param>
        /// <param name="type">通知種類</param>
        /// <param name="priority">優先度</param>
        /// <param name="actionUrl">アクションURL</param>
        /// <param name="actionText">アクションボタンテキスト</param>
        /// <param name="icon">アイコン名</param>
        /// <param name="createdBy">作成者</param>
        /// <returns>作成された通知のID</returns>
        Task<int> SendToAllUsersAsync(
            string title, 
            string message, 
            NotificationType type = NotificationType.System,
            NotificationPriority priority = NotificationPriority.Normal,
            string? actionUrl = null,
            string? actionText = null,
            string? icon = null,
            string createdBy = "System");

        /// <summary>
        /// ユーザーの通知一覧を取得します
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="includeRead">既読通知を含むかどうか</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <param name="page">ページ番号</param>
        /// <returns>通知のリスト</returns>
        Task<List<Notification>> GetUserNotificationsAsync(string userId, bool includeRead = true, int pageSize = 50, int page = 1);

        /// <summary>
        /// ユーザーの未読通知数を取得します
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>未読通知数</returns>
        Task<int> GetUnreadCountAsync(string userId);

        /// <summary>
        /// 通知を既読にマークします
        /// </summary>
        /// <param name="notificationId">通知ID</param>
        /// <param name="userId">ユーザーID</param>
        /// <returns>更新成功の場合true</returns>
        Task<bool> MarkAsReadAsync(int notificationId, string userId);

        /// <summary>
        /// ユーザーの全通知を既読にマークします
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>更新件数</returns>
        Task<int> MarkAllAsReadAsync(string userId);

        /// <summary>
        /// 通知を削除します
        /// </summary>
        /// <param name="notificationId">通知ID</param>
        /// <param name="userId">ユーザーID</param>
        /// <returns>削除成功の場合true</returns>
        Task<bool> DeleteNotificationAsync(int notificationId, string userId);

        /// <summary>
        /// 既読通知を全て削除します
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>削除件数</returns>
        Task<int> DeleteAllReadAsync(string userId);

        /// <summary>
        /// ユーザーの通知設定を取得します
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>通知設定</returns>
        Task<NotificationSettings> GetUserSettingsAsync(string userId);

        /// <summary>
        /// ユーザーの通知設定を更新します
        /// </summary>
        /// <param name="settings">通知設定</param>
        /// <returns>更新成功の場合true</returns>
        Task<bool> UpdateUserSettingsAsync(NotificationSettings settings);

        /// <summary>
        /// 通知を検索します
        /// </summary>
        /// <param name="searchText">検索テキスト</param>
        /// <param name="userId">ユーザーID</param>
        /// <param name="type">通知種類</param>
        /// <param name="priority">優先度</param>
        /// <param name="dateFrom">作成日時From</param>
        /// <param name="dateTo">作成日時To</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <param name="page">ページ番号</param>
        /// <returns>検索結果のリスト</returns>
        Task<List<Notification>> SearchNotificationsAsync(
            string? searchText = null,
            string? userId = null,
            NotificationType? type = null,
            NotificationPriority? priority = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int pageSize = 50,
            int page = 1);

        /// <summary>
        /// 期限切れの通知をクリーンアップします
        /// </summary>
        /// <returns>削除件数</returns>
        Task<int> CleanupExpiredNotificationsAsync();

        /// <summary>
        /// 通知統計を取得します
        /// </summary>
        /// <param name="dateFrom">集計期間From</param>
        /// <param name="dateTo">集計期間To</param>
        /// <param name="userId">ユーザーID（指定なしの場合は全ユーザー）</param>
        /// <returns>統計情報</returns>
        Task<NotificationStatistics> GetNotificationStatisticsAsync(
            DateTime dateFrom,
            DateTime dateTo,
            string? userId = null);
    }
}
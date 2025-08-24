using System.Threading.Tasks;
using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Application.Interfaces
{
    /// <summary>
    /// 通知配信サービスのインターフェース
    /// SignalRを使用したリアルタイム通知配信を管理します
    /// </summary>
    public interface INotificationDeliveryService
    {
        /// <summary>
        /// 通知を適切な対象に配信します
        /// </summary>
        /// <param name="notification">配信する通知</param>
        /// <returns>配信成功の場合true</returns>
        Task<bool> DeliverAsync(Notification notification);

        /// <summary>
        /// 指定ユーザーに通知を配信します
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="notification">配信する通知</param>
        /// <returns>配信成功の場合true</returns>
        Task<bool> DeliverToUserAsync(string userId, Notification notification);

        /// <summary>
        /// 指定グループに通知を配信します
        /// </summary>
        /// <param name="groupName">グループ名</param>
        /// <param name="notification">配信する通知</param>
        /// <returns>配信成功の場合true</returns>
        Task<bool> DeliverToGroupAsync(string groupName, Notification notification);

        /// <summary>
        /// 全ユーザーに通知をブロードキャストします
        /// </summary>
        /// <param name="notification">配信する通知</param>
        /// <returns>配信成功の場合true</returns>
        Task<bool> DeliverBroadcastAsync(Notification notification);

        /// <summary>
        /// ユーザーの未読通知数を更新します
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="count">未読数</param>
        /// <returns>送信成功の場合true</returns>
        Task<bool> UpdateUnreadCountAsync(string userId, int count);

        /// <summary>
        /// システム通知を送信します（トーストのみ、永続化しない）
        /// </summary>
        /// <param name="userId">ユーザーID（nullの場合は全ユーザー）</param>
        /// <param name="title">通知タイトル</param>
        /// <param name="message">通知メッセージ</param>
        /// <param name="type">通知種類</param>
        /// <param name="priority">優先度</param>
        /// <param name="icon">アイコン名</param>
        /// <returns>送信成功の場合true</returns>
        Task<bool> SendToastNotificationAsync(
            string? userId,
            string title,
            string message,
            NotificationType type = NotificationType.System,
            NotificationPriority priority = NotificationPriority.Normal,
            string? icon = null);

        /// <summary>
        /// 接続中のユーザー数を取得します
        /// </summary>
        /// <returns>接続中のユーザー数</returns>
        Task<int> GetConnectedUserCountAsync();

        /// <summary>
        /// 指定ユーザーがオンラインかどうかを確認します
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>オンラインの場合true</returns>
        Task<bool> IsUserOnlineAsync(string userId);
    }
}
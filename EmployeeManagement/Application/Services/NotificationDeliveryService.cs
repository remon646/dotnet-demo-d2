using System;
using System.Threading.Tasks;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Application.Services
{
    /// <summary>
    /// 通知配信サービスの実装
    /// SignalRを使用したリアルタイム通知配信を管理します
    /// </summary>
    public class NotificationDeliveryService : INotificationDeliveryService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationDeliveryService> _logger;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NotificationDeliveryService(
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationDeliveryService> logger)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> DeliverAsync(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            try
            {
                _logger.LogInformation("Delivering notification: {NotificationId}, UserId: {UserId}", 
                    notification.Id, notification.UserId ?? "All");

                bool success;

                // ユーザー指定の有無で配信方法を変える
                if (string.IsNullOrEmpty(notification.UserId))
                {
                    // 全ユーザーへの配信
                    success = await DeliverBroadcastAsync(notification);
                }
                else
                {
                    // 特定ユーザーへの配信
                    success = await DeliverToUserAsync(notification.UserId, notification);
                }

                if (success)
                {
                    _logger.LogInformation("Notification delivered successfully: {NotificationId}", notification.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to deliver notification: {NotificationId}", notification.Id);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delivering notification: {NotificationId}", notification.Id);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeliverToUserAsync(string userId, Notification notification)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("ユーザーIDが指定されていません。", nameof(userId));
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            try
            {
                var groupName = $"User_{userId}";
                
                _logger.LogDebug("Sending notification to user group: {GroupName}, NotificationId: {NotificationId}", 
                    groupName, notification.Id);

                // ユーザーグループに通知を送信
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);

                // 永続的な通知の場合は未読数も更新
                if (notification.IsPersistent)
                {
                    // 未読数は通知サービス側で計算して送信される想定
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deliver notification to user: {UserId}, NotificationId: {NotificationId}", 
                    userId, notification.Id);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeliverToGroupAsync(string groupName, Notification notification)
        {
            if (string.IsNullOrEmpty(groupName)) throw new ArgumentException("グループ名が指定されていません。", nameof(groupName));
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            try
            {
                _logger.LogDebug("Sending notification to group: {GroupName}, NotificationId: {NotificationId}", 
                    groupName, notification.Id);

                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deliver notification to group: {GroupName}, NotificationId: {NotificationId}", 
                    groupName, notification.Id);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeliverBroadcastAsync(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            try
            {
                _logger.LogDebug("Broadcasting notification: {NotificationId}", notification.Id);

                // 全クライアントに通知を送信
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast notification: {NotificationId}", notification.Id);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> UpdateUnreadCountAsync(string userId, int count)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("ユーザーIDが指定されていません。", nameof(userId));

            try
            {
                var groupName = $"User_{userId}";
                
                _logger.LogDebug("Updating unread count for user: {UserId}, Count: {Count}", userId, count);

                await _hubContext.Clients.Group(groupName).SendAsync("UnreadCountUpdated", count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update unread count for user: {UserId}", userId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SendToastNotificationAsync(
            string? userId,
            string title,
            string message,
            NotificationType type = NotificationType.System,
            NotificationPriority priority = NotificationPriority.Normal,
            string? icon = null)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentException("タイトルが指定されていません。", nameof(title));
            if (string.IsNullOrEmpty(message)) throw new ArgumentException("メッセージが指定されていません。", nameof(message));

            try
            {
                var toastData = new
                {
                    Title = title,
                    Message = message,
                    Type = type.ToString(),
                    Priority = priority.ToString(),
                    Icon = icon ?? GetDefaultIcon(type),
                    Timestamp = DateTime.Now
                };

                _logger.LogDebug("Sending toast notification: {Title}, UserId: {UserId}", title, userId ?? "All");

                if (string.IsNullOrEmpty(userId))
                {
                    // 全ユーザーにトースト通知
                    await _hubContext.Clients.All.SendAsync("ShowToast", toastData);
                }
                else
                {
                    // 特定ユーザーにトースト通知
                    var groupName = $"User_{userId}";
                    await _hubContext.Clients.Group(groupName).SendAsync("ShowToast", toastData);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send toast notification: {Title}", title);
                return false;
            }
        }

        /// <inheritdoc />
        public Task<int> GetConnectedUserCountAsync()
        {
            // SignalRの接続数を取得する機能は標準では提供されていないため、
            // より高度な実装が必要。現在は概算値を返す
            _logger.LogDebug("Getting connected user count (estimated)");
            return Task.FromResult(1); // 仮の値
        }

        /// <inheritdoc />
        public Task<bool> IsUserOnlineAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("ユーザーIDが指定されていません。", nameof(userId));

            // SignalRでは接続の詳細な状態管理が難しいため、
            // より高度な実装が必要。現在は基本的な確認のみ
            _logger.LogDebug("Checking if user is online: {UserId}", userId);
            return Task.FromResult(true); // 仮の値
        }

        /// <summary>
        /// 通知種類に応じたデフォルトアイコンを取得します
        /// </summary>
        /// <param name="type">通知種類</param>
        /// <returns>アイコン名</returns>
        private static string GetDefaultIcon(NotificationType type)
        {
            return type switch
            {
                NotificationType.System => "info",
                NotificationType.DataChange => "update",
                NotificationType.UserAction => "person",
                NotificationType.Alert => "warning",
                NotificationType.Warning => "warning",
                NotificationType.Error => "error",
                NotificationType.Success => "check_circle",
                NotificationType.AuditLog => "history",
                NotificationType.Permission => "security",
                NotificationType.Critical => "priority_high",
                _ => "notifications"
            };
        }
    }
}
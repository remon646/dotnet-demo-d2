using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Infrastructure.Hubs
{
    /// <summary>
    /// リアルタイム通知機能を提供するSignalRハブ
    /// ユーザー間の通知配信とリアルタイム通信を管理します
    /// </summary>
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        private readonly INotificationService _notificationService;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger">ロガー</param>
        /// <param name="notificationService">通知サービス</param>
        public NotificationHub(
            ILogger<NotificationHub> logger,
            INotificationService notificationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        /// <summary>
        /// ユーザーグループに参加します
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>非同期タスク</returns>
        public async Task JoinUserGroup(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("JoinUserGroup called with empty userId. ConnectionId: {ConnectionId}", Context.ConnectionId);
                return;
            }

            var groupName = $"User_{userId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            _logger.LogInformation("User {UserId} joined group {GroupName}. ConnectionId: {ConnectionId}", 
                userId, groupName, Context.ConnectionId);

            // クライアントに参加完了を通知
            await Clients.Caller.SendAsync("UserGroupJoined", userId);
        }

        /// <summary>
        /// ユーザーグループから離脱します
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>非同期タスク</returns>
        public async Task LeaveUserGroup(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("LeaveUserGroup called with empty userId. ConnectionId: {ConnectionId}", Context.ConnectionId);
                return;
            }

            var groupName = $"User_{userId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            
            _logger.LogInformation("User {UserId} left group {GroupName}. ConnectionId: {ConnectionId}", 
                userId, groupName, Context.ConnectionId);

            // クライアントに離脱完了を通知
            await Clients.Caller.SendAsync("UserGroupLeft", userId);
        }

        /// <summary>
        /// 通知を既読にマークします
        /// </summary>
        /// <param name="notificationId">通知ID</param>
        /// <returns>非同期タスク</returns>
        public async Task MarkAsRead(int notificationId)
        {
            try
            {
                var userId = Context.UserIdentifier ?? GetUserIdFromContext();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("MarkAsRead called without valid userId. ConnectionId: {ConnectionId}", Context.ConnectionId);
                    await Clients.Caller.SendAsync("Error", "ユーザー認証が必要です。");
                    return;
                }

                var success = await _notificationService.MarkAsReadAsync(notificationId, userId);
                
                if (success)
                {
                    _logger.LogInformation("Notification {NotificationId} marked as read by user {UserId}", notificationId, userId);
                    
                    // 既読マーク完了をクライアントに通知
                    await Clients.Caller.SendAsync("NotificationMarkedAsRead", notificationId);
                    
                    // 未読数を更新
                    var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
                    await Clients.Caller.SendAsync("UnreadCountUpdated", unreadCount);
                }
                else
                {
                    _logger.LogWarning("Failed to mark notification {NotificationId} as read for user {UserId}", notificationId, userId);
                    await Clients.Caller.SendAsync("Error", "通知の既読マークに失敗しました。");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
                await Clients.Caller.SendAsync("Error", "エラーが発生しました。");
            }
        }

        /// <summary>
        /// 全通知を既読にマークします
        /// </summary>
        /// <returns>非同期タスク</returns>
        public async Task MarkAllAsRead()
        {
            try
            {
                var userId = Context.UserIdentifier ?? GetUserIdFromContext();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("MarkAllAsRead called without valid userId. ConnectionId: {ConnectionId}", Context.ConnectionId);
                    await Clients.Caller.SendAsync("Error", "ユーザー認証が必要です。");
                    return;
                }

                var count = await _notificationService.MarkAllAsReadAsync(userId);
                
                _logger.LogInformation("Marked {Count} notifications as read for user {UserId}", count, userId);
                
                // 全既読完了をクライアントに通知
                await Clients.Caller.SendAsync("AllNotificationsMarkedAsRead", count);
                
                // 未読数を更新（0になるはず）
                await Clients.Caller.SendAsync("UnreadCountUpdated", 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                await Clients.Caller.SendAsync("Error", "エラーが発生しました。");
            }
        }

        /// <summary>
        /// 通知を削除します
        /// </summary>
        /// <param name="notificationId">通知ID</param>
        /// <returns>非同期タスク</returns>
        public async Task DeleteNotification(int notificationId)
        {
            try
            {
                var userId = Context.UserIdentifier ?? GetUserIdFromContext();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("DeleteNotification called without valid userId. ConnectionId: {ConnectionId}", Context.ConnectionId);
                    await Clients.Caller.SendAsync("Error", "ユーザー認証が必要です。");
                    return;
                }

                var success = await _notificationService.DeleteNotificationAsync(notificationId, userId);
                
                if (success)
                {
                    _logger.LogInformation("Notification {NotificationId} deleted by user {UserId}", notificationId, userId);
                    
                    // 削除完了をクライアントに通知
                    await Clients.Caller.SendAsync("NotificationDeleted", notificationId);
                    
                    // 未読数を更新
                    var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
                    await Clients.Caller.SendAsync("UnreadCountUpdated", unreadCount);
                }
                else
                {
                    _logger.LogWarning("Failed to delete notification {NotificationId} for user {UserId}", notificationId, userId);
                    await Clients.Caller.SendAsync("Error", "通知の削除に失敗しました。");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId}", notificationId);
                await Clients.Caller.SendAsync("Error", "エラーが発生しました。");
            }
        }

        /// <summary>
        /// ユーザーの未読通知数を取得します
        /// </summary>
        /// <returns>非同期タスク</returns>
        public async Task GetUnreadCount()
        {
            try
            {
                var userId = Context.UserIdentifier ?? GetUserIdFromContext();
                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("UnreadCountUpdated", 0);
                    return;
                }

                var count = await _notificationService.GetUnreadCountAsync(userId);
                await Clients.Caller.SendAsync("UnreadCountUpdated", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                await Clients.Caller.SendAsync("UnreadCountUpdated", 0);
            }
        }

        /// <summary>
        /// クライアントが接続したときの処理
        /// </summary>
        /// <returns>非同期タスク</returns>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier ?? GetUserIdFromContext();
            
            _logger.LogInformation("Client connected. ConnectionId: {ConnectionId}, UserId: {UserId}", 
                Context.ConnectionId, userId ?? "Unknown");

            if (!string.IsNullOrEmpty(userId))
            {
                // 自動的にユーザーグループに参加
                await JoinUserGroup(userId);
                
                // 未読通知数を送信
                await GetUnreadCount();
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// クライアントが切断したときの処理
        /// </summary>
        /// <param name="exception">切断時のエラー情報</param>
        /// <returns>非同期タスク</returns>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier ?? GetUserIdFromContext();
            
            _logger.LogInformation("Client disconnected. ConnectionId: {ConnectionId}, UserId: {UserId}, Exception: {Exception}", 
                Context.ConnectionId, userId ?? "Unknown", exception?.Message);

            if (!string.IsNullOrEmpty(userId))
            {
                // ユーザーグループから離脱
                await LeaveUserGroup(userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 接続コンテキストからユーザーIDを取得します（フォールバック処理）
        /// </summary>
        /// <returns>ユーザーID</returns>
        private string? GetUserIdFromContext()
        {
            // HTTPコンテキストからセッション経由でユーザーIDを取得を試みる
            try
            {
                var httpContext = Context.GetHttpContext();
                if (httpContext?.Session != null)
                {
                    return httpContext.Session.GetString("UserId");
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to get userId from session");
            }

            return null;
        }
    }
}
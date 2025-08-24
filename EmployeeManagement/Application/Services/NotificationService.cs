using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Application.Services
{
    /// <summary>
    /// 通知管理サービスの実装
    /// 通知の作成・配信・管理機能を提供します
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationTemplateRepository _templateRepository;
        private readonly INotificationSettingsRepository _settingsRepository;
        private readonly INotificationDeliveryService _deliveryService;
        private readonly ILogger<NotificationService> _logger;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NotificationService(
            INotificationRepository notificationRepository,
            INotificationTemplateRepository templateRepository,
            INotificationSettingsRepository settingsRepository,
            INotificationDeliveryService deliveryService,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
            _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
            _deliveryService = deliveryService ?? throw new ArgumentNullException(nameof(deliveryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<int> CreateNotificationAsync(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            try
            {
                _logger.LogInformation("Creating notification: {Title}", notification.Title);

                // 通知を作成
                var notificationId = await _notificationRepository.CreateAsync(notification);
                notification.Id = notificationId;

                _logger.LogInformation("Notification created with ID: {NotificationId}", notificationId);

                // リアルタイム配信
                await _deliveryService.DeliverAsync(notification);

                return notificationId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create notification: {Title}", notification.Title);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> CreateFromTemplateAsync(string templateName, Dictionary<string, string> parameters, string? userId = null, string createdBy = "System")
        {
            if (string.IsNullOrEmpty(templateName)) throw new ArgumentException("テンプレート名が指定されていません。", nameof(templateName));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            try
            {
                _logger.LogInformation("Creating notification from template: {TemplateName}, UserId: {UserId}", templateName, userId);

                // テンプレートを取得
                var template = await _templateRepository.GetByNameAsync(templateName);
                if (template == null)
                {
                    _logger.LogWarning("Template not found: {TemplateName}", templateName);
                    throw new ArgumentException($"テンプレート '{templateName}' が見つかりません。", nameof(templateName));
                }

                if (!template.IsActive)
                {
                    _logger.LogWarning("Template is inactive: {TemplateName}", templateName);
                    throw new InvalidOperationException($"テンプレート '{templateName}' は無効化されています。");
                }

                // テンプレートから通知を生成
                var notification = template.GenerateNotification(parameters, userId, createdBy);

                // 通知を作成・配信
                return await CreateNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create notification from template: {TemplateName}", templateName);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SendNotificationAsync(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            try
            {
                _logger.LogInformation("Sending notification: {NotificationId}", notification.Id);

                // 配信サービスを使用してリアルタイム配信
                var success = await _deliveryService.DeliverAsync(notification);

                if (success)
                {
                    _logger.LogInformation("Notification sent successfully: {NotificationId}", notification.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to send notification: {NotificationId}", notification.Id);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification: {NotificationId}", notification.Id);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<int> SendToUserAsync(
            string userId, 
            string title, 
            string message, 
            NotificationType type = NotificationType.System,
            NotificationPriority priority = NotificationPriority.Normal,
            string? actionUrl = null,
            string? actionText = null,
            string? icon = null,
            string createdBy = "System")
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("ユーザーIDが指定されていません。", nameof(userId));
            if (string.IsNullOrEmpty(title)) throw new ArgumentException("タイトルが指定されていません。", nameof(title));
            if (string.IsNullOrEmpty(message)) throw new ArgumentException("メッセージが指定されていません。", nameof(message));

            var notification = new Notification
            {
                Title = title,
                Message = message,
                Type = type,
                Priority = priority,
                UserId = userId,
                CreatedBy = createdBy,
                ActionUrl = actionUrl ?? string.Empty,
                ActionText = actionText ?? string.Empty,
                Icon = icon ?? GetDefaultIcon(type),
                CreatedAt = DateTime.Now
            };

            return await CreateNotificationAsync(notification);
        }

        /// <inheritdoc />
        public async Task<int> SendToAllUsersAsync(
            string title, 
            string message, 
            NotificationType type = NotificationType.System,
            NotificationPriority priority = NotificationPriority.Normal,
            string? actionUrl = null,
            string? actionText = null,
            string? icon = null,
            string createdBy = "System")
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentException("タイトルが指定されていません。", nameof(title));
            if (string.IsNullOrEmpty(message)) throw new ArgumentException("メッセージが指定されていません。", nameof(message));

            var notification = new Notification
            {
                Title = title,
                Message = message,
                Type = type,
                Priority = priority,
                UserId = null, // 全ユーザー向け
                CreatedBy = createdBy,
                ActionUrl = actionUrl ?? string.Empty,
                ActionText = actionText ?? string.Empty,
                Icon = icon ?? GetDefaultIcon(type),
                CreatedAt = DateTime.Now
            };

            return await CreateNotificationAsync(notification);
        }

        /// <inheritdoc />
        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, bool includeRead = true, int pageSize = 50, int page = 1)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("ユーザーIDが指定されていません。", nameof(userId));

            try
            {
                return await _notificationRepository.GetUserNotificationsAsync(userId, includeRead, false, pageSize, page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user notifications for user: {UserId}", userId);
                return new List<Notification>();
            }
        }

        /// <inheritdoc />
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("ユーザーIDが指定されていません。", nameof(userId));

            try
            {
                return await _notificationRepository.GetUnreadCountAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get unread count for user: {UserId}", userId);
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("ユーザーIDが指定されていません。", nameof(userId));

            try
            {
                var success = await _notificationRepository.MarkAsReadAsync(notificationId, userId);
                
                if (success)
                {
                    // 未読数を更新通知
                    var unreadCount = await GetUnreadCountAsync(userId);
                    await _deliveryService.UpdateUnreadCountAsync(userId, unreadCount);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark notification as read: {NotificationId}, User: {UserId}", notificationId, userId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<int> MarkAllAsReadAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("ユーザーIDが指定されていません。", nameof(userId));

            try
            {
                var count = await _notificationRepository.MarkAllAsReadAsync(userId);
                
                // 未読数を更新通知（0になるはず）
                await _deliveryService.UpdateUnreadCountAsync(userId, 0);

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark all notifications as read for user: {UserId}", userId);
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteNotificationAsync(int notificationId, string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("ユーザーIDが指定されていません。", nameof(userId));

            try
            {
                var success = await _notificationRepository.DeleteAsync(notificationId, userId);
                
                if (success)
                {
                    // 未読数を更新通知
                    var unreadCount = await GetUnreadCountAsync(userId);
                    await _deliveryService.UpdateUnreadCountAsync(userId, unreadCount);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete notification: {NotificationId}, User: {UserId}", notificationId, userId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<int> DeleteAllReadAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("ユーザーIDが指定されていません。", nameof(userId));

            try
            {
                return await _notificationRepository.DeleteAllReadAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete all read notifications for user: {UserId}", userId);
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<NotificationSettings> GetUserSettingsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("ユーザーIDが指定されていません。", nameof(userId));

            try
            {
                return await _settingsRepository.GetByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user notification settings: {UserId}", userId);
                
                // エラー時はデフォルト設定を返す
                return new NotificationSettings { UserId = userId };
            }
        }

        /// <inheritdoc />
        public async Task<bool> UpdateUserSettingsAsync(NotificationSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrEmpty(settings.UserId)) throw new ArgumentException("ユーザーIDが指定されていません。");

            try
            {
                return await _settingsRepository.SaveAsync(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user notification settings: {UserId}", settings.UserId);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<List<Notification>> SearchNotificationsAsync(
            string? searchText = null,
            string? userId = null,
            NotificationType? type = null,
            NotificationPriority? priority = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int pageSize = 50,
            int page = 1)
        {
            try
            {
                return await _notificationRepository.SearchAsync(searchText, type, priority, dateFrom, dateTo, userId, pageSize, page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search notifications");
                return new List<Notification>();
            }
        }

        /// <inheritdoc />
        public async Task<int> CleanupExpiredNotificationsAsync()
        {
            try
            {
                var count = await _notificationRepository.CleanupExpiredAsync();
                _logger.LogInformation("Cleaned up {Count} expired notifications", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup expired notifications");
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<NotificationStatistics> GetNotificationStatisticsAsync(
            DateTime dateFrom,
            DateTime dateTo,
            string? userId = null)
        {
            try
            {
                return await _notificationRepository.GetStatisticsAsync(dateFrom, dateTo, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get notification statistics");
                return new NotificationStatistics();
            }
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
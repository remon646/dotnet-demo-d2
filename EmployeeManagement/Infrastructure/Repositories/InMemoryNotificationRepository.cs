using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Infrastructure.DataStores;

namespace EmployeeManagement.Infrastructure.Repositories
{
    /// <summary>
    /// 通知のインメモリリポジトリ実装
    /// </summary>
    public class InMemoryNotificationRepository : INotificationRepository
    {
        private readonly ConcurrentInMemoryDataStore _dataStore;
        private readonly string _collectionName = "Notifications";
        private ConcurrentBag<Notification> _notifications;
        private int _nextId = 1;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataStore">データストア</param>
        public InMemoryNotificationRepository(ConcurrentInMemoryDataStore dataStore)
        {
            _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
            InitializeCollection();
        }

        /// <summary>
        /// コレクションを初期化します
        /// </summary>
        private void InitializeCollection()
        {
            if (!_dataStore.HasCollection(_collectionName))
            {
                _dataStore.InitializeCollection<Notification>(_collectionName);
                
                // デモ用のサンプル通知を作成
                CreateSampleNotifications();
            }

            _notifications = _dataStore.GetCollection<Notification>(_collectionName);
            
            // 次のIDを設定
            var maxId = _notifications.Any() ? _notifications.Max(n => n.Id) : 0;
            _nextId = maxId + 1;
        }

        /// <summary>
        /// サンプル通知を作成します
        /// </summary>
        private void CreateSampleNotifications()
        {
            var sampleNotifications = new[]
            {
                new Notification
                {
                    Id = 1,
                    Title = "システムメンテナンスのお知らせ",
                    Message = "明日の深夜2:00-4:00にシステムメンテナンスを実施いたします。この時間帯はシステムをご利用いただけません。",
                    Type = NotificationType.System,
                    Priority = NotificationPriority.High,
                    CreatedBy = "System",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    Icon = "construction",
                    IsPersistent = true
                },
                new Notification
                {
                    Id = 2,
                    Title = "新機能リリース",
                    Message = "通知システム機能がリリースされました。リアルタイム通知をお楽しみください。",
                    Type = NotificationType.System,
                    Priority = NotificationPriority.Normal,
                    CreatedBy = "System",
                    CreatedAt = DateTime.Now.AddHours(-6),
                    Icon = "new_releases",
                    ActionUrl = "/notifications",
                    ActionText = "詳細を見る",
                    IsPersistent = true
                },
                new Notification
                {
                    Id = 3,
                    Title = "社員情報更新完了",
                    Message = "山田太郎さんの情報が正常に更新されました。",
                    Type = NotificationType.DataChange,
                    Priority = NotificationPriority.Normal,
                    UserId = "admin",
                    CreatedBy = "admin",
                    CreatedAt = DateTime.Now.AddHours(-2),
                    Icon = "person",
                    ActionUrl = "/employees",
                    ActionText = "社員一覧を見る",
                    IsPersistent = true
                },
                new Notification
                {
                    Id = 4,
                    Title = "ログイン成功",
                    Message = "システムに正常にログインしました。",
                    Type = NotificationType.UserAction,
                    Priority = NotificationPriority.Low,
                    UserId = "admin",
                    CreatedBy = "System",
                    CreatedAt = DateTime.Now.AddMinutes(-30),
                    Icon = "login",
                    IsPersistent = false,
                    IsRead = true,
                    ReadAt = DateTime.Now.AddMinutes(-29)
                }
            };

            var notifications = _dataStore.GetCollection<Notification>(_collectionName);
            foreach (var notification in sampleNotifications)
            {
                notifications.Add(notification);
            }
        }

        /// <inheritdoc />
        public Task<int> CreateAsync(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            notification.Id = _nextId++;
            notification.CreatedAt = DateTime.Now;

            _notifications.Add(notification);
            return Task.FromResult(notification.Id);
        }

        /// <inheritdoc />
        public Task<bool> UpdateAsync(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            var existing = _notifications.FirstOrDefault(n => n.Id == notification.Id);
            if (existing == null) return Task.FromResult(false);

            // 既存の通知を削除して新しいものを追加（ConcurrentBagの制限）
            var updatedList = _notifications.Where(n => n.Id != notification.Id).ToList();
            updatedList.Add(notification);

            _dataStore.ReplaceCollection(_collectionName, new ConcurrentBag<Notification>(updatedList));
            _notifications = _dataStore.GetCollection<Notification>(_collectionName);

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<Notification?> GetByIdAsync(int id)
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == id && !n.IsDeleted);
            return Task.FromResult(notification);
        }

        /// <inheritdoc />
        public Task<List<Notification>> GetUserNotificationsAsync(
            string? userId = null, 
            bool includeRead = true, 
            bool includeExpired = false,
            int pageSize = 50, 
            int page = 1)
        {
            var query = _notifications.Where(n => !n.IsDeleted);

            // ユーザー指定がある場合は、そのユーザー向けまたは全ユーザー向け通知をフィルタ
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(n => n.UserId == userId || string.IsNullOrEmpty(n.UserId));
            }

            // 既読フィルタ
            if (!includeRead)
            {
                query = query.Where(n => !n.IsRead);
            }

            // 期限切れフィルタ
            if (!includeExpired)
            {
                query = query.Where(n => !n.IsExpired());
            }

            // ページング処理
            var result = query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<int> GetUnreadCountAsync(string? userId = null)
        {
            var query = _notifications.Where(n => !n.IsDeleted && !n.IsRead && !n.IsExpired());

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(n => n.UserId == userId || string.IsNullOrEmpty(n.UserId));
            }

            var count = query.Count();
            return Task.FromResult(count);
        }

        /// <inheritdoc />
        public Task<bool> MarkAsReadAsync(int notificationId, string? userId = null)
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == notificationId && !n.IsDeleted);
            if (notification == null) return Task.FromResult(false);

            // ユーザー指定がある場合は権限をチェック
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(notification.UserId))
            {
                if (notification.UserId != userId)
                {
                    return Task.FromResult(false); // 権限なし
                }
            }

            notification.MarkAsRead();
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<int> MarkAllAsReadAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("ユーザーIDが指定されていません。", nameof(userId));

            var notifications = _notifications
                .Where(n => !n.IsDeleted && !n.IsRead && !n.IsExpired() && 
                           (n.UserId == userId || string.IsNullOrEmpty(n.UserId)))
                .ToList();

            var count = 0;
            foreach (var notification in notifications)
            {
                notification.MarkAsRead();
                count++;
            }

            return Task.FromResult(count);
        }

        /// <inheritdoc />
        public Task<bool> DeleteAsync(int notificationId, string? userId = null)
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == notificationId && !n.IsDeleted);
            if (notification == null) return Task.FromResult(false);

            // ユーザー指定がある場合は権限をチェック
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(notification.UserId))
            {
                if (notification.UserId != userId)
                {
                    return Task.FromResult(false); // 権限なし
                }
            }

            notification.MarkAsDeleted();
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<int> CleanupExpiredAsync()
        {
            var expiredNotifications = _notifications
                .Where(n => !n.IsDeleted && n.IsExpired())
                .ToList();

            var count = 0;
            foreach (var notification in expiredNotifications)
            {
                notification.MarkAsDeleted();
                count++;
            }

            return Task.FromResult(count);
        }

        /// <inheritdoc />
        public Task<int> DeleteAllReadAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("ユーザーIDが指定されていません。", nameof(userId));

            var readNotifications = _notifications
                .Where(n => !n.IsDeleted && n.IsRead && 
                           (n.UserId == userId || string.IsNullOrEmpty(n.UserId)))
                .ToList();

            var count = 0;
            foreach (var notification in readNotifications)
            {
                notification.MarkAsDeleted();
                count++;
            }

            return Task.FromResult(count);
        }

        /// <inheritdoc />
        public Task<List<Notification>> SearchAsync(
            string? searchText = null,
            NotificationType? type = null,
            NotificationPriority? priority = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            string? userId = null,
            int pageSize = 50,
            int page = 1)
        {
            var query = _notifications.Where(n => !n.IsDeleted);

            // テキスト検索
            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(n => 
                    n.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    n.Message.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            // 種類フィルタ
            if (type.HasValue)
            {
                query = query.Where(n => n.Type == type.Value);
            }

            // 優先度フィルタ
            if (priority.HasValue)
            {
                query = query.Where(n => n.Priority == priority.Value);
            }

            // 日付フィルタ
            if (dateFrom.HasValue)
            {
                query = query.Where(n => n.CreatedAt >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(n => n.CreatedAt <= dateTo.Value);
            }

            // ユーザーフィルタ
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(n => n.UserId == userId || string.IsNullOrEmpty(n.UserId));
            }

            // ページング処理
            var result = query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<NotificationStatistics> GetStatisticsAsync(
            DateTime dateFrom,
            DateTime dateTo,
            string? userId = null)
        {
            var query = _notifications.Where(n => 
                !n.IsDeleted && 
                n.CreatedAt >= dateFrom && 
                n.CreatedAt <= dateTo);

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(n => n.UserId == userId || string.IsNullOrEmpty(n.UserId));
            }

            var notifications = query.ToList();

            var statistics = new NotificationStatistics
            {
                TotalCount = notifications.Count,
                UnreadCount = notifications.Count(n => !n.IsRead),
                CountByType = notifications
                    .GroupBy(n => n.Type)
                    .ToDictionary(g => g.Key, g => g.Count()),
                CountByPriority = notifications
                    .GroupBy(n => n.Priority)
                    .ToDictionary(g => g.Key, g => g.Count()),
                CountByDay = notifications
                    .GroupBy(n => n.CreatedAt.Date)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return Task.FromResult(statistics);
        }
    }
}
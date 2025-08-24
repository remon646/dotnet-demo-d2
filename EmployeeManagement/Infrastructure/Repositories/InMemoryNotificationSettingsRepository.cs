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
    /// 通知設定のインメモリリポジトリ実装
    /// </summary>
    public class InMemoryNotificationSettingsRepository : INotificationSettingsRepository
    {
        private readonly ConcurrentInMemoryDataStore _dataStore;
        private readonly string _collectionName = "NotificationSettings";
        private ConcurrentBag<NotificationSettings> _settings;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataStore">データストア</param>
        public InMemoryNotificationSettingsRepository(ConcurrentInMemoryDataStore dataStore)
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
                _dataStore.InitializeCollection<NotificationSettings>(_collectionName);
                CreateDefaultSettings();
            }

            _settings = _dataStore.GetCollection<NotificationSettings>(_collectionName);
        }

        /// <summary>
        /// デフォルト設定を作成します
        /// </summary>
        private void CreateDefaultSettings()
        {
            var settings = _dataStore.GetCollection<NotificationSettings>(_collectionName);

            // 管理者ユーザーのデフォルト設定
            var adminSettings = new NotificationSettings
            {
                UserId = "admin",
                IsEnabled = true,
                PlaySound = true,
                ShowDesktopNotification = true,
                AutoDismissSeconds = 5,
                MinimumPriority = NotificationPriority.Low,
                MaxDisplayCount = 10,
                QuietHoursEnabled = true,
                QuietHoursStart = new TimeSpan(22, 0, 0),
                QuietHoursEnd = new TimeSpan(8, 0, 0),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            settings.Add(adminSettings);
        }

        /// <inheritdoc />
        public Task<NotificationSettings> GetByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("ユーザーIDが指定されていません。", nameof(userId));

            var userSettings = _settings.FirstOrDefault(s => s.UserId == userId);
            
            // 設定が存在しない場合はデフォルト設定を作成
            if (userSettings == null)
            {
                userSettings = new NotificationSettings
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _settings.Add(userSettings);
            }

            return Task.FromResult(userSettings);
        }

        /// <inheritdoc />
        public Task<bool> SaveAsync(NotificationSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrEmpty(settings.UserId)) throw new ArgumentException("ユーザーIDが指定されていません。");

            var existing = _settings.FirstOrDefault(s => s.UserId == settings.UserId);
            
            if (existing != null)
            {
                // 既存設定を更新
                settings.UpdatedAt = DateTime.Now;
                settings.CreatedAt = existing.CreatedAt; // 作成日時は保持

                var updatedList = _settings.Where(s => s.UserId != settings.UserId).ToList();
                updatedList.Add(settings);

                _dataStore.ReplaceCollection(_collectionName, new ConcurrentBag<NotificationSettings>(updatedList));
                _settings = _dataStore.GetCollection<NotificationSettings>(_collectionName);
            }
            else
            {
                // 新規作成
                settings.CreatedAt = DateTime.Now;
                settings.UpdatedAt = DateTime.Now;
                _settings.Add(settings);
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<List<NotificationSettings>> GetAllAsync()
        {
            var result = _settings.OrderBy(s => s.UserId).ToList();
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<bool> DeleteAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("ユーザーIDが指定されていません。", nameof(userId));

            var existing = _settings.FirstOrDefault(s => s.UserId == userId);
            if (existing == null) return Task.FromResult(false);

            var updatedList = _settings.Where(s => s.UserId != userId).ToList();
            _dataStore.ReplaceCollection(_collectionName, new ConcurrentBag<NotificationSettings>(updatedList));
            _settings = _dataStore.GetCollection<NotificationSettings>(_collectionName);

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<NotificationSettings> GetDefaultSettingsAsync()
        {
            var defaultSettings = new NotificationSettings
            {
                UserId = string.Empty, // デフォルト設定なのでユーザーIDは空
                IsEnabled = true,
                PlaySound = true,
                ShowDesktopNotification = true,
                AutoDismissSeconds = 5,
                MinimumPriority = NotificationPriority.Low,
                MaxDisplayCount = 10,
                QuietHoursEnabled = false,
                QuietHoursStart = new TimeSpan(22, 0, 0),
                QuietHoursEnd = new TimeSpan(8, 0, 0),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            return Task.FromResult(defaultSettings);
        }

        /// <inheritdoc />
        public Task<bool> ResetToDefaultAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("ユーザーIDが指定されていません。", nameof(userId));

            var existing = _settings.FirstOrDefault(s => s.UserId == userId);
            
            if (existing != null)
            {
                // 既存設定をデフォルトにリセット
                var createdAt = existing.CreatedAt;
                existing.ResetToDefault();
                existing.UserId = userId; // ユーザーIDを復元
                existing.CreatedAt = createdAt; // 作成日時を復元
                existing.UpdatedAt = DateTime.Now;

                var updatedList = _settings.Where(s => s.UserId != userId).ToList();
                updatedList.Add(existing);

                _dataStore.ReplaceCollection(_collectionName, new ConcurrentBag<NotificationSettings>(updatedList));
                _settings = _dataStore.GetCollection<NotificationSettings>(_collectionName);
            }
            else
            {
                // 新規作成（デフォルト設定）
                var newSettings = new NotificationSettings
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _settings.Add(newSettings);
            }

            return Task.FromResult(true);
        }
    }
}
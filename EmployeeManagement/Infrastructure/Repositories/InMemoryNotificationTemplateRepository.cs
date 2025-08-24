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
    /// 通知テンプレートのインメモリリポジトリ実装
    /// </summary>
    public class InMemoryNotificationTemplateRepository : INotificationTemplateRepository
    {
        private readonly ConcurrentInMemoryDataStore _dataStore;
        private readonly string _collectionName = "NotificationTemplates";
        private ConcurrentBag<NotificationTemplate> _templates;
        private int _nextId = 1;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataStore">データストア</param>
        public InMemoryNotificationTemplateRepository(ConcurrentInMemoryDataStore dataStore)
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
                _dataStore.InitializeCollection<NotificationTemplate>(_collectionName);
                CreateSystemTemplates();
            }

            _templates = _dataStore.GetCollection<NotificationTemplate>(_collectionName);
            
            // 次のIDを設定
            var maxId = _templates.Any() ? _templates.Max(t => t.Id) : 0;
            _nextId = maxId + 1;
        }

        /// <summary>
        /// システム定義テンプレートを作成します
        /// </summary>
        private void CreateSystemTemplates()
        {
            var systemTemplates = new[]
            {
                new NotificationTemplate
                {
                    Id = 1,
                    Name = "EmployeeCreated",
                    Description = "社員作成通知",
                    TitleTemplate = "新しい社員が登録されました",
                    MessageTemplate = "{EmployeeName} さんが {DepartmentName} に登録されました。",
                    Type = NotificationType.DataChange,
                    Priority = NotificationPriority.Normal,
                    IsActive = true,
                    Icon = "person_add",
                    ActionUrlTemplate = "/employees/{EmployeeId}",
                    ActionText = "詳細を見る",
                    Category = "Employee",
                    IsSystemTemplate = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                },
                new NotificationTemplate
                {
                    Id = 2,
                    Name = "EmployeeUpdated",
                    Description = "社員更新通知",
                    TitleTemplate = "社員情報が更新されました",
                    MessageTemplate = "{EmployeeName} さんの情報が更新されました。",
                    Type = NotificationType.DataChange,
                    Priority = NotificationPriority.Normal,
                    IsActive = true,
                    Icon = "person",
                    ActionUrlTemplate = "/employees/{EmployeeId}",
                    ActionText = "詳細を見る",
                    Category = "Employee",
                    IsSystemTemplate = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                },
                new NotificationTemplate
                {
                    Id = 3,
                    Name = "EmployeeDeleted",
                    Description = "社員削除通知",
                    TitleTemplate = "社員が削除されました",
                    MessageTemplate = "{EmployeeName} さんが削除されました。",
                    Type = NotificationType.DataChange,
                    Priority = NotificationPriority.High,
                    IsActive = true,
                    Icon = "person_remove",
                    ActionUrlTemplate = "/employees",
                    ActionText = "社員一覧を見る",
                    Category = "Employee",
                    IsSystemTemplate = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                },
                new NotificationTemplate
                {
                    Id = 4,
                    Name = "DepartmentCreated",
                    Description = "部署作成通知",
                    TitleTemplate = "新しい部署が作成されました",
                    MessageTemplate = "{DepartmentName} 部署が作成されました。",
                    Type = NotificationType.DataChange,
                    Priority = NotificationPriority.Normal,
                    IsActive = true,
                    Icon = "domain",
                    ActionUrlTemplate = "/departments/{DepartmentId}",
                    ActionText = "詳細を見る",
                    Category = "Department",
                    IsSystemTemplate = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                },
                new NotificationTemplate
                {
                    Id = 5,
                    Name = "SystemMaintenance",
                    Description = "システムメンテナンス通知",
                    TitleTemplate = "システムメンテナンスのお知らせ",
                    MessageTemplate = "{MaintenanceDate} にシステムメンテナンスを実施いたします。メンテナンス時間: {StartTime} - {EndTime}",
                    Type = NotificationType.System,
                    Priority = NotificationPriority.High,
                    IsActive = true,
                    Icon = "construction",
                    ActionUrlTemplate = "/maintenance",
                    ActionText = "詳細を見る",
                    Category = "System",
                    IsSystemTemplate = true,
                    ExpirationDays = 7,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                },
                new NotificationTemplate
                {
                    Id = 6,
                    Name = "LoginSuccess",
                    Description = "ログイン成功通知",
                    TitleTemplate = "ログイン成功",
                    MessageTemplate = "{UserName} さんがシステムにログインしました。IP: {IpAddress}",
                    Type = NotificationType.UserAction,
                    Priority = NotificationPriority.Low,
                    IsActive = true,
                    Icon = "login",
                    Category = "Authentication",
                    IsSystemTemplate = true,
                    ExpirationDays = 1,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                },
                new NotificationTemplate
                {
                    Id = 7,
                    Name = "ErrorOccurred",
                    Description = "エラー発生通知",
                    TitleTemplate = "システムエラーが発生しました",
                    MessageTemplate = "エラーが発生しました: {ErrorMessage}",
                    Type = NotificationType.Error,
                    Priority = NotificationPriority.Critical,
                    IsActive = true,
                    Icon = "error",
                    ActionUrlTemplate = "/logs",
                    ActionText = "ログを確認",
                    Category = "System",
                    IsSystemTemplate = true,
                    ExpirationDays = 30,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                }
            };

            var templates = _dataStore.GetCollection<NotificationTemplate>(_collectionName);
            foreach (var template in systemTemplates)
            {
                // パラメーター情報を追加
                AddParametersToTemplate(template);
                templates.Add(template);
            }
        }

        /// <summary>
        /// テンプレートにパラメーター情報を追加します
        /// </summary>
        private void AddParametersToTemplate(NotificationTemplate template)
        {
            switch (template.Name)
            {
                case "EmployeeCreated":
                case "EmployeeUpdated":
                case "EmployeeDeleted":
                    template.Parameters = new List<NotificationTemplateParameter>
                    {
                        new() { Name = "EmployeeName", Description = "社員名", IsRequired = true },
                        new() { Name = "EmployeeId", Description = "社員ID", IsRequired = true },
                        new() { Name = "DepartmentName", Description = "部署名", IsRequired = false }
                    };
                    break;

                case "DepartmentCreated":
                    template.Parameters = new List<NotificationTemplateParameter>
                    {
                        new() { Name = "DepartmentName", Description = "部署名", IsRequired = true },
                        new() { Name = "DepartmentId", Description = "部署ID", IsRequired = true }
                    };
                    break;

                case "SystemMaintenance":
                    template.Parameters = new List<NotificationTemplateParameter>
                    {
                        new() { Name = "MaintenanceDate", Description = "メンテナンス日", IsRequired = true },
                        new() { Name = "StartTime", Description = "開始時刻", IsRequired = true },
                        new() { Name = "EndTime", Description = "終了時刻", IsRequired = true }
                    };
                    break;

                case "LoginSuccess":
                    template.Parameters = new List<NotificationTemplateParameter>
                    {
                        new() { Name = "UserName", Description = "ユーザー名", IsRequired = true },
                        new() { Name = "IpAddress", Description = "IPアドレス", IsRequired = false, DefaultValue = "不明" }
                    };
                    break;

                case "ErrorOccurred":
                    template.Parameters = new List<NotificationTemplateParameter>
                    {
                        new() { Name = "ErrorMessage", Description = "エラーメッセージ", IsRequired = true }
                    };
                    break;
            }
        }

        /// <inheritdoc />
        public Task<int> CreateAsync(NotificationTemplate template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            template.Id = _nextId++;
            template.CreatedAt = DateTime.Now;
            template.UpdatedAt = DateTime.Now;

            _templates.Add(template);
            return Task.FromResult(template.Id);
        }

        /// <inheritdoc />
        public Task<bool> UpdateAsync(NotificationTemplate template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            var existing = _templates.FirstOrDefault(t => t.Id == template.Id);
            if (existing == null) return Task.FromResult(false);

            template.UpdatedAt = DateTime.Now;

            // 既存のテンプレートを削除して新しいものを追加
            var updatedList = _templates.Where(t => t.Id != template.Id).ToList();
            updatedList.Add(template);

            _dataStore.ReplaceCollection(_collectionName, new ConcurrentBag<NotificationTemplate>(updatedList));
            _templates = _dataStore.GetCollection<NotificationTemplate>(_collectionName);

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<NotificationTemplate?> GetByIdAsync(int id)
        {
            var template = _templates.FirstOrDefault(t => t.Id == id);
            return Task.FromResult(template);
        }

        /// <inheritdoc />
        public Task<NotificationTemplate?> GetByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("テンプレート名が指定されていません。", nameof(name));

            var template = _templates.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(template);
        }

        /// <inheritdoc />
        public Task<List<NotificationTemplate>> GetAllAsync(bool includeInactive = false, string? category = null)
        {
            var query = _templates.AsEnumerable();

            if (!includeInactive)
            {
                query = query.Where(t => t.IsActive);
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            var result = query.OrderBy(t => t.Name).ToList();
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<List<NotificationTemplate>> GetActiveByTypeAsync(NotificationType type)
        {
            var result = _templates
                .Where(t => t.IsActive && t.Type == type)
                .OrderBy(t => t.Name)
                .ToList();

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<List<NotificationTemplate>> GetSystemTemplatesAsync()
        {
            var result = _templates
                .Where(t => t.IsSystemTemplate)
                .OrderBy(t => t.Name)
                .ToList();

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<bool> DeleteAsync(int id)
        {
            var template = _templates.FirstOrDefault(t => t.Id == id);
            if (template == null) return Task.FromResult(false);

            // システムテンプレートは削除不可
            if (template.IsSystemTemplate) return Task.FromResult(false);

            var updatedList = _templates.Where(t => t.Id != id).ToList();
            _dataStore.ReplaceCollection(_collectionName, new ConcurrentBag<NotificationTemplate>(updatedList));
            _templates = _dataStore.GetCollection<NotificationTemplate>(_collectionName);

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> ExistsAsync(string name, int? excludeId = null)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("テンプレート名が指定されていません。", nameof(name));

            var exists = _templates.Any(t => 
                t.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                (!excludeId.HasValue || t.Id != excludeId.Value));

            return Task.FromResult(exists);
        }

        /// <inheritdoc />
        public Task<int> CloneAsync(int sourceId, string newName)
        {
            if (string.IsNullOrEmpty(newName)) throw new ArgumentException("新しいテンプレート名が指定されていません。", nameof(newName));

            var source = _templates.FirstOrDefault(t => t.Id == sourceId);
            if (source == null) throw new ArgumentException("コピー元のテンプレートが見つかりません。", nameof(sourceId));

            var clone = source.Clone();
            clone.Name = newName;
            clone.Id = _nextId++;
            clone.CreatedAt = DateTime.Now;
            clone.UpdatedAt = DateTime.Now;

            _templates.Add(clone);
            return Task.FromResult(clone.Id);
        }
    }
}
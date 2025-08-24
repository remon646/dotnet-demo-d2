using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;

namespace EmployeeManagement.Application.Services
{
    /// <summary>
    /// 監査ログサービスの実装
    /// 監査ログの記録と取得に関するビジネスロジックを提供します
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// AuditLogServiceの新しいインスタンスを初期化します
        /// </summary>
        /// <param name="auditLogRepository">監査ログリポジトリ</param>
        public AuditLogService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
            
            // JSON シリアライズ設定
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        /// <inheritdoc/>
        public async Task<AuditLog> LogAsync(
            string userId,
            string userName,
            string action,
            string entityType,
            string entityId,
            string? oldValues = null,
            string? newValues = null,
            string? ipAddress = null,
            AuditLogLevel level = AuditLogLevel.Info,
            string? details = null)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("UserId cannot be null or empty", nameof(userId));
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentException("UserName cannot be null or empty", nameof(userName));
            if (string.IsNullOrEmpty(action))
                throw new ArgumentException("Action cannot be null or empty", nameof(action));
            if (string.IsNullOrEmpty(entityType))
                throw new ArgumentException("EntityType cannot be null or empty", nameof(entityType));
            if (string.IsNullOrEmpty(entityId))
                throw new ArgumentException("EntityId cannot be null or empty", nameof(entityId));

            var auditLog = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues ?? string.Empty,
                NewValues = newValues ?? string.Empty,
                IpAddress = ipAddress ?? string.Empty,
                Level = level,
                Details = details ?? string.Empty,
                Timestamp = DateTime.Now
            };

            return await _auditLogRepository.AddAsync(auditLog);
        }

        /// <inheritdoc/>
        public async Task<AuditLog> LogCreateAsync<T>(string userId, string userName, T entity, string? ipAddress = null)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entityType = typeof(T).Name;
            var entityId = GetEntityId(entity);
            var newValues = SerializeEntity(entity);

            return await LogAsync(
                userId,
                userName,
                "Create",
                entityType,
                entityId,
                null,
                newValues,
                ipAddress,
                AuditLogLevel.Info,
                $"{entityType}を作成しました");
        }

        /// <inheritdoc/>
        public async Task<AuditLog> LogUpdateAsync<T>(string userId, string userName, T oldEntity, T newEntity, string? ipAddress = null)
        {
            if (oldEntity == null)
                throw new ArgumentNullException(nameof(oldEntity));
            if (newEntity == null)
                throw new ArgumentNullException(nameof(newEntity));

            var entityType = typeof(T).Name;
            var entityId = GetEntityId(newEntity);
            var oldValues = SerializeEntity(oldEntity);
            var newValues = SerializeEntity(newEntity);

            // 変更差分を計算
            var changes = CalculateChanges(oldEntity, newEntity);
            var details = changes.Any() ? $"変更項目: {string.Join(", ", changes)}" : "変更なし";

            return await LogAsync(
                userId,
                userName,
                "Update",
                entityType,
                entityId,
                oldValues,
                newValues,
                ipAddress,
                AuditLogLevel.Info,
                details);
        }

        /// <inheritdoc/>
        public async Task<AuditLog> LogDeleteAsync<T>(string userId, string userName, T entity, string? ipAddress = null)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entityType = typeof(T).Name;
            var entityId = GetEntityId(entity);
            var oldValues = SerializeEntity(entity);

            return await LogAsync(
                userId,
                userName,
                "Delete",
                entityType,
                entityId,
                oldValues,
                null,
                ipAddress,
                AuditLogLevel.Warning,
                $"{entityType}を削除しました");
        }

        /// <inheritdoc/>
        public async Task<AuditLog> LogLoginAsync(string userId, string userName, string? ipAddress = null, bool isSuccess = true)
        {
            var level = isSuccess ? AuditLogLevel.Info : AuditLogLevel.Warning;
            var details = isSuccess ? "ログインに成功しました" : "ログインに失敗しました";

            return await LogAsync(
                userId,
                userName,
                "Login",
                "User",
                userId,
                null,
                null,
                ipAddress,
                level,
                details);
        }

        /// <inheritdoc/>
        public async Task<AuditLog> LogLogoutAsync(string userId, string userName, string? ipAddress = null)
        {
            return await LogAsync(
                userId,
                userName,
                "Logout",
                "User",
                userId,
                null,
                null,
                ipAddress,
                AuditLogLevel.Info,
                "ログアウトしました");
        }

        /// <inheritdoc/>
        public async Task<AuditLog?> GetAuditLogAsync(int id)
        {
            return await _auditLogRepository.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync()
        {
            return await _auditLogRepository.GetAllAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AuditLog>> SearchAuditLogsAsync(
            string? userId = null,
            string? action = null,
            string? entityType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            AuditLogLevel? level = null)
        {
            return await _auditLogRepository.SearchAsync(userId, action, entityType, fromDate, toDate, level);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AuditLog>> GetEntityAuditLogsAsync(string entityType, string entityId)
        {
            return await _auditLogRepository.GetByEntityAsync(entityType, entityId);
        }

        /// <inheritdoc/>
        public async Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedAuditLogsAsync(int page, int pageSize)
        {
            if (page < 1)
                throw new ArgumentException("Page must be greater than 0", nameof(page));
            if (pageSize < 1)
                throw new ArgumentException("PageSize must be greater than 0", nameof(pageSize));

            var totalCount = await _auditLogRepository.GetCountAsync();
            var skip = (page - 1) * pageSize;
            var logs = await _auditLogRepository.GetPagedAsync(skip, pageSize);

            return (logs, totalCount);
        }

        /// <inheritdoc/>
        public async Task<int> CleanupOldLogsAsync(int retentionDays)
        {
            if (retentionDays < 0)
                throw new ArgumentException("Retention days must be greater than or equal to 0", nameof(retentionDays));

            var cutoffDate = DateTime.Now.AddDays(-retentionDays);
            return await _auditLogRepository.DeleteOlderThanAsync(cutoffDate);
        }

        /// <summary>
        /// エンティティをJSON形式でシリアライズします
        /// </summary>
        /// <param name="entity">シリアライズするエンティティ</param>
        /// <returns>JSON文字列</returns>
        private string SerializeEntity<T>(T entity)
        {
            try
            {
                return JsonSerializer.Serialize(entity, _jsonOptions);
            }
            catch (Exception)
            {
                // シリアライズに失敗した場合はToString()を使用
                return entity?.ToString() ?? string.Empty;
            }
        }

        /// <summary>
        /// エンティティのIDを取得します
        /// </summary>
        /// <param name="entity">エンティティ</param>
        /// <returns>エンティティID</returns>
        private string GetEntityId<T>(T entity)
        {
            if (entity == null)
                return string.Empty;

            // リフレクションを使用してIdプロパティを取得
            var type = typeof(T);
            var idProperty = type.GetProperty("Id") ?? 
                           type.GetProperty("ID") ?? 
                           type.GetProperty("EmployeeId") ??
                           type.GetProperty("DepartmentId");

            if (idProperty != null)
            {
                var value = idProperty.GetValue(entity);
                return value?.ToString() ?? string.Empty;
            }

            // IDプロパティが見つからない場合はToString()を使用
            return entity.ToString() ?? string.Empty;
        }

        /// <summary>
        /// 2つのエンティティの変更差分を計算します
        /// </summary>
        /// <param name="oldEntity">変更前のエンティティ</param>
        /// <param name="newEntity">変更後のエンティティ</param>
        /// <returns>変更されたプロパティ名のリスト</returns>
        private List<string> CalculateChanges<T>(T oldEntity, T newEntity)
        {
            var changes = new List<string>();
            
            if (oldEntity == null || newEntity == null)
                return changes;

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                try
                {
                    var oldValue = property.GetValue(oldEntity);
                    var newValue = property.GetValue(newEntity);

                    if (!Equals(oldValue, newValue))
                    {
                        changes.Add(property.Name);
                    }
                }
                catch
                {
                    // プロパティの取得に失敗した場合は無視
                }
            }

            return changes;
        }
    }
}
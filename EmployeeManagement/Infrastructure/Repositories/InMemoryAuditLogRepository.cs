using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Infrastructure.DataStores;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Infrastructure.Repositories
{
    /// <summary>
    /// インメモリ監査ログリポジトリの実装
    /// ConcurrentInMemoryDataStoreを使用してスレッドセーフな操作を提供します
    /// </summary>
    public class InMemoryAuditLogRepository : IAuditLogRepository
    {
        private readonly ConcurrentInMemoryDataStore _dataStore;
        private const string AuditLogKey = "AuditLogs";
        private int _nextId = 1;

        /// <summary>
        /// InMemoryAuditLogRepositoryの新しいインスタンスを初期化します
        /// </summary>
        /// <param name="dataStore">データストア</param>
        public InMemoryAuditLogRepository(ConcurrentInMemoryDataStore dataStore)
        {
            _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
            
            // 監査ログコレクションが存在しない場合は初期化
            if (!_dataStore.HasCollection(AuditLogKey))
            {
                _dataStore.InitializeCollection<AuditLog>(AuditLogKey);
            }
        }

        /// <inheritdoc/>
        public async Task<AuditLog> AddAsync(AuditLog auditLog)
        {
            if (auditLog == null)
                throw new ArgumentNullException(nameof(auditLog));

            // 新しいIDを割り当て
            auditLog.Id = _nextId++;
            auditLog.Timestamp = DateTime.Now;

            var auditLogs = _dataStore.GetCollection<AuditLog>(AuditLogKey);
            auditLogs.Add(auditLog);

            return await Task.FromResult(auditLog);
        }

        /// <inheritdoc/>
        public async Task<AuditLog?> GetByIdAsync(int id)
        {
            var auditLogs = _dataStore.GetCollection<AuditLog>(AuditLogKey);
            var auditLog = auditLogs.FirstOrDefault(a => a.Id == id);
            return await Task.FromResult(auditLog);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AuditLog>> GetAllAsync()
        {
            var auditLogs = _dataStore.GetCollection<AuditLog>(AuditLogKey);
            // 新しい順（Timestampの降順）でソート
            var sortedLogs = auditLogs.OrderByDescending(a => a.Timestamp).ToList();
            return await Task.FromResult(sortedLogs);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AuditLog>> SearchAsync(
            string? userId = null,
            string? action = null,
            string? entityType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            AuditLogLevel? level = null)
        {
            var auditLogs = _dataStore.GetCollection<AuditLog>(AuditLogKey);
            var query = auditLogs.AsQueryable();

            // 各条件でフィルタリング
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(a => a.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(a => a.Action.Equals(action, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(entityType))
            {
                query = query.Where(a => a.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= toDate.Value);
            }

            if (level.HasValue)
            {
                query = query.Where(a => a.Level == level.Value);
            }

            // 新しい順でソート
            var result = query.OrderByDescending(a => a.Timestamp).ToList();
            return await Task.FromResult(result);
        }

        /// <inheritdoc/>
        public async Task<int> DeleteOlderThanAsync(DateTime olderThan)
        {
            var auditLogs = _dataStore.GetCollection<AuditLog>(AuditLogKey);
            var allLogs = auditLogs.ToList();
            var logsToDelete = allLogs.Where(a => a.Timestamp < olderThan).ToList();
            
            // 新しいコレクションを作成して古いログを除外
            var newCollection = new ConcurrentBag<AuditLog>(allLogs.Except(logsToDelete));
            
            // コレクションを置き換え
            _dataStore.ReplaceCollection(AuditLogKey, newCollection);

            return await Task.FromResult(logsToDelete.Count);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId)
        {
            var auditLogs = _dataStore.GetCollection<AuditLog>(AuditLogKey);
            var entityLogs = auditLogs
                .Where(a => a.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase) &&
                           a.EntityId.Equals(entityId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(a => a.Timestamp)
                .ToList();
            
            return await Task.FromResult(entityLogs);
        }

        /// <inheritdoc/>
        public async Task<int> GetCountAsync()
        {
            var auditLogs = _dataStore.GetCollection<AuditLog>(AuditLogKey);
            return await Task.FromResult(auditLogs.Count);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AuditLog>> GetPagedAsync(int skip, int take)
        {
            var auditLogs = _dataStore.GetCollection<AuditLog>(AuditLogKey);
            var pagedLogs = auditLogs
                .OrderByDescending(a => a.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToList();
            
            return await Task.FromResult(pagedLogs);
        }
    }
}
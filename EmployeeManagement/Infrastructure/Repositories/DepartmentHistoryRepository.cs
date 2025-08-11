using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Infrastructure.DataStores;
using System.Collections.Concurrent;

namespace EmployeeManagement.Infrastructure.Repositories;

/// <summary>
/// 部門履歴リポジトリ実装
/// </summary>
public class DepartmentHistoryRepository : IDepartmentHistoryRepository
{
    private readonly ConcurrentInMemoryDataStore _dataStore;
    private readonly ConcurrentDictionary<string, DepartmentHistory> _departmentHistories;

    public DepartmentHistoryRepository(ConcurrentInMemoryDataStore dataStore)
    {
        _dataStore = dataStore;
        _departmentHistories = _dataStore.GetOrCreateCollection<DepartmentHistory>("DepartmentHistories");
        
        // デモデータの初期化
        InitializeDemoData();
    }

    public async Task<IEnumerable<DepartmentHistory>> GetAllAsync()
    {
        return await Task.FromResult(_departmentHistories.Values.OrderBy(h => h.EmployeeNumber).ThenBy(h => h.StartDate));
    }

    public async Task<DepartmentHistory?> GetByIdAsync(string historyId)
    {
        _departmentHistories.TryGetValue(historyId, out var history);
        return await Task.FromResult(history);
    }

    public async Task<IEnumerable<DepartmentHistory>> GetByEmployeeNumberAsync(string employeeNumber)
    {
        var result = _departmentHistories.Values
            .Where(h => h.EmployeeNumber == employeeNumber)
            .OrderBy(h => h.StartDate);
        return await Task.FromResult(result);
    }

    public async Task<DepartmentHistory?> GetCurrentByEmployeeNumberAsync(string employeeNumber)
    {
        var current = _departmentHistories.Values
            .Where(h => h.EmployeeNumber == employeeNumber && h.EndDate == null)
            .FirstOrDefault();
        return await Task.FromResult(current);
    }

    public async Task<IEnumerable<DepartmentHistory>> GetByDepartmentAsync(Department department)
    {
        var result = _departmentHistories.Values
            .Where(h => h.Department == department)
            .OrderBy(h => h.StartDate);
        return await Task.FromResult(result);
    }

    public async Task<IEnumerable<DepartmentHistory>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
    {
        var result = _departmentHistories.Values
            .Where(h => h.StartDate <= endDate && (h.EndDate == null || h.EndDate >= startDate))
            .OrderBy(h => h.StartDate);
        return await Task.FromResult(result);
    }

    public async Task<DepartmentHistory> AddAsync(DepartmentHistory departmentHistory)
    {
        if (string.IsNullOrEmpty(departmentHistory.HistoryId))
        {
            departmentHistory.HistoryId = Guid.NewGuid().ToString();
        }

        departmentHistory.CreatedAt = DateTime.Now;
        departmentHistory.UpdatedAt = DateTime.Now;
        
        _departmentHistories.TryAdd(departmentHistory.HistoryId, departmentHistory);
        return departmentHistory;
    }

    public async Task<DepartmentHistory> UpdateAsync(DepartmentHistory departmentHistory)
    {
        if (!_departmentHistories.ContainsKey(departmentHistory.HistoryId))
        {
            throw new InvalidOperationException($"履歴ID '{departmentHistory.HistoryId}' が見つかりません。");
        }

        departmentHistory.UpdatedAt = DateTime.Now;
        _departmentHistories.TryUpdate(departmentHistory.HistoryId, departmentHistory, _departmentHistories[departmentHistory.HistoryId]);
        return departmentHistory;
    }

    public async Task<bool> DeleteAsync(string historyId)
    {
        return await Task.FromResult(_departmentHistories.TryRemove(historyId, out _));
    }

    public async Task<DepartmentHistory> TransferDepartmentAsync(string employeeNumber, Department newDepartment, Position newPosition, DateTime transferDate, string? reason = null)
    {
        // 現在の履歴を終了
        var currentHistory = await GetCurrentByEmployeeNumberAsync(employeeNumber);
        if (currentHistory != null)
        {
            currentHistory.EndDate = transferDate.Date;
            currentHistory.UpdatedAt = DateTime.Now;
            await UpdateAsync(currentHistory);
        }

        // 新しい履歴を作成
        var newHistory = new DepartmentHistory
        {
            EmployeeNumber = employeeNumber,
            Department = newDepartment,
            Position = newPosition,
            StartDate = transferDate.Date,
            EndDate = null,
            TransferReason = reason
        };

        return await AddAsync(newHistory);
    }

    public async Task<int> GetCurrentEmployeeCountByDepartmentAsync(Department department)
    {
        var count = _departmentHistories.Values
            .Count(h => h.Department == department && h.EndDate == null);
        return await Task.FromResult(count);
    }

    private void InitializeDemoData()
    {
        if (_departmentHistories.Any()) return;

        var demoHistories = new[]
        {
            new DepartmentHistory
            {
                HistoryId = Guid.NewGuid().ToString(),
                EmployeeNumber = "EMP2024001",
                Department = Department.Sales,
                Position = Position.Manager,
                StartDate = new DateTime(2024, 4, 1),
                EndDate = null,
                TransferReason = "新卒入社",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }
        };

        foreach (var history in demoHistories)
        {
            _departmentHistories.TryAdd(history.HistoryId, history);
        }
    }
}
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Infrastructure.DataStores;
using System.Collections.Concurrent;

namespace EmployeeManagement.Infrastructure.Repositories;

/// <summary>
/// 社員番号リポジトリ実装
/// </summary>
public class EmployeeNumberRepository : IEmployeeNumberRepository
{
    private readonly ConcurrentInMemoryDataStore _dataStore;
    private readonly ConcurrentDictionary<string, EmployeeNumber> _employeeNumbers;

    public EmployeeNumberRepository(ConcurrentInMemoryDataStore dataStore)
    {
        _dataStore = dataStore;
        _employeeNumbers = _dataStore.GetOrCreateCollection<EmployeeNumber>("EmployeeNumbers");
        
        // デモデータの初期化
        InitializeDemoData();
    }

    public async Task<IEnumerable<EmployeeNumber>> GetAllAsync()
    {
        return await Task.FromResult(_employeeNumbers.Values.OrderBy(e => e.Number));
    }

    public async Task<EmployeeNumber?> GetByNumberAsync(string number)
    {
        _employeeNumbers.TryGetValue(number, out var employeeNumber);
        return await Task.FromResult(employeeNumber);
    }

    public async Task<IEnumerable<EmployeeNumber>> GetByYearAsync(int year)
    {
        var result = _employeeNumbers.Values
            .Where(e => e.IssueYear == year)
            .OrderBy(e => e.SequenceNumber);
        return await Task.FromResult(result);
    }

    public async Task<string> GetNextAvailableNumberAsync(int year)
    {
        var yearNumbers = await GetByYearAsync(year);
        
        // Only consider active and reserved numbers (not deactivated)
        var activeNumbers = yearNumbers.Where(e => e.Status != EmployeeNumberStatus.Deactivated);
        var maxSequence = activeNumbers.Any() 
            ? activeNumbers.Max(e => e.SequenceNumber) 
            : 0;
        
        var nextSequence = maxSequence + 1;
        return $"EMP{year}{nextSequence:D3}";
    }

    public async Task<EmployeeNumber> AddAsync(EmployeeNumber employeeNumber)
    {
        if (await ExistsAsync(employeeNumber.Number))
        {
            throw new InvalidOperationException($"社員番号 '{employeeNumber.Number}' は既に存在します。");
        }

        employeeNumber.CreatedAt = DateTime.Now;
        employeeNumber.UpdatedAt = DateTime.Now;
        
        _employeeNumbers.TryAdd(employeeNumber.Number, employeeNumber);
        return employeeNumber;
    }

    public async Task<EmployeeNumber> UpdateAsync(EmployeeNumber employeeNumber)
    {
        if (!await ExistsAsync(employeeNumber.Number))
        {
            throw new InvalidOperationException($"社員番号 '{employeeNumber.Number}' が見つかりません。");
        }

        employeeNumber.UpdatedAt = DateTime.Now;
        _employeeNumbers.TryUpdate(employeeNumber.Number, employeeNumber, _employeeNumbers[employeeNumber.Number]);
        return employeeNumber;
    }

    public async Task<bool> DeactivateAsync(string number)
    {
        var employeeNumber = await GetByNumberAsync(number);
        if (employeeNumber == null) return false;

        employeeNumber.IsActive = false;
        employeeNumber.UpdatedAt = DateTime.Now;
        
        return _employeeNumbers.TryUpdate(number, employeeNumber, _employeeNumbers[number]);
    }

    public async Task<bool> ExistsAsync(string number)
    {
        return await Task.FromResult(_employeeNumbers.ContainsKey(number));
    }

    public async Task<int> GetUsedCountByYearAsync(int year)
    {
        var yearNumbers = await GetByYearAsync(year);
        return yearNumbers.Count();
    }

    public Task<bool> DeleteAsync(EmployeeNumber employeeNumber)
    {
        if (employeeNumber == null || string.IsNullOrEmpty(employeeNumber.Number))
            return Task.FromResult(false);

        var removed = _employeeNumbers.TryRemove(employeeNumber.Number, out _);
        return Task.FromResult(removed);
    }

    private void InitializeDemoData()
    {
        if (_employeeNumbers.Any()) return;

        var demoNumbers = new[]
        {
            new EmployeeNumber
            {
                Number = "EMP2024001",
                IssueYear = 2024,
                SequenceNumber = 1,
                IssuedAt = new DateTime(2024, 4, 1),
                IsActive = true,
                Remarks = "新卒入社"
            }
        };

        foreach (var number in demoNumbers)
        {
            _employeeNumbers.TryAdd(number.Number, number);
        }
    }
}
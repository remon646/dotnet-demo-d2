using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Application.Services;

/// <summary>
/// 部門履歴管理サービス
/// </summary>
public class DepartmentHistoryService
{
    private readonly IDepartmentHistoryRepository _departmentHistoryRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public DepartmentHistoryService(
        IDepartmentHistoryRepository departmentHistoryRepository,
        IEmployeeRepository employeeRepository)
    {
        _departmentHistoryRepository = departmentHistoryRepository;
        _employeeRepository = employeeRepository;
    }

    /// <summary>
    /// 社員の部門異動処理
    /// </summary>
    public async Task<DepartmentTransferResult> TransferEmployeeAsync(
        string employeeNumber, 
        Department newDepartment, 
        Position newPosition, 
        DateTime transferDate,
        string? reason = null)
    {
        // 社員の存在確認
        var employee = await _employeeRepository.GetByEmployeeNumberAsync(employeeNumber);
        if (employee == null)
        {
            return new DepartmentTransferResult
            {
                Success = false,
                ErrorMessage = $"社員番号 '{employeeNumber}' の社員が見つかりません。"
            };
        }

        // 現在の部門履歴を取得
        var currentHistory = await _departmentHistoryRepository.GetCurrentByEmployeeNumberAsync(employeeNumber);
        
        // 同じ部門・役職への異動チェック
        if (currentHistory != null && 
            currentHistory.Department == newDepartment && 
            currentHistory.Position == newPosition)
        {
            return new DepartmentTransferResult
            {
                Success = false,
                ErrorMessage = "現在と同じ部門・役職への異動はできません。"
            };
        }

        // 異動日の妥当性チェック
        if (currentHistory != null && transferDate < currentHistory.StartDate)
        {
            return new DepartmentTransferResult
            {
                Success = false,
                ErrorMessage = "異動日は現在の配属開始日以降である必要があります。"
            };
        }

        try
        {
            // 部門異動実行
            var newHistory = await _departmentHistoryRepository.TransferDepartmentAsync(
                employeeNumber, newDepartment, newPosition, transferDate, reason);

            // 社員情報の更新
            employee.CurrentDepartmentHistory = newHistory;
            await _employeeRepository.UpdateAsync(employee);

            return new DepartmentTransferResult
            {
                Success = true,
                NewHistory = newHistory,
                PreviousHistory = currentHistory
            };
        }
        catch (Exception ex)
        {
            return new DepartmentTransferResult
            {
                Success = false,
                ErrorMessage = $"異動処理中にエラーが発生しました: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// 社員の部門履歴を取得
    /// </summary>
    public async Task<IEnumerable<DepartmentHistory>> GetEmployeeHistoryAsync(string employeeNumber)
    {
        return await _departmentHistoryRepository.GetByEmployeeNumberAsync(employeeNumber);
    }

    /// <summary>
    /// 部門別の現在所属員数を取得
    /// </summary>
    public async Task<Dictionary<Department, int>> GetCurrentDepartmentCountsAsync()
    {
        var counts = new Dictionary<Department, int>();
        
        foreach (Department department in Enum.GetValues<Department>())
        {
            var count = await _departmentHistoryRepository.GetCurrentEmployeeCountByDepartmentAsync(department);
            counts[department] = count;
        }
        
        return counts;
    }

    /// <summary>
    /// 期間別の部門異動統計を取得
    /// </summary>
    public async Task<DepartmentTransferStatistics> GetTransferStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        var histories = await _departmentHistoryRepository.GetByPeriodAsync(startDate, endDate);
        var transfers = histories.Where(h => h.StartDate >= startDate && h.StartDate <= endDate);

        var statistics = new DepartmentTransferStatistics
        {
            Period = $"{startDate:yyyy/MM/dd} - {endDate:yyyy/MM/dd}",
            TotalTransfers = transfers.Count(),
            TransfersByDepartment = new Dictionary<Department, int>(),
            TransfersByPosition = new Dictionary<Position, int>()
        };

        foreach (Department department in Enum.GetValues<Department>())
        {
            statistics.TransfersByDepartment[department] = transfers.Count(t => t.Department == department);
        }

        foreach (Position position in Enum.GetValues<Position>())
        {
            statistics.TransfersByPosition[position] = transfers.Count(t => t.Position == position);
        }

        return statistics;
    }

    /// <summary>
    /// 履歴の整合性チェック
    /// </summary>
    public async Task<List<string>> ValidateHistoryIntegrityAsync(string employeeNumber)
    {
        var errors = new List<string>();
        var histories = await GetEmployeeHistoryAsync(employeeNumber);
        var sortedHistories = histories.OrderBy(h => h.StartDate).ToList();

        // 重複期間チェック
        for (int i = 0; i < sortedHistories.Count - 1; i++)
        {
            var current = sortedHistories[i];
            var next = sortedHistories[i + 1];

            if (current.EndDate == null)
            {
                errors.Add($"履歴 {current.HistoryId}: 最新以外の履歴に終了日が設定されていません。");
            }
            else if (current.EndDate >= next.StartDate)
            {
                errors.Add($"履歴 {current.HistoryId} と {next.HistoryId}: 期間が重複しています。");
            }
        }

        // 現在履歴の重複チェック
        var currentHistories = sortedHistories.Where(h => h.EndDate == null).ToList();
        if (currentHistories.Count > 1)
        {
            errors.Add("現在の履歴が複数存在します。");
        }

        return errors;
    }
}

/// <summary>
/// 部門異動結果
/// </summary>
public class DepartmentTransferResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DepartmentHistory? NewHistory { get; set; }
    public DepartmentHistory? PreviousHistory { get; set; }
}

/// <summary>
/// 部門異動統計
/// </summary>
public class DepartmentTransferStatistics
{
    public string Period { get; set; } = string.Empty;
    public int TotalTransfers { get; set; }
    public Dictionary<Department, int> TransfersByDepartment { get; set; } = new();
    public Dictionary<Position, int> TransfersByPosition { get; set; } = new();
}
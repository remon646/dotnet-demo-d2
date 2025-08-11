using EmployeeManagement.Domain.Models;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Domain.Interfaces;

/// <summary>
/// 部門履歴リポジトリインターフェース
/// </summary>
public interface IDepartmentHistoryRepository
{
    /// <summary>
    /// 全ての部門履歴を取得
    /// </summary>
    Task<IEnumerable<DepartmentHistory>> GetAllAsync();

    /// <summary>
    /// 履歴IDで取得
    /// </summary>
    Task<DepartmentHistory?> GetByIdAsync(string historyId);

    /// <summary>
    /// 社員番号の部門履歴を取得
    /// </summary>
    Task<IEnumerable<DepartmentHistory>> GetByEmployeeNumberAsync(string employeeNumber);

    /// <summary>
    /// 社員の現在の部門履歴を取得
    /// </summary>
    Task<DepartmentHistory?> GetCurrentByEmployeeNumberAsync(string employeeNumber);

    /// <summary>
    /// 部門別の履歴を取得
    /// </summary>
    Task<IEnumerable<DepartmentHistory>> GetByDepartmentAsync(Department department);

    /// <summary>
    /// 期間指定で履歴を取得
    /// </summary>
    Task<IEnumerable<DepartmentHistory>> GetByPeriodAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 部門履歴を追加
    /// </summary>
    Task<DepartmentHistory> AddAsync(DepartmentHistory departmentHistory);

    /// <summary>
    /// 部門履歴を更新
    /// </summary>
    Task<DepartmentHistory> UpdateAsync(DepartmentHistory departmentHistory);

    /// <summary>
    /// 部門履歴を削除
    /// </summary>
    Task<bool> DeleteAsync(string historyId);

    /// <summary>
    /// 社員の現在の履歴を終了し、新しい履歴を開始
    /// </summary>
    Task<DepartmentHistory> TransferDepartmentAsync(string employeeNumber, Department newDepartment, Position newPosition, DateTime transferDate, string? reason = null);

    /// <summary>
    /// 部門に所属中の社員数を取得
    /// </summary>
    Task<int> GetCurrentEmployeeCountByDepartmentAsync(Department department);
}
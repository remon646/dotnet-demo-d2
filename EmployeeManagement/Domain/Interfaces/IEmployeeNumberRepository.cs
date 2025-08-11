using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Domain.Interfaces;

/// <summary>
/// 社員番号リポジトリインターフェース
/// </summary>
public interface IEmployeeNumberRepository
{
    /// <summary>
    /// 全ての社員番号を取得
    /// </summary>
    Task<IEnumerable<EmployeeNumber>> GetAllAsync();

    /// <summary>
    /// 社員番号で取得
    /// </summary>
    Task<EmployeeNumber?> GetByNumberAsync(string number);

    /// <summary>
    /// 年度別の社員番号一覧を取得
    /// </summary>
    Task<IEnumerable<EmployeeNumber>> GetByYearAsync(int year);

    /// <summary>
    /// 使用可能な次の番号を取得
    /// </summary>
    Task<string> GetNextAvailableNumberAsync(int year);

    /// <summary>
    /// 社員番号を追加
    /// </summary>
    Task<EmployeeNumber> AddAsync(EmployeeNumber employeeNumber);

    /// <summary>
    /// 社員番号を更新
    /// </summary>
    Task<EmployeeNumber> UpdateAsync(EmployeeNumber employeeNumber);

    /// <summary>
    /// 社員番号を廃番にする
    /// </summary>
    Task<bool> DeactivateAsync(string number);

    /// <summary>
    /// 番号の重複チェック
    /// </summary>
    Task<bool> ExistsAsync(string number);

    /// <summary>
    /// 年度別の使用済み番号数を取得
    /// </summary>
    Task<int> GetUsedCountByYearAsync(int year);

    /// <summary>
    /// 社員番号を削除
    /// </summary>
    Task<bool> DeleteAsync(EmployeeNumber employeeNumber);
}
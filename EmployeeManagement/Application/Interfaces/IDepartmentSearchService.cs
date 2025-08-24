using EmployeeManagement.Domain.Models;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Application.Interfaces;

/// <summary>
/// 部門検索サービスのインターフェース
/// 部門マスタから部門を検索する機能を提供
/// </summary>
public interface IDepartmentSearchService
{
    /// <summary>
    /// オートコンプリート用の部門検索
    /// 軽量な検索結果を返す
    /// </summary>
    /// <param name="searchTerm">検索語句</param>
    /// <param name="maxResults">最大取得件数</param>
    /// <returns>部門マスタリスト</returns>
    Task<IEnumerable<DepartmentMaster>> SearchDepartmentsAutocompleteAsync(string searchTerm, int maxResults = 10);

    /// <summary>
    /// 詳細検索用の部門検索
    /// 複数の条件による詳細検索機能
    /// </summary>
    /// <param name="searchCriteria">検索条件</param>
    /// <returns>検索結果</returns>
    Task<IEnumerable<DepartmentMaster>> SearchDepartmentsAsync(DepartmentSearchCriteria searchCriteria);

    /// <summary>
    /// 部門コードによる単体取得
    /// </summary>
    /// <param name="departmentCode">部門コード</param>
    /// <returns>部門マスタ（見つからない場合はnull）</returns>
    Task<DepartmentMaster?> GetByDepartmentCodeAsync(string departmentCode);

    /// <summary>
    /// 有効な部門のみを取得
    /// </summary>
    /// <returns>有効な部門マスタリスト</returns>
    Task<IEnumerable<DepartmentMaster>> GetActiveDepartmentsAsync();

    /// <summary>
    /// 部門データのキャッシュを無効化
    /// 部門マスタでデータが更新された際に呼び出す
    /// </summary>
    void InvalidateCache();
}

/// <summary>
/// 部門検索条件
/// DepartmentSearchDialogで使用される詳細検索機能
/// </summary>
public class DepartmentSearchCriteria
{
    /// <summary>
    /// 部門コード（部分一致）
    /// </summary>
    public string? DepartmentCode { get; set; }

    /// <summary>
    /// 部門名（部分一致）
    /// </summary>
    public string? DepartmentName { get; set; }

    /// <summary>
    /// 責任者名（部分一致）
    /// </summary>
    public string? ManagerName { get; set; }

    /// <summary>
    /// 責任者社員番号（完全一致）
    /// </summary>
    public string? ManagerEmployeeNumber { get; set; }

    /// <summary>
    /// 部門種別
    /// </summary>
    public Department? DepartmentType { get; set; }

    /// <summary>
    /// 有効状態フィルター
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// 最大取得件数
    /// </summary>
    public int MaxResults { get; set; } = 50;
}
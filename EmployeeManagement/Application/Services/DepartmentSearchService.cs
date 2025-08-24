using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace EmployeeManagement.Application.Services;

/// <summary>
/// 部門検索サービスの実装
/// 部門マスタから部門を検索する機能を提供
/// </summary>
public class DepartmentSearchService : IDepartmentSearchService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<DepartmentSearchService> _logger;

    private const string ALL_DEPARTMENTS_CACHE_KEY = "DepartmentSearch_AllDepartments";
    private const int CACHE_DURATION_MINUTES = 10;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="departmentRepository">部門リポジトリ</param>
    /// <param name="memoryCache">メモリキャッシュ</param>
    /// <param name="logger">ロガー</param>
    public DepartmentSearchService(
        IDepartmentRepository departmentRepository,
        IMemoryCache memoryCache,
        ILogger<DepartmentSearchService> logger)
    {
        _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// オートコンプリート用の部門検索
    /// 軽量な検索結果を返す
    /// </summary>
    /// <param name="searchTerm">検索語句</param>
    /// <param name="maxResults">最大取得件数</param>
    /// <returns>部門マスタリスト</returns>
    public async Task<IEnumerable<DepartmentMaster>> SearchDepartmentsAutocompleteAsync(string searchTerm, int maxResults = 10)
    {
        try
        {
            // 入力検証
            if (maxResults <= 0 || maxResults > 1000)
                throw new ArgumentOutOfRangeException(nameof(maxResults), "Max results must be between 1 and 1000");

            _logger.LogDebug("部門オートコンプリート検索開始: 検索語句={SearchTerm}, 最大件数={MaxResults}", searchTerm, maxResults);

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // 検索語句が空の場合は有効な部門を返す
                var activeDepartments = await GetActiveDepartmentsAsync();
                return activeDepartments.Take(maxResults);
            }

            // キャッシュから全部門を取得してフィルタリング
            var allDepartments = await GetCachedAllDepartmentsAsync();
            
            var filteredDepartments = allDepartments
                .Where(d => d.IsActive) // 有効な部門のみ
                .Where(d => 
                    MatchesSearchTerm(d.DepartmentCode, searchTerm) ||
                    MatchesSearchTerm(d.DepartmentName, searchTerm) ||
                    MatchesSearchTerm(d.ManagerName, searchTerm))
                .OrderBy(d => d.DepartmentCode) // 部門コード順でソート
                .Take(maxResults);

            _logger.LogDebug("部門オートコンプリート検索完了: 結果件数={ResultCount}", filteredDepartments.Count());
            
            return filteredDepartments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "部門オートコンプリート検索中にエラーが発生しました: 検索語句={SearchTerm}", searchTerm);
            return Enumerable.Empty<DepartmentMaster>();
        }
    }

    /// <summary>
    /// 詳細検索用の部門検索
    /// 複数の条件による詳細検索機能
    /// </summary>
    /// <param name="searchCriteria">検索条件</param>
    /// <returns>検索結果</returns>
    public async Task<IEnumerable<DepartmentMaster>> SearchDepartmentsAsync(DepartmentSearchCriteria searchCriteria)
    {
        try
        {
            // 入力検証
            if (searchCriteria.MaxResults <= 0 || searchCriteria.MaxResults > 1000)
                throw new ArgumentOutOfRangeException(nameof(searchCriteria.MaxResults), "Max results must be between 1 and 1000");

            _logger.LogDebug("部門詳細検索開始: {@SearchCriteria}", searchCriteria);

            // キャッシュから全部門を取得
            var allDepartments = await GetCachedAllDepartmentsAsync();
            var query = allDepartments.AsEnumerable();

            // 部門コード条件
            if (!string.IsNullOrWhiteSpace(searchCriteria.DepartmentCode))
            {
                query = query.Where(d => MatchesSearchTerm(d.DepartmentCode, searchCriteria.DepartmentCode));
            }

            // 部門名条件
            if (!string.IsNullOrWhiteSpace(searchCriteria.DepartmentName))
            {
                query = query.Where(d => MatchesSearchTerm(d.DepartmentName, searchCriteria.DepartmentName));
            }

            // 責任者名条件
            if (!string.IsNullOrWhiteSpace(searchCriteria.ManagerName))
            {
                query = query.Where(d => MatchesSearchTerm(d.ManagerName, searchCriteria.ManagerName));
            }

            // 責任者社員番号条件
            if (!string.IsNullOrWhiteSpace(searchCriteria.ManagerEmployeeNumber))
            {
                query = query.Where(d => d.ManagerEmployeeNumber == searchCriteria.ManagerEmployeeNumber);
            }

            // 部門種別条件
            if (searchCriteria.DepartmentType.HasValue)
            {
                query = query.Where(d => d.DepartmentType == searchCriteria.DepartmentType.Value);
            }

            // 有効状態条件
            if (searchCriteria.IsActive.HasValue)
            {
                query = query.Where(d => d.IsActive == searchCriteria.IsActive.Value);
            }

            // ソートと件数制限
            var results = query
                .OrderBy(d => d.DepartmentCode)
                .Take(searchCriteria.MaxResults);

            _logger.LogDebug("部門詳細検索完了: 結果件数={ResultCount}", results.Count());

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "部門詳細検索中にエラーが発生しました: {@SearchCriteria}", searchCriteria);
            return Enumerable.Empty<DepartmentMaster>();
        }
    }

    /// <summary>
    /// 部門コードによる単体取得
    /// </summary>
    /// <param name="departmentCode">部門コード</param>
    /// <returns>部門マスタ（見つからない場合はnull）</returns>
    public async Task<DepartmentMaster?> GetByDepartmentCodeAsync(string departmentCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(departmentCode))
            {
                return null;
            }

            _logger.LogDebug("部門単体取得開始: 部門コード={DepartmentCode}", departmentCode);

            var department = await _departmentRepository.GetByIdAsync(departmentCode);

            _logger.LogDebug("部門単体取得完了: 部門コード={DepartmentCode}, 取得結果={Found}", 
                departmentCode, department != null);

            return department;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "部門単体取得中にエラーが発生しました: 部門コード={DepartmentCode}", departmentCode);
            return null;
        }
    }

    /// <summary>
    /// 有効な部門のみを取得
    /// </summary>
    /// <returns>有効な部門マスタリスト</returns>
    public async Task<IEnumerable<DepartmentMaster>> GetActiveDepartmentsAsync()
    {
        try
        {
            _logger.LogDebug("有効部門一覧取得開始");

            var allDepartments = await GetCachedAllDepartmentsAsync();
            var activeDepartments = allDepartments
                .Where(d => d.IsActive)
                .OrderBy(d => d.DepartmentCode);

            _logger.LogDebug("有効部門一覧取得完了: 結果件数={ResultCount}", activeDepartments.Count());

            return activeDepartments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "有効部門一覧取得中にエラーが発生しました");
            return Enumerable.Empty<DepartmentMaster>();
        }
    }

    /// <summary>
    /// キャッシュから全部門を取得（パフォーマンス最適化）
    /// </summary>
    /// <returns>全部門リスト</returns>
    private async Task<IEnumerable<DepartmentMaster>> GetCachedAllDepartmentsAsync()
    {
        try
        {
            if (_memoryCache.TryGetValue(ALL_DEPARTMENTS_CACHE_KEY, out IEnumerable<DepartmentMaster>? cached))
            {
                _logger.LogDebug("部門データをキャッシュから取得");
                return cached!;
            }

            _logger.LogDebug("部門データをリポジトリから取得してキャッシュに保存");
            var departments = await _departmentRepository.GetAllAsync();
            var departmentList = departments.ToList();

            _memoryCache.Set(ALL_DEPARTMENTS_CACHE_KEY, departmentList, 
                TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));

            return departmentList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "キャッシュされた部門データの取得中にエラーが発生しました");
            // フォールバック：キャッシュに失敗した場合は直接取得
            return await _departmentRepository.GetAllAsync();
        }
    }

    /// <summary>
    /// 文字列の部分一致検索（大文字小文字を区別しない）
    /// パフォーマンス最適化されたヘルパーメソッド
    /// </summary>
    /// <param name="field">検索対象フィールド</param>
    /// <param name="searchTerm">検索語句</param>
    /// <returns>一致する場合はtrue</returns>
    private static bool MatchesSearchTerm(string? field, string searchTerm)
    {
        return !string.IsNullOrEmpty(field) && 
               field.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 部門データのキャッシュを無効化
    /// 部門マスタでデータが更新された際に呼び出す
    /// </summary>
    public void InvalidateCache()
    {
        try
        {
            _memoryCache.Remove(ALL_DEPARTMENTS_CACHE_KEY);
            _logger.LogInformation("部門データのキャッシュを無効化しました");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "部門データのキャッシュ無効化中にエラーが発生しました");
        }
    }
}
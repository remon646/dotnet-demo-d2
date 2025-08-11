using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Constants;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace EmployeeManagement.Application.Services;

/// <summary>
/// 社員検索機能を提供するサービス実装クラス
/// オートコンプリート用の高速検索と詳細検索機能を実装
/// キャッシュ機能により高いパフォーマンスを実現
/// </summary>
public class EmployeeSearchService : IEmployeeSearchService
{
    #region Private Fields
    
    /// <summary>
    /// 社員データアクセス用リポジトリ
    /// 基本的なCRUD操作を提供
    /// </summary>
    private readonly IEmployeeRepository _employeeRepository;
    
    /// <summary>
    /// 責任者バリデーションサービス
    /// 責任者適格性の判定に使用
    /// </summary>
    private readonly IManagerValidationService _managerValidationService;
    
    /// <summary>
    /// メモリキャッシュサービス
    /// 検索結果のキャッシュに使用
    /// </summary>
    private readonly IMemoryCache _memoryCache;
    
    /// <summary>
    /// ログ出力用インスタンス
    /// 検索処理の詳細ログと統計情報に使用
    /// </summary>
    private readonly ILogger<EmployeeSearchService> _logger;
    
    /// <summary>
    /// 検索統計情報（スレッドセーフ）
    /// パフォーマンス監視用の統計データ
    /// </summary>
    private readonly ConcurrentDictionary<string, long> _statistics;
    
    /// <summary>
    /// レスポンス時間測定用リスト（スレッドセーフ）
    /// 平均レスポンス時間計算に使用
    /// </summary>
    private readonly ConcurrentQueue<double> _responseTimes;
    
    /// <summary>
    /// 統計情報リセット時刻
    /// 統計情報の期間管理に使用
    /// </summary>
    private DateTime _statisticsResetTime;
    
    #endregion
    
    #region Constants
    
    /// <summary>キャッシュキーのプレフィックス</summary>
    private const string CACHE_PREFIX = "EmployeeSearch";
    
    /// <summary>全社員キャッシュのキー</summary>
    private const string ALL_EMPLOYEES_CACHE_KEY = $"{CACHE_PREFIX}_AllEmployees";
    
    /// <summary>キャッシュの有効期限（分）</summary>
    private const int CACHE_DURATION_MINUTES = 10;
    
    /// <summary>レスポンス時間サンプルの最大保持数</summary>
    private const int MAX_RESPONSE_TIME_SAMPLES = 1000;
    
    #endregion
    
    #region Constructor
    
    /// <summary>
    /// コンストラクタ - 依存性注入によるインスタンス初期化
    /// </summary>
    /// <param name="employeeRepository">社員リポジトリ</param>
    /// <param name="managerValidationService">責任者バリデーションサービス</param>
    /// <param name="memoryCache">メモリキャッシュサービス</param>
    /// <param name="logger">ログ出力インスタンス</param>
    public EmployeeSearchService(
        IEmployeeRepository employeeRepository,
        IManagerValidationService managerValidationService,
        IMemoryCache memoryCache,
        ILogger<EmployeeSearchService> logger)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _managerValidationService = managerValidationService ?? throw new ArgumentNullException(nameof(managerValidationService));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // 統計情報の初期化
        _statistics = new ConcurrentDictionary<string, long>();
        _responseTimes = new ConcurrentQueue<double>();
        _statisticsResetTime = DateTime.Now;
        
        // 統計情報カウンターの初期化
        InitializeStatistics();
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// オートコンプリート用の社員検索
    /// </summary>
    public async Task<IEnumerable<Employee>> SearchForAutocompleteAsync(
        string keyword, 
        int maxResults = DepartmentEditConstants.MAX_SEARCH_RESULTS, 
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // 統計情報の更新
            _statistics.AddOrUpdate("AutocompleteSearchCount", 1, (key, value) => value + 1);
            
            _logger.LogDebug("オートコンプリート検索開始: {Keyword}, 最大件数: {MaxResults}", 
                keyword, maxResults);
            
            // キーワードの基本チェック
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < ValidationConstants.EMPLOYEE_NUMBER_MIN_LENGTH)
            {
                _logger.LogDebug("検索キーワードが短すぎるため空の結果を返却: {Keyword}", keyword);
                return Enumerable.Empty<Employee>();
            }
            
            // キャンセレーション確認
            cancellationToken.ThrowIfCancellationRequested();
            
            // キャッシュされた全社員データを取得
            var allEmployees = await GetCachedAllEmployeesAsync(cancellationToken);
            
            // キーワードでフィルタリング（大文字小文字を区別しない）
            var results = allEmployees
                .Where(emp => 
                    emp.EmployeeNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    emp.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .OrderBy(emp => emp.EmployeeNumber)
                .Take(maxResults)
                .ToList();
            
            _logger.LogDebug("オートコンプリート検索完了: {Keyword} -> {ResultCount}件", 
                keyword, results.Count);
            
            return results;
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("オートコンプリート検索がキャンセルされました: {Keyword}", keyword);
            return Enumerable.Empty<Employee>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "オートコンプリート検索中にエラーが発生: {Keyword}", keyword);
            return Enumerable.Empty<Employee>();
        }
        finally
        {
            // レスポンス時間の記録
            stopwatch.Stop();
            RecordResponseTime(stopwatch.Elapsed.TotalMilliseconds);
        }
    }
    
    /// <summary>
    /// 詳細条件での社員検索（ダイアログ用）
    /// </summary>
    public async Task<IEnumerable<Employee>> SearchDetailedAsync(
        EmployeeSearchCriteria criteria, 
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // 統計情報の更新
            _statistics.AddOrUpdate("DetailedSearchCount", 1, (key, value) => value + 1);
            
            _logger.LogDebug("詳細検索開始: {Criteria}", 
                System.Text.Json.JsonSerializer.Serialize(criteria));
            
            if (criteria == null || criteria.IsEmpty)
            {
                _logger.LogDebug("検索条件が空のため空の結果を返却");
                return Enumerable.Empty<Employee>();
            }
            
            // キャンセレーション確認
            cancellationToken.ThrowIfCancellationRequested();
            
            // キャッシュされた全社員データを取得
            var allEmployees = await GetCachedAllEmployeesAsync(cancellationToken);
            
            // 条件でフィルタリング
            var query = allEmployees.AsEnumerable();
            
            // 社員番号フィルタ
            if (!string.IsNullOrWhiteSpace(criteria.EmployeeNumber))
            {
                query = query.Where(emp => 
                    emp.EmployeeNumber.Contains(criteria.EmployeeNumber, StringComparison.OrdinalIgnoreCase));
            }
            
            // 氏名フィルタ
            if (!string.IsNullOrWhiteSpace(criteria.Name))
            {
                query = query.Where(emp => 
                    emp.Name.Contains(criteria.Name, StringComparison.OrdinalIgnoreCase));
            }
            
            // 部門フィルタ
            if (!string.IsNullOrWhiteSpace(criteria.DepartmentCode))
            {
                // TODO: DepartmentHistoryとDepartmentMasterの関連付けが必要
                // 現在のモックデータベース構造では部門コードでの検索が困難
                // 将来的にデータベーススキーマの改善が必要
                /*
                query = query.Where(emp => 
                    emp.CurrentDepartmentHistory?.Department.ToString().Equals(criteria.DepartmentCode, StringComparison.OrdinalIgnoreCase) == true);
                */
            }
            
            // 職位フィルタ
            if (!string.IsNullOrWhiteSpace(criteria.Position))
            {
                query = query.Where(emp => 
                    emp.CurrentPosition?.ToString().Equals(criteria.Position, StringComparison.OrdinalIgnoreCase) == true);
            }
            
            // 有効な社員のみフィルタ
            if (criteria.ActiveOnly)
            {
                // 将来的に Employee に IsActive プロパティが追加される場合の実装
                // query = query.Where(emp => emp.IsActive);
            }
            
            var results = query.OrderBy(emp => emp.EmployeeNumber).ToList();
            
            _logger.LogInformation("詳細検索完了: {ResultCount}件", results.Count);
            
            return results;
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("詳細検索がキャンセルされました");
            return Enumerable.Empty<Employee>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "詳細検索中にエラーが発生");
            return Enumerable.Empty<Employee>();
        }
        finally
        {
            // レスポンス時間の記録
            stopwatch.Stop();
            RecordResponseTime(stopwatch.Elapsed.TotalMilliseconds);
        }
    }
    
    /// <summary>
    /// 責任者として適格な社員のみを検索
    /// </summary>
    public async Task<IEnumerable<Employee>> SearchEligibleManagersAsync(
        string keyword = "", 
        int maxResults = DepartmentEditConstants.MAX_SEARCH_RESULTS, 
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("責任者候補検索開始: {Keyword}, 最大件数: {MaxResults}", keyword, maxResults);
            
            // キャンセレーション確認
            cancellationToken.ThrowIfCancellationRequested();
            
            // 責任者として適格な社員一覧を取得
            var eligibleManagers = await _managerValidationService.GetEligibleManagersAsync();
            
            // キーワードでフィルタリング（キーワードが空の場合は全件）
            var results = string.IsNullOrWhiteSpace(keyword)
                ? eligibleManagers.Take(maxResults).ToList()
                : eligibleManagers
                    .Where(emp => 
                        emp.EmployeeNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        emp.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(emp => emp.EmployeeNumber)
                    .Take(maxResults)
                    .ToList();
            
            _logger.LogDebug("責任者候補検索完了: {Keyword} -> {ResultCount}件", keyword, results.Count);
            
            return results;
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("責任者候補検索がキャンセルされました: {Keyword}", keyword);
            return Enumerable.Empty<Employee>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "責任者候補検索中にエラーが発生: {Keyword}", keyword);
            return Enumerable.Empty<Employee>();
        }
        finally
        {
            // レスポンス時間の記録
            stopwatch.Stop();
            RecordResponseTime(stopwatch.Elapsed.TotalMilliseconds);
        }
    }
    
    /// <summary>
    /// 社員番号での完全一致検索
    /// </summary>
    public async Task<Employee?> GetByEmployeeNumberAsync(
        string employeeNumber, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(employeeNumber))
        {
            return null;
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("社員番号検索開始: {EmployeeNumber}", employeeNumber);
            
            // キャンセレーション確認
            cancellationToken.ThrowIfCancellationRequested();
            
            // 直接リポジトリから取得（高速化のためキャッシュは使わない）
            var employee = await _employeeRepository.GetByEmployeeNumberAsync(employeeNumber);
            
            if (employee != null)
            {
                _logger.LogDebug("社員番号検索成功: {EmployeeNumber} -> {Name}", 
                    employeeNumber, employee.Name);
            }
            else
            {
                _logger.LogDebug("社員番号検索失敗: {EmployeeNumber} -> 見つかりません", employeeNumber);
            }
            
            return employee;
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("社員番号検索がキャンセルされました: {EmployeeNumber}", employeeNumber);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "社員番号検索中にエラーが発生: {EmployeeNumber}", employeeNumber);
            return null;
        }
        finally
        {
            // レスポンス時間の記録
            stopwatch.Stop();
            RecordResponseTime(stopwatch.Elapsed.TotalMilliseconds);
        }
    }
    
    /// <summary>
    /// 検索キャッシュをクリア
    /// </summary>
    public void ClearCache()
    {
        _logger.LogInformation("検索キャッシュクリア開始");
        
        try
        {
            // 全社員キャッシュをクリア
            _memoryCache.Remove(ALL_EMPLOYEES_CACHE_KEY);
            
            _logger.LogInformation("検索キャッシュクリア完了");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "検索キャッシュクリア中にエラーが発生");
        }
    }
    
    /// <summary>
    /// 検索統計情報を取得
    /// </summary>
    public SearchStatistics GetSearchStatistics()
    {
        // レスポンス時間の平均を計算
        var responseTimes = _responseTimes.ToArray();
        var averageResponseTime = responseTimes.Any() ? responseTimes.Average() : 0.0;
        
        return new SearchStatistics
        {
            AutocompleteSearchCount = _statistics.GetValueOrDefault("AutocompleteSearchCount"),
            DetailedSearchCount = _statistics.GetValueOrDefault("DetailedSearchCount"),
            CacheHitCount = _statistics.GetValueOrDefault("CacheHitCount"),
            CacheMissCount = _statistics.GetValueOrDefault("CacheMissCount"),
            AverageResponseTimeMs = averageResponseTime,
            LastResetTime = _statisticsResetTime
        };
    }
    
    #endregion
    
    #region Private Helper Methods
    
    /// <summary>
    /// キャッシュされた全社員データを取得
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>全社員のリスト</returns>
    private async Task<IEnumerable<Employee>> GetCachedAllEmployeesAsync(CancellationToken cancellationToken)
    {
        // キャッシュから取得を試行
        if (_memoryCache.TryGetValue(ALL_EMPLOYEES_CACHE_KEY, out IEnumerable<Employee>? cachedEmployees))
        {
            _statistics.AddOrUpdate("CacheHitCount", 1, (key, value) => value + 1);
            _logger.LogDebug("全社員データをキャッシュから取得");
            return cachedEmployees!;
        }
        
        // キャッシュミス
        _statistics.AddOrUpdate("CacheMissCount", 1, (key, value) => value + 1);
        _logger.LogDebug("全社員データをデータベースから取得");
        
        // データベースから取得
        var employees = await _employeeRepository.GetAllAsync();
        var employeeList = employees.ToList();
        
        // キャッシュに保存
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
            Priority = CacheItemPriority.Normal
        };
        
        _memoryCache.Set(ALL_EMPLOYEES_CACHE_KEY, employeeList, cacheOptions);
        
        _logger.LogDebug("全社員データをキャッシュに保存: {Count}件, 有効期限: {Duration}分", 
            employeeList.Count, CACHE_DURATION_MINUTES);
        
        return employeeList;
    }
    
    /// <summary>
    /// レスポンス時間を記録
    /// </summary>
    /// <param name="responseTimeMs">レスポンス時間（ミリ秒）</param>
    private void RecordResponseTime(double responseTimeMs)
    {
        _responseTimes.Enqueue(responseTimeMs);
        
        // サンプル数の制限
        while (_responseTimes.Count > MAX_RESPONSE_TIME_SAMPLES)
        {
            _responseTimes.TryDequeue(out _);
        }
    }
    
    /// <summary>
    /// 統計情報カウンターの初期化
    /// </summary>
    private void InitializeStatistics()
    {
        _statistics.TryAdd("AutocompleteSearchCount", 0);
        _statistics.TryAdd("DetailedSearchCount", 0);
        _statistics.TryAdd("CacheHitCount", 0);
        _statistics.TryAdd("CacheMissCount", 0);
    }
    
    #endregion
}
using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Application.Interfaces;

/// <summary>
/// 社員検索機能を提供するサービスインターフェース
/// オートコンプリート用の高速検索と詳細検索機能を提供
/// UIコンポーネントから検索処理を分離し疎結合を実現
/// </summary>
public interface IEmployeeSearchService
{
    /// <summary>
    /// オートコンプリート用の社員検索
    /// </summary>
    /// <param name="keyword">検索キーワード（社員番号または氏名）</param>
    /// <param name="maxResults">最大結果件数</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>
    /// 検索結果の社員リスト
    /// 社員番号と氏名での部分一致検索を実行
    /// 結果は社員番号順でソートして返却
    /// </returns>
    /// <remarks>
    /// このメソッドは以下の特徴を持ちます:
    /// 1. 高速レスポンスのため最小文字数制限あり
    /// 2. 結果件数制限によるパフォーマンス最適化
    /// 3. キャンセレーション対応でリアルタイム検索に適用
    /// 4. 大文字小文字を区別しない検索
    /// </remarks>
    Task<IEnumerable<Employee>> SearchForAutocompleteAsync(
        string keyword, 
        int maxResults = 10, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 詳細条件での社員検索（ダイアログ用）
    /// </summary>
    /// <param name="criteria">検索条件</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>
    /// 検索条件にマッチする社員リスト
    /// 複数条件での絞り込み検索を実行
    /// </returns>
    /// <remarks>
    /// EmployeeSearchDialogで使用される詳細検索機能
    /// 複数の検索条件を組み合わせた柔軟な検索を提供
    /// </remarks>
    Task<IEnumerable<Employee>> SearchDetailedAsync(
        EmployeeSearchCriteria criteria, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 責任者として適格な社員のみを検索
    /// </summary>
    /// <param name="keyword">検索キーワード</param>
    /// <param name="maxResults">最大結果件数</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>
    /// 責任者として設定可能な社員リスト
    /// 退職者や制限対象者を除外
    /// </returns>
    /// <remarks>
    /// 責任者選択時に使用する専用検索機能
    /// 業務制約に基づいた適格性フィルタリングを実行
    /// </remarks>
    Task<IEnumerable<Employee>> SearchEligibleManagersAsync(
        string keyword = "", 
        int maxResults = 10, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 社員番号での完全一致検索
    /// </summary>
    /// <param name="employeeNumber">社員番号</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>
    /// 該当する社員（見つからない場合はnull）
    /// </returns>
    /// <remarks>
    /// バリデーション処理や詳細情報取得で使用
    /// キャッシュ機能により高速レスポンスを実現
    /// </remarks>
    Task<Employee?> GetByEmployeeNumberAsync(
        string employeeNumber, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 検索キャッシュをクリア
    /// </summary>
    /// <remarks>
    /// データ更新後の整合性確保やメモリ使用量制御で使用
    /// 定期的なクリアやシステム負荷軽減に活用
    /// </remarks>
    void ClearCache();
    
    /// <summary>
    /// 検索統計情報を取得
    /// </summary>
    /// <returns>
    /// 検索回数、キャッシュヒット率等の統計情報
    /// </returns>
    /// <remarks>
    /// パフォーマンス監視や最適化のための情報提供
    /// 開発・運用時のボトルネック特定に活用
    /// </remarks>
    SearchStatistics GetSearchStatistics();
}

/// <summary>
/// 社員検索条件を格納するクラス
/// 詳細検索ダイアログでの複数条件検索に使用
/// </summary>
public class EmployeeSearchCriteria
{
    /// <summary>社員番号（部分一致）</summary>
    public string? EmployeeNumber { get; set; }
    
    /// <summary>氏名（部分一致）</summary>
    public string? Name { get; set; }
    
    /// <summary>所属部門コード（完全一致）</summary>
    public string? DepartmentCode { get; set; }
    
    /// <summary>職位（完全一致）</summary>
    public string? Position { get; set; }
    
    /// <summary>有効な社員のみ検索するフラグ</summary>
    public bool ActiveOnly { get; set; } = true;
    
    /// <summary>
    /// 検索条件が空かどうかを判定
    /// </summary>
    public bool IsEmpty => 
        string.IsNullOrWhiteSpace(EmployeeNumber) && 
        string.IsNullOrWhiteSpace(Name) && 
        string.IsNullOrWhiteSpace(DepartmentCode) && 
        string.IsNullOrWhiteSpace(Position);
}

/// <summary>
/// 検索統計情報を格納するクラス
/// パフォーマンス監視と最適化に使用
/// </summary>
public class SearchStatistics
{
    /// <summary>オートコンプリート検索回数</summary>
    public long AutocompleteSearchCount { get; init; }
    
    /// <summary>詳細検索回数</summary>
    public long DetailedSearchCount { get; init; }
    
    /// <summary>キャッシュヒット回数</summary>
    public long CacheHitCount { get; init; }
    
    /// <summary>キャッシュミス回数</summary>
    public long CacheMissCount { get; init; }
    
    /// <summary>総検索回数</summary>
    public long TotalSearchCount => AutocompleteSearchCount + DetailedSearchCount;
    
    /// <summary>キャッシュヒット率（パーセント）</summary>
    public double CacheHitRate
    {
        get
        {
            var totalCacheAccess = CacheHitCount + CacheMissCount;
            return totalCacheAccess == 0 ? 0.0 : (double)CacheHitCount / totalCacheAccess * 100;
        }
    }
    
    /// <summary>平均レスポンス時間（ミリ秒）</summary>
    public double AverageResponseTimeMs { get; init; }
    
    /// <summary>最後にリセットされた日時</summary>
    public DateTime LastResetTime { get; init; }
}
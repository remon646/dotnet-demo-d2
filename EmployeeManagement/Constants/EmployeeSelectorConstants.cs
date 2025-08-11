namespace EmployeeManagement.Constants;

/// <summary>
/// 社員選択コンポーネント関連の定数定義
/// パフォーマンス最適化とメンテナンス性向上のための設定値
/// </summary>
public static class EmployeeSelectorConstants
{
    /// <summary>デフォルトの検索結果表示高さ（ピクセル）</summary>
    public const int DEFAULT_SEARCH_RESULTS_HEIGHT = 400;
    
    /// <summary>デフォルトのオートコンプリート最大表示件数</summary>
    public const int DEFAULT_MAX_AUTOCOMPLETE_RESULTS = 10;
    
    /// <summary>デフォルトの検索最小文字数</summary>
    public const int DEFAULT_MIN_SEARCH_LENGTH = 2;
    
    /// <summary>責任者検索時の最大取得件数</summary>
    public const int MANAGER_SEARCH_MAX_RESULTS = 1000;
    
    /// <summary>検索キャッシュの有効期限（分）</summary>
    public const int SEARCH_CACHE_EXPIRY_MINUTES = 5;
    
    /// <summary>検索結果の最大表示件数（詳細検索）</summary>
    public const int DETAILED_SEARCH_MAX_RESULTS = 500;
    
    /// <summary>オートコンプリート検索の遅延時間（ミリ秒）</summary>
    public const int AUTOCOMPLETE_DEBOUNCE_MS = 300;
}

/// <summary>
/// 社員選択コンポーネントのCSSクラス定義
/// 一貫したスタイリングのための定数
/// </summary>
public static class EmployeeSelectorCssClasses
{
    /// <summary>メインコンテナのCSSクラス</summary>
    public const string MAIN_CONTAINER = "employee-selector-component";
    
    /// <summary>選択情報表示カードのCSSクラス</summary>
    public const string SELECTED_INFO_CARD = "employee-selector-selected-info";
    
    /// <summary>責任者候補チップのCSSクラス</summary>
    public const string MANAGER_CANDIDATE_CHIP = "manager-candidate-chip";
    
    /// <summary>コンパクトセレクターのCSSクラス</summary>
    public const string COMPACT_SELECTOR = "employee-selector-compact";
}
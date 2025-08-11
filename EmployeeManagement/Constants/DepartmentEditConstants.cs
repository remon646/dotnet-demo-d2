namespace EmployeeManagement.Constants;

/// <summary>
/// 部門編集画面で使用する定数定義
/// UI表示やユーザー操作に関連する数値を集約管理
/// コードの可読性向上とマジックナンバー排除が目的
/// </summary>
public static class DepartmentEditConstants
{
    #region 検索関連定数
    
    /// <summary>
    /// オートコンプリート検索結果の最大表示件数
    /// パフォーマンス考慮により制限を設定
    /// </summary>
    public const int MAX_SEARCH_RESULTS = 10;
    
    /// <summary>
    /// 検索を開始する最小文字数
    /// 無駄な検索処理を防止しDBアクセス負荷を軽減
    /// </summary>
    public const int MIN_SEARCH_LENGTH = 2;
    
    /// <summary>
    /// 検索処理のデバウンス時間（ミリ秒）
    /// ユーザーの入力中の連続検索を防止
    /// </summary>
    public const int SEARCH_DEBOUNCE_MS = 300;
    
    #endregion
    
    #region フィールド長制限定数
    
    /// <summary>
    /// 部署コードの最大文字数
    /// データベーススキーマと一致させる
    /// </summary>
    public const int DEPARTMENT_CODE_MAX_LENGTH = 10;
    
    /// <summary>
    /// 部署名の最大文字数
    /// UI表示とデータベーススキーマ制限
    /// </summary>
    public const int DEPARTMENT_NAME_MAX_LENGTH = 50;
    
    /// <summary>
    /// 社員番号の最大文字数
    /// システム統一の社員番号形式
    /// </summary>
    public const int EMPLOYEE_NUMBER_MAX_LENGTH = 10;
    
    /// <summary>
    /// 内線番号の最大文字数
    /// 一般的な内線番号桁数を考慮
    /// </summary>
    public const int EXTENSION_MAX_LENGTH = 10;
    
    /// <summary>
    /// 説明文の最大文字数
    /// ユーザビリティとデータベース制限のバランス
    /// </summary>
    public const int DESCRIPTION_MAX_LENGTH = 500;
    
    #endregion
    
    #region UI表示設定定数
    
    /// <summary>
    /// 説明文テキストエリアの表示行数
    /// ユーザビリティを考慮した適切な表示領域
    /// </summary>
    public const int DESCRIPTION_TEXTAREA_LINES = 3;
    
    /// <summary>
    /// 成功メッセージ表示後の画面遷移遅延時間（ミリ秒）
    /// ユーザーがメッセージを確認できる時間を確保
    /// </summary>
    public const int SUCCESS_MESSAGE_DELAY_MS = 1000;
    
    #endregion
    
    #region バリデーション関連定数
    
    /// <summary>
    /// 部署コードの最小文字数
    /// ビジネスルールに基づく制限
    /// </summary>
    public const int DEPARTMENT_CODE_MIN_LENGTH = 3;
    
    /// <summary>
    /// 部署名の最小文字数
    /// データ品質確保のための制限
    /// </summary>
    public const int DEPARTMENT_NAME_MIN_LENGTH = 2;
    
    #endregion
}
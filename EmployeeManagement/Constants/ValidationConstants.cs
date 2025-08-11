namespace EmployeeManagement.Constants;

/// <summary>
/// バリデーション処理で使用する定数定義
/// 各種バリデーションルールの数値基準を集約管理
/// ビジネスルールの一元管理とメンテナンス性向上が目的
/// </summary>
public static class ValidationConstants
{
    #region 一般的なバリデーション定数
    
    /// <summary>
    /// 必須項目の最小文字数
    /// 空白や無効な入力を防ぐための基準
    /// </summary>
    public const int REQUIRED_FIELD_MIN_LENGTH = 1;
    
    #endregion
    
    #region 部門関連バリデーション定数
    
    /// <summary>
    /// 部署コード形式チェック用の正規表現パターン
    /// 英数字のみを許可する制限
    /// </summary>
    public const string DEPARTMENT_CODE_PATTERN = @"^[a-zA-Z0-9]+$";
    
    /// <summary>
    /// 部署コードの最小有効文字数
    /// ビジネスルールによる制限
    /// </summary>
    public const int DEPARTMENT_CODE_MIN_LENGTH = 3;
    
    /// <summary>
    /// 部署名の最小有効文字数
    /// データ品質確保のための制限
    /// </summary>
    public const int DEPARTMENT_NAME_MIN_LENGTH = 2;
    
    #endregion
    
    #region 社員関連バリデーション定数
    
    /// <summary>
    /// 社員番号形式チェック用の正規表現パターン
    /// システム統一の社員番号形式
    /// </summary>
    public const string EMPLOYEE_NUMBER_PATTERN = @"^[a-zA-Z0-9]+$";
    
    /// <summary>
    /// 社員番号の最小有効文字数
    /// システム運用上の最小要件
    /// </summary>
    public const int EMPLOYEE_NUMBER_MIN_LENGTH = 4;
    
    #endregion
    
    #region 日付関連バリデーション定数
    
    /// <summary>
    /// 設立日の最大許可日数（未来日制限）
    /// 設立日は今日以前の日付のみ許可
    /// </summary>
    public const int ESTABLISHED_DATE_MAX_FUTURE_DAYS = 0;
    
    /// <summary>
    /// 設立日の最大過去年数制限
    /// システム運用上の合理的な制限
    /// </summary>
    public const int ESTABLISHED_DATE_MAX_PAST_YEARS = 200;
    
    #endregion
    
    #region エラーメッセージテンプレート
    
    /// <summary>
    /// 必須項目未入力エラーメッセージテンプレート
    /// {0}にフィールド名が入る
    /// </summary>
    public const string REQUIRED_FIELD_ERROR = "{0}は必須です。";
    
    /// <summary>
    /// 最小文字数不足エラーメッセージテンプレート
    /// {0}にフィールド名、{1}に最小文字数が入る
    /// </summary>
    public const string MIN_LENGTH_ERROR = "{0}は{1}文字以上で入力してください。";
    
    /// <summary>
    /// 形式不正エラーメッセージテンプレート
    /// {0}にフィールド名が入る
    /// </summary>
    public const string FORMAT_ERROR = "{0}の形式が正しくありません。";
    
    /// <summary>
    /// 未来日エラーメッセージテンプレート
    /// {0}にフィールド名が入る
    /// </summary>
    public const string FUTURE_DATE_ERROR = "{0}は今日以前の日付を選択してください。";
    
    #endregion
}
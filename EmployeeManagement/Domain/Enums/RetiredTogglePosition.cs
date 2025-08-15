namespace EmployeeManagement.Domain.Enums;

/// <summary>
/// 退職者切り替えスイッチの表示位置
/// AutoCompleteコンポーネントでの退職者フィルタUIの配置を制御
/// </summary>
public enum RetiredTogglePosition
{
    /// <summary>
    /// 非表示
    /// 退職者切り替え機能を無効化
    /// </summary>
    None,

    /// <summary>
    /// 分離表示（従来方式）
    /// AutoCompleteの下に独立したカードで表示
    /// </summary>
    Separate,

    /// <summary>
    /// インライン表示
    /// AutoCompleteと同一行に表示（コンパクト）
    /// </summary>
    Inline,

    /// <summary>
    /// Adornment統合表示
    /// AutoCompleteの装飾領域に検索ボタンと並べて表示
    /// </summary>
    Adornment
}
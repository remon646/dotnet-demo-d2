namespace EmployeeManagement.Domain.Enums;

/// <summary>
/// 社員選択コンポーネントのモード
/// 検索対象や動作を制御するための設定値
/// </summary>
public enum EmployeeSelectorMode
{
    /// <summary>
    /// 標準モード（全社員検索可能）
    /// 全社員を対象とした検索が可能
    /// </summary>
    Standard,

    /// <summary>
    /// 責任者モード（責任者候補のみ検索可能）
    /// 責任者として適格な社員のみを検索対象とする
    /// </summary>
    ManagerOnly,

    /// <summary>
    /// オートコンプリートのみモード（詳細検索ダイアログなし）
    /// 軽量なオートコンプリート機能のみを提供
    /// </summary>
    AutocompleteOnly
}
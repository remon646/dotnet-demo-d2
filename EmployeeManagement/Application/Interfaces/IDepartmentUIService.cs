using MudBlazor;

namespace EmployeeManagement.Application.Interfaces;

/// <summary>
/// 部門編集画面のUI操作を担当するサービスインターフェース
/// Snackbar表示、ナビゲーション等のUI操作を集約
/// UIコンポーネントから具体的なUI実装を分離し疎結合を実現
/// </summary>
public interface IDepartmentUIService
{
    /// <summary>
    /// 成功メッセージを表示し、指定URLに画面遷移を行う
    /// </summary>
    /// <param name="message">表示する成功メッセージ</param>
    /// <param name="navigateTo">遷移先URL</param>
    /// <param name="delayMs">画面遷移前の遅延時間（ミリ秒）</param>
    /// <returns>画面遷移完了までの処理タスク</returns>
    /// <remarks>
    /// 部門作成・更新成功時の標準的なUI操作
    /// ユーザーがメッセージを確認できる時間を確保してから遷移
    /// </remarks>
    Task ShowSuccessAndNavigateAsync(string message, string navigateTo, int delayMs = 1000);
    
    /// <summary>
    /// エラーメッセージを表示する
    /// </summary>
    /// <param name="message">表示するエラーメッセージ</param>
    /// <param name="duration">表示時間（ミリ秒）、nullの場合はデフォルト</param>
    /// <remarks>
    /// バリデーションエラーや処理エラー時に使用
    /// 複数行のメッセージにも対応
    /// </remarks>
    void ShowError(string message, int? duration = null);
    
    /// <summary>
    /// 警告メッセージを表示する
    /// </summary>
    /// <param name="message">表示する警告メッセージ</param>
    /// <param name="duration">表示時間（ミリ秒）、nullの場合はデフォルト</param>
    /// <remarks>
    /// 責任者重複警告などの注意喚起に使用
    /// エラーではないが注意が必要な情報の表示
    /// </remarks>
    void ShowWarning(string message, int? duration = null);
    
    /// <summary>
    /// 情報メッセージを表示する
    /// </summary>
    /// <param name="message">表示する情報メッセージ</param>
    /// <param name="duration">表示時間（ミリ秒）、nullの場合はデフォルト</param>
    /// <remarks>
    /// 一般的な情報提供や操作完了通知に使用
    /// </remarks>
    void ShowInfo(string message, int? duration = null);
    
    /// <summary>
    /// 確認ダイアログを表示し、ユーザーの選択を取得
    /// </summary>
    /// <param name="title">ダイアログのタイトル</param>
    /// <param name="message">確認メッセージ</param>
    /// <param name="confirmText">確認ボタンのテキスト（デフォルト: "はい"）</param>
    /// <param name="cancelText">キャンセルボタンのテキスト（デフォルト: "いいえ"）</param>
    /// <returns>
    /// ユーザーの選択結果
    /// - true: 確認ボタンクリック
    /// - false: キャンセルボタンクリックまたはダイアログ閉じる
    /// </returns>
    /// <remarks>
    /// 削除確認や上書き確認などの重要な操作前に使用
    /// </remarks>
    Task<bool> ShowConfirmationAsync(string title, string message, string confirmText = "はい", string cancelText = "いいえ");
    
    /// <summary>
    /// 指定URLに画面遷移を実行
    /// </summary>
    /// <param name="url">遷移先URL</param>
    /// <param name="forceLoad">強制リロードフラグ</param>
    /// <remarks>
    /// キャンセルボタンや処理完了後の画面遷移で使用
    /// </remarks>
    void NavigateTo(string url, bool forceLoad = false);
    
    /// <summary>
    /// ブラウザの戻るボタンと同等の操作を実行
    /// </summary>
    /// <remarks>
    /// 履歴がある場合は前の画面に戻る
    /// 履歴がない場合は指定のデフォルトURLに遷移
    /// </remarks>
    void NavigateBack(string? defaultUrl = "/departments");
    
    /// <summary>
    /// 複数のエラーメッセージを統合して表示
    /// </summary>
    /// <param name="errors">エラーメッセージのリスト</param>
    /// <param name="title">エラータイトル（任意）</param>
    /// <remarks>
    /// バリデーションエラーが複数ある場合の一括表示
    /// 見やすい形式でユーザーに提示
    /// </remarks>
    void ShowMultipleErrors(IEnumerable<string> errors, string? title = null);
    
    /// <summary>
    /// 長時間処理の開始を示すローディング表示
    /// </summary>
    /// <param name="message">処理中メッセージ</param>
    /// <returns>ローディング制御用のIDisposable</returns>
    /// <remarks>
    /// using文で使用することで自動的にローディング終了
    /// 非同期処理の進行状況をユーザーに明示
    /// </remarks>
    IDisposable ShowLoading(string message = "処理中...");
    
    /// <summary>
    /// 現在表示中のSnackbarメッセージを全てクリア
    /// </summary>
    /// <remarks>
    /// 画面遷移前や新しいメッセージ表示前のクリーンアップ
    /// </remarks>
    void ClearMessages();
}
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace EmployeeManagement.Application.Services;

/// <summary>
/// 部門編集画面のUI操作を担当するサービス実装クラス
/// Snackbar表示、ナビゲーション等のUI操作を集約実装
/// UIコンポーネントから具体的なUI実装を分離
/// </summary>
public class DepartmentUIService : IDepartmentUIService
{
    #region Private Fields
    
    /// <summary>
    /// Snackbarメッセージ表示用サービス
    /// MudBlazorのSnackbar機能を使用
    /// </summary>
    private readonly ISnackbar _snackbar;
    
    /// <summary>
    /// ページナビゲーション用サービス
    /// Blazorの画面遷移機能を使用
    /// </summary>
    private readonly NavigationManager _navigationManager;
    
    /// <summary>
    /// ダイアログ表示用サービス
    /// MudBlazorのダイアログ機能を使用
    /// </summary>
    private readonly IDialogService _dialogService;
    
    /// <summary>
    /// ログ出力用インスタンス
    /// UI操作の詳細ログとエラー追跡に使用
    /// </summary>
    private readonly ILogger<DepartmentUIService> _logger;
    
    #endregion
    
    #region Constructor
    
    /// <summary>
    /// コンストラクタ - 依存性注入によるインスタンス初期化
    /// </summary>
    /// <param name="snackbar">Snackbar表示サービス</param>
    /// <param name="navigationManager">ナビゲーションマネージャー</param>
    /// <param name="dialogService">ダイアログサービス</param>
    /// <param name="logger">ログ出力インスタンス</param>
    public DepartmentUIService(
        ISnackbar snackbar,
        NavigationManager navigationManager,
        IDialogService dialogService,
        ILogger<DepartmentUIService> logger)
    {
        _snackbar = snackbar ?? throw new ArgumentNullException(nameof(snackbar));
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// 成功メッセージを表示し、指定URLに画面遷移を行う
    /// </summary>
    public async Task ShowSuccessAndNavigateAsync(string message, string navigateTo, int delayMs = DepartmentEditConstants.SUCCESS_MESSAGE_DELAY_MS)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning("ShowSuccessAndNavigateAsync: メッセージが空です");
            message = "操作が完了しました。";
        }
        
        if (string.IsNullOrWhiteSpace(navigateTo))
        {
            _logger.LogWarning("ShowSuccessAndNavigateAsync: 遷移先URLが空です");
            navigateTo = "/departments";
        }
        
        _logger.LogInformation("成功メッセージ表示と画面遷移: {Message} -> {NavigateTo}", message, navigateTo);
        
        try
        {
            // 成功メッセージを表示
            _snackbar.Add(message, Severity.Success);
            
            // ユーザーがメッセージを確認できる時間を確保
            await Task.Delay(delayMs);
            
            // 画面遷移実行
            _navigationManager.NavigateTo(navigateTo);
            
            _logger.LogDebug("画面遷移完了: {NavigateTo}", navigateTo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ShowSuccessAndNavigateAsync中にエラーが発生: {NavigateTo}", navigateTo);
            
            // エラーが発生した場合でも画面遷移は実行
            _navigationManager.NavigateTo(navigateTo);
        }
    }
    
    /// <summary>
    /// エラーメッセージを表示する
    /// </summary>
    public void ShowError(string message, int? duration = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning("ShowError: エラーメッセージが空です");
            message = "エラーが発生しました。";
        }
        
        _logger.LogWarning("エラーメッセージ表示: {Message}", message);
        
        try
        {
            var config = CreateSnackbarConfig(duration);
            _snackbar.Add(message, Severity.Error, config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ShowError中にエラーが発生: {Message}", message);
        }
    }
    
    /// <summary>
    /// 警告メッセージを表示する
    /// </summary>
    public void ShowWarning(string message, int? duration = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning("ShowWarning: 警告メッセージが空です");
            return;
        }
        
        _logger.LogInformation("警告メッセージ表示: {Message}", message);
        
        try
        {
            var config = CreateSnackbarConfig(duration);
            _snackbar.Add(message, Severity.Warning, config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ShowWarning中にエラーが発生: {Message}", message);
        }
    }
    
    /// <summary>
    /// 情報メッセージを表示する
    /// </summary>
    public void ShowInfo(string message, int? duration = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning("ShowInfo: 情報メッセージが空です");
            return;
        }
        
        _logger.LogDebug("情報メッセージ表示: {Message}", message);
        
        try
        {
            var config = CreateSnackbarConfig(duration);
            _snackbar.Add(message, Severity.Info, config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ShowInfo中にエラーが発生: {Message}", message);
        }
    }
    
    /// <summary>
    /// 確認ダイアログを表示し、ユーザーの選択を取得
    /// </summary>
    public async Task<bool> ShowConfirmationAsync(string title, string message, string confirmText = "はい", string cancelText = "いいえ")
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            title = "確認";
        }
        
        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning("ShowConfirmationAsync: 確認メッセージが空です");
            message = "この操作を実行してもよろしいですか？";
        }
        
        _logger.LogInformation("確認ダイアログ表示: {Title} - {Message}", title, message);
        
        try
        {
            var result = await _dialogService.ShowMessageBox(
                title: title,
                message: message,
                yesText: confirmText,
                noText: cancelText,
                options: new DialogOptions 
                { 
                    CloseOnEscapeKey = true,
                    MaxWidth = MaxWidth.Small,
                    FullWidth = true
                });
            
            var confirmed = result == true;
            _logger.LogDebug("確認ダイアログ結果: {Confirmed}", confirmed);
            
            return confirmed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ShowConfirmationAsync中にエラーが発生");
            
            // エラー時は安全側に倒してfalseを返却
            return false;
        }
    }
    
    /// <summary>
    /// 指定URLに画面遷移を実行
    /// </summary>
    public void NavigateTo(string url, bool forceLoad = false)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            _logger.LogWarning("NavigateTo: URLが空です");
            url = "/";
        }
        
        _logger.LogInformation("画面遷移実行: {Url} (強制リロード: {ForceLoad})", url, forceLoad);
        
        try
        {
            _navigationManager.NavigateTo(url, forceLoad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NavigateTo中にエラーが発生: {Url}", url);
            
            // エラー時はホーム画面に遷移
            try
            {
                _navigationManager.NavigateTo("/");
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "フォールバック画面遷移も失敗");
            }
        }
    }
    
    /// <summary>
    /// ブラウザの戻るボタンと同等の操作を実行
    /// </summary>
    public void NavigateBack(string? defaultUrl = "/departments")
    {
        _logger.LogInformation("戻る操作実行 (デフォルト: {DefaultUrl})", defaultUrl);
        
        try
        {
            // JavaScript履歴APIを使用した戻る操作
            // 実装方法はBlazorのバージョンや要件により異なる
            // ここでは簡易実装としてデフォルトURLに遷移
            NavigateTo(defaultUrl ?? "/departments");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NavigateBack中にエラーが発生");
            NavigateTo("/departments");
        }
    }
    
    /// <summary>
    /// 複数のエラーメッセージを統合して表示
    /// </summary>
    public void ShowMultipleErrors(IEnumerable<string> errors, string? title = null)
    {
        if (errors == null || !errors.Any())
        {
            _logger.LogWarning("ShowMultipleErrors: エラーリストが空です");
            return;
        }
        
        var errorList = errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
        if (!errorList.Any())
        {
            return;
        }
        
        _logger.LogWarning("複数エラーメッセージ表示: {ErrorCount}件", errorList.Count);
        
        try
        {
            // タイトルがある場合は含める
            var message = string.IsNullOrWhiteSpace(title) 
                ? string.Join("\n", errorList)
                : $"{title}\n{string.Join("\n", errorList)}";
            
            ShowError(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ShowMultipleErrors中にエラーが発生");
            
            // フォールバック: 最初のエラーメッセージのみ表示
            ShowError(errorList.First());
        }
    }
    
    /// <summary>
    /// 長時間処理の開始を示すローディング表示
    /// </summary>
    public IDisposable ShowLoading(string message = "処理中...")
    {
        _logger.LogDebug("ローディング表示開始: {Message}", message);
        
        try
        {
            // Snackbarを使ったシンプルなローディング表示
            // より高度な実装では専用のローディングコンポーネントを使用
            var snackbar = _snackbar.Add(message, Severity.Info, config =>
            {
                config.ShowCloseIcon = false;
                config.RequireInteraction = true; // 自動で消えないように設定
            });
            
            return new LoadingDisposable(snackbar, _logger);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ShowLoading中にエラーが発生");
            return new EmptyDisposable();
        }
    }
    
    /// <summary>
    /// 現在表示中のSnackbarメッセージを全てクリア
    /// </summary>
    public void ClearMessages()
    {
        _logger.LogDebug("全メッセージクリア実行");
        
        try
        {
            _snackbar.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ClearMessages中にエラーが発生");
        }
    }
    
    #endregion
    
    #region Private Helper Methods
    
    /// <summary>
    /// Snackbar設定を作成
    /// </summary>
    /// <param name="duration">表示時間（ミリ秒）</param>
    /// <returns>Snackbar設定</returns>
    private Action<SnackbarOptions> CreateSnackbarConfig(int? duration)
    {
        return config =>
        {
            config.ShowCloseIcon = true;
            config.VisibleStateDuration = duration ?? 5000; // デフォルト5秒
            config.HideTransitionDuration = 500;
        };
    }
    
    #endregion
}

#region Helper Classes

/// <summary>
/// ローディング制御用のDisposableクラス
/// using文での自動リソース管理に使用
/// </summary>
internal class LoadingDisposable : IDisposable
{
    private readonly Snackbar _snackbar;
    private readonly ILogger _logger;
    private bool _disposed = false;
    
    public LoadingDisposable(Snackbar snackbar, ILogger logger)
    {
        _snackbar = snackbar;
        _logger = logger;
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            try
            {
                _snackbar?.Dispose();
                _logger.LogDebug("ローディング表示終了");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ローディングDispose中にエラーが発生");
            }
            
            _disposed = true;
        }
    }
}

/// <summary>
/// 何もしないDisposableクラス
/// エラー時のフォールバックとして使用
/// </summary>
internal class EmptyDisposable : IDisposable
{
    public void Dispose()
    {
        // 何もしない
    }
}

#endregion
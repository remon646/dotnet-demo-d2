using EmployeeManagement.Application.Interfaces;
using Microsoft.AspNetCore.Components;

namespace EmployeeManagement.Components;

/// <summary>
/// 認証が必要なページの基底クラス
/// このクラスを継承することで、認証チェックと未認証時の自動リダイレクトが行われます
/// </summary>
public class AuthRequiredComponentBase : ComponentBase
{
    [Inject] protected IAuthenticationService AuthService { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected bool IsLoading { get; set; } = true;
    protected bool IsAuthenticated { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        await CheckAuthenticationAsync();
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // 初回レンダリング後に再度認証チェック（セッション状態を確実に確認）
            await CheckAuthenticationAsync();
            StateHasChanged();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    /// <summary>
    /// 認証状態をチェックし、未認証の場合はログイン画面にリダイレクト
    /// </summary>
    protected virtual async Task CheckAuthenticationAsync()
    {
        try
        {
            IsLoading = true;
            IsAuthenticated = await AuthService.IsAuthenticatedAsync();
            
            if (!IsAuthenticated)
            {
                Console.WriteLine($"Unauthorized access to {Navigation.Uri}, redirecting to login");
                Navigation.NavigateTo("/login", replace: true);
                return;
            }
            
            // 認証済みの場合は、派生クラスの初期化処理を実行
            await OnAuthenticatedAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Authentication check failed: {ex.Message}");
            Navigation.NavigateTo("/login", replace: true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 認証が確認された後に実行される処理
    /// 派生クラスでオーバーライドして、ページ固有の初期化処理を実装
    /// </summary>
    protected virtual async Task OnAuthenticatedAsync()
    {
        // 派生クラスでオーバーライド
        await Task.CompletedTask;
    }

    /// <summary>
    /// 管理者権限が必要かチェック
    /// </summary>
    /// <returns>管理者の場合true、それ以外false</returns>
    protected async Task<bool> RequireAdminAsync()
    {
        if (!IsAuthenticated)
        {
            return false;
        }

        var isAdmin = await AuthService.IsAdminAsync();
        if (!isAdmin)
        {
            Console.WriteLine($"Admin access required for {Navigation.Uri}, user is not admin");
            Navigation.NavigateTo("/", replace: true);
            return false;
        }

        return true;
    }
}
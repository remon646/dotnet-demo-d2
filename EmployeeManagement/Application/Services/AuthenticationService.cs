using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Models;
using System.Collections.Concurrent;

namespace EmployeeManagement.Application.Services;

public class AuthenticationService : Application.Interfaces.IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;
    
    // セッション管理用の定数
    private const string USER_SESSION_KEY = "CurrentUser";
    private const string AUTH_SESSION_KEY = "IsAuthenticated";
    
    // フォールバック用グローバル状態（デモ用 - 本番環境では削除）
    private static User? _fallbackCurrentUser = null;

    public AuthenticationService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor, IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
    }

    public async Task<bool> LoginAsync(string userId, string password)
    {
        var ipAddress = GetClientIpAddress();
        
        if (await _userRepository.ValidateUserAsync(userId, password))
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                // セッションに認証情報を保存
                SetUserSession(user);
                
                // フォールバック用のグローバル状態も更新
                _fallbackCurrentUser = user;
                
                Console.WriteLine($"User {userId} authenticated and stored in session");
                await _userRepository.UpdateLastLoginAsync(userId);
                
                // 成功ログイン監査ログを記録
                await _auditLogService.LogLoginAsync(user.UserId, user.DisplayName, ipAddress, true);
                
                return true;
            }
        }
        
        // 失敗ログイン監査ログを記録
        await _auditLogService.LogLoginAsync(userId, userId, ipAddress, false);
        
        return false;
    }

    public async Task LogoutAsync()
    {
        var user = await GetCurrentUserAsync();
        var userId = user?.UserId;
        var userName = user?.DisplayName ?? userId ?? "Unknown";
        var ipAddress = GetClientIpAddress();
        
        // セッションから認証情報をクリア
        ClearUserSession();
        
        // グローバル状態もクリア
        _fallbackCurrentUser = null;
        
        Console.WriteLine($"User {userId} logged out - session and global state cleared");
        
        // ログアウト監査ログを記録
        if (!string.IsNullOrEmpty(userId))
        {
            await _auditLogService.LogLogoutAsync(userId, userName, ipAddress);
        }
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        // まずセッションから取得を試行
        var sessionUser = GetUserFromSession();
        if (sessionUser != null)
        {
            return sessionUser;
        }
        
        // セッションにない場合はフォールバック用グローバル状態を確認
        return _fallbackCurrentUser;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        // セッションでの認証チェック
        if (IsAuthenticatedInSession())
        {
            return true;
        }
        
        // フォールバック用グローバル状態チェック
        return _fallbackCurrentUser != null;
    }

    public async Task<bool> IsAdminAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.IsAdmin ?? false;
    }

    private void SetUserSession(User user)
    {
        try
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.SetString(USER_SESSION_KEY, System.Text.Json.JsonSerializer.Serialize(user));
                session.SetString(AUTH_SESSION_KEY, "true");
                Console.WriteLine($"User session set for {user.UserId}");
            }
            else
            {
                Console.WriteLine("Warning: HttpContext.Session is null, using fallback authentication");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting user session: {ex.Message}");
        }
    }

    private User? GetUserFromSession()
    {
        try
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                var userJson = session.GetString(USER_SESSION_KEY);
                if (!string.IsNullOrEmpty(userJson))
                {
                    return System.Text.Json.JsonSerializer.Deserialize<User>(userJson);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting user from session: {ex.Message}");
        }
        return null;
    }

    private bool IsAuthenticatedInSession()
    {
        try
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                var authStatus = session.GetString(AUTH_SESSION_KEY);
                return authStatus == "true";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking session authentication: {ex.Message}");
        }
        return false;
    }

    private void ClearUserSession()
    {
        try
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.Remove(USER_SESSION_KEY);
                session.Remove(AUTH_SESSION_KEY);
                session.Clear(); // 完全にセッションをクリア
                Console.WriteLine("User session cleared completely");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing user session: {ex.Message}");
        }
    }

    /// <summary>
    /// クライアントのIPアドレスを取得します
    /// </summary>
    /// <returns>IPアドレス文字列</returns>
    private string GetClientIpAddress()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return "Unknown";

            // X-Forwarded-For ヘッダーをチェック（プロキシ経由の場合）
            var xForwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                return xForwardedFor.Split(',')[0].Trim();
            }

            // X-Real-IP ヘッダーをチェック
            var xRealIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp))
            {
                return xRealIp;
            }

            // RemoteIpAddress を使用
            var remoteIp = httpContext.Connection.RemoteIpAddress;
            if (remoteIp != null)
            {
                return remoteIp.ToString();
            }

            return "Unknown";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting client IP address: {ex.Message}");
            return "Unknown";
        }
    }
}
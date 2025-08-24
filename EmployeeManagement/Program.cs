using EmployeeManagement.Components;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Application.Services;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Infrastructure.DataStores;
using EmployeeManagement.Infrastructure.Repositories;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add SignalR for Blazor Server
builder.Services.AddSignalR();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

// Add session support for authentication
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // セッション有効期限30分
    options.Cookie.HttpOnly = true; // XSS攻撃を防ぐためにHttpOnlyに設定
    options.Cookie.IsEssential = true; // GDPR対応で必須Cookieとして設定
    options.Cookie.SameSite = SameSiteMode.Strict; // CSRF攻撃を防ぐ
});

// Register data store as singleton (for in-memory persistence)
builder.Services.AddSingleton<ConcurrentInMemoryDataStore>();

// Register repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmployeeNumberRepository, EmployeeNumberRepository>();
builder.Services.AddScoped<IDepartmentHistoryRepository, DepartmentHistoryRepository>();

// Register role and permission repositories
builder.Services.AddScoped<IRoleRepository, EmployeeManagement.Infrastructure.Repositories.InMemoryRoleRepository>();
builder.Services.AddScoped<IPermissionRepository, EmployeeManagement.Infrastructure.Repositories.InMemoryPermissionRepository>();

// Register audit log repository and service
builder.Services.AddScoped<IAuditLogRepository, EmployeeManagement.Infrastructure.Repositories.InMemoryAuditLogRepository>();
builder.Services.AddScoped<IAuditLogService, EmployeeManagement.Application.Services.AuditLogService>();

// Register notification repositories
builder.Services.AddScoped<INotificationRepository, EmployeeManagement.Infrastructure.Repositories.InMemoryNotificationRepository>();
builder.Services.AddScoped<INotificationTemplateRepository, EmployeeManagement.Infrastructure.Repositories.InMemoryNotificationTemplateRepository>();
builder.Services.AddScoped<INotificationSettingsRepository, EmployeeManagement.Infrastructure.Repositories.InMemoryNotificationSettingsRepository>();

// Register notification services
builder.Services.AddScoped<INotificationService, EmployeeManagement.Application.Services.NotificationService>();
builder.Services.AddScoped<INotificationDeliveryService, EmployeeManagement.Application.Services.NotificationDeliveryService>();

// Register application services - 既存サービス
builder.Services.AddScoped<EmployeeManagement.Application.Interfaces.IAuthenticationService, EmployeeManagement.Application.Services.AuthenticationService>();
builder.Services.AddScoped<EmployeeNumberService>();
builder.Services.AddScoped<DepartmentHistoryService>();

// Register employee delete service - 社員削除機能
builder.Services.AddScoped<IEmployeeDeleteService, EmployeeDeleteService>();

// Register new application services - 部門編集機能の疎結合サービス
// バリデーションサービス
builder.Services.AddScoped<IManagerValidationService, ManagerValidationService>();
builder.Services.AddScoped<IDepartmentValidationService, DepartmentValidationService>();

// データサービス
builder.Services.AddScoped<IDepartmentDataService, DepartmentDataService>();
builder.Services.AddScoped<IEmployeeSearchService, EmployeeSearchService>();
builder.Services.AddScoped<IDepartmentSearchService, DepartmentSearchService>();

// UIサービス
builder.Services.AddScoped<IDepartmentUIService, DepartmentUIService>();

// ViewModels（状態管理）
builder.Services.AddScoped<EmployeeManagement.ViewModels.DepartmentEditViewModel>();

// Authorization services
builder.Services.AddScoped<IAuthorizationService, EmployeeManagement.Application.Services.AuthorizationService>();
builder.Services.AddScoped<IRoleInitializationService, RoleInitializationService>();

// メモリキャッシュサービス（社員検索用）
builder.Services.AddMemoryCache();

var app = builder.Build();

// Initialize roles and permissions on startup
using (var scope = app.Services.CreateScope())
{
    var roleInitService = scope.ServiceProvider.GetRequiredService<IRoleInitializationService>();
    await roleInitService.InitializeSystemRolesAsync("system");
    
    // Assign admin role to existing admin user
    await roleInitService.CreateAdminUserAsync("admin", "system");
    await roleInitService.AssignDefaultUserRoleAsync("user", "system");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

// セッションミドルウェアを追加（UseStaticFilesの後、UseAntiforgeryの前）
app.UseSession();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map SignalR hub
app.MapHub<EmployeeManagement.Infrastructure.Hubs.NotificationHub>("/notificationhub");

app.Run();

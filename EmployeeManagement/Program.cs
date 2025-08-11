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

// Register application services - 既存サービス
builder.Services.AddScoped<EmployeeManagement.Application.Interfaces.IAuthenticationService, EmployeeManagement.Application.Services.AuthenticationService>();
builder.Services.AddScoped<EmployeeNumberService>();
builder.Services.AddScoped<DepartmentHistoryService>();

// Register new application services - 部門編集機能の疎結合サービス
// バリデーションサービス
builder.Services.AddScoped<IManagerValidationService, ManagerValidationService>();
builder.Services.AddScoped<IDepartmentValidationService, DepartmentValidationService>();

// データサービス
builder.Services.AddScoped<IDepartmentDataService, DepartmentDataService>();
builder.Services.AddScoped<IEmployeeSearchService, EmployeeSearchService>();

// UIサービス
builder.Services.AddScoped<IDepartmentUIService, DepartmentUIService>();

// ViewModels（状態管理）
builder.Services.AddScoped<EmployeeManagement.ViewModels.DepartmentEditViewModel>();

// メモリキャッシュサービス（社員検索用）
builder.Services.AddMemoryCache();

var app = builder.Build();

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

app.Run();

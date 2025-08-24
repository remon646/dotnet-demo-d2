# 通知システム実装計画

## 概要
リアルタイム通知機能とアラートシステムを実装し、ユーザーへの情報伝達を強化します。SignalRを活用したリアルタイム通信を中心とした通知システムを構築します。

## 機能要件

### 基本機能
- ✅ リアルタイム通知（SignalR活用）
- ✅ システム通知（メンテナンス、アップデート等）
- ✅ データ変更通知（社員・部署の作成・更新・削除）
- ✅ 通知一覧表示・管理
- ✅ 通知の既読/未読管理
- ✅ 通知設定（有効/無効、種類別設定）

### アラート機能
- ✅ 操作確認アラート
- ✅ エラー通知
- ✅ 成功メッセージ
- ✅ 警告通知
- ✅ 自動消去タイマー

### 高度な機能
- ✅ 通知のフィルタリング・検索
- ✅ 通知の一括操作（既読化、削除）
- ✅ 通知テンプレート管理
- ✅ 通知配信ルール設定
- ✅ 通知統計・分析

## 技術実装詳細

### 1. ドメインモデル

#### 通知モデル
```csharp
public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public string UserId { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsRead { get; set; }
    public string ActionUrl { get; set; }
    public string ActionText { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public DateTime? ExpiresAt { get; set; }
    public bool IsPersistent { get; set; } = true;
}

public enum NotificationType
{
    System = 1,
    DataChange = 2,
    UserAction = 3,
    Alert = 4,
    Warning = 5,
    Error = 6,
    Success = 7
}

public enum NotificationPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Critical = 4
}
```

#### 通知設定モデル
```csharp
public class NotificationSettings
{
    public string UserId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public Dictionary<NotificationType, bool> TypeSettings { get; set; } = new();
    public bool PlaySound { get; set; } = true;
    public bool ShowDesktopNotification { get; set; } = true;
    public int AutoDismissSeconds { get; set; } = 5;
    public NotificationPriority MinimumPriority { get; set; } = NotificationPriority.Low;
}
```

#### 通知テンプレート
```csharp
public class NotificationTemplate
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string TitleTemplate { get; set; }
    public string MessageTemplate { get; set; }
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public bool IsActive { get; set; }
    public List<NotificationTemplateParameter> Parameters { get; set; } = new();
}

public class NotificationTemplateParameter
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsRequired { get; set; }
    public string DefaultValue { get; set; }
}
```

### 2. SignalR ハブ実装

#### 通知ハブ
```csharp
public class NotificationHub : Hub
{
    private readonly INotificationService _notificationService;

    public NotificationHub(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
    }

    public async Task LeaveUserGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
    }

    public async Task MarkAsRead(int notificationId)
    {
        await _notificationService.MarkAsReadAsync(notificationId, Context.UserIdentifier);
    }

    public async Task MarkAllAsRead(string userId)
    {
        await _notificationService.MarkAllAsReadAsync(userId);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await JoinUserGroup(userId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await LeaveUserGroup(userId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}
```

### 3. サービス層実装

#### 通知サービス
```csharp
public interface INotificationService
{
    Task<int> CreateNotificationAsync(Notification notification);
    Task<int> CreateFromTemplateAsync(string templateName, string userId, Dictionary<string, string> parameters);
    Task<bool> SendNotificationAsync(Notification notification);
    Task<bool> SendToUserAsync(string userId, string title, string message, NotificationType type = NotificationType.System);
    Task<bool> SendToAllUsersAsync(string title, string message, NotificationType type = NotificationType.System);
    Task<List<Notification>> GetUserNotificationsAsync(string userId, bool includeRead = true, int pageSize = 50, int page = 1);
    Task<int> GetUnreadCountAsync(string userId);
    Task<bool> MarkAsReadAsync(int notificationId, string userId);
    Task<bool> MarkAllAsReadAsync(string userId);
    Task<bool> DeleteNotificationAsync(int notificationId, string userId);
    Task<bool> DeleteAllReadAsync(string userId);
    Task<NotificationSettings> GetUserSettingsAsync(string userId);
    Task<bool> UpdateUserSettingsAsync(NotificationSettings settings);
}
```

#### 通知配信サービス
```csharp
public interface INotificationDeliveryService
{
    Task DeliverAsync(Notification notification);
    Task DeliverToUserAsync(string userId, Notification notification);
    Task DeliverToGroupAsync(string groupName, Notification notification);
    Task DeliverBroadcastAsync(Notification notification);
}
```

### 4. UI コンポーネント

#### 通知ベル（ヘッダー）
```razor
<!-- NotificationBell.razor -->
<MudBadge Content="@_unreadCount" Color="Color.Error" Visible="@(_unreadCount > 0)">
    <MudIconButton Icon="@Icons.Material.Filled.Notifications" 
                   Color="Color.Inherit" 
                   OnClick="@ToggleNotificationPanel" />
</MudBadge>

@if (_showPanel)
{
    <MudPopover Open="true" AnchorOrigin="Origin.BottomLeft" TransformOrigin="Origin.TopLeft">
        <NotificationPanel OnClose="@CloseNotificationPanel" />
    </MudPopover>
}
```

#### 通知パネル
```razor
<!-- NotificationPanel.razor -->
<MudPaper Class="notification-panel" Elevation="8">
    <MudList>
        <MudListSubheader>
            通知 (@_notifications.Count(n => !n.IsRead) 件未読)
            <MudSpacer />
            <MudButton Size="Size.Small" OnClick="@MarkAllAsRead">全て既読</MudButton>
        </MudListSubheader>
        
        @foreach (var notification in _notifications.Take(10))
        {
            <NotificationItem Notification="notification" OnClick="@HandleNotificationClick" />
        }
        
        @if (_notifications.Count > 10)
        {
            <MudListItem>
                <MudButton FullWidth="true" Variant="Variant.Text" Href="/notifications">
                    全ての通知を表示 (@_notifications.Count 件)
                </MudButton>
            </MudListItem>
        }
    </MudList>
</MudPaper>
```

#### トースト通知
```razor
<!-- ToastNotification.razor -->
<MudSnackbarProvider />

@code {
    [Inject] private ISnackbar Snackbar { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await HubConnection.StartAsync();
        
        HubConnection.On<Notification>("ReceiveNotification", (notification) =>
        {
            if (!notification.IsPersistent)
            {
                ShowToast(notification);
            }
            InvokeAsync(StateHasChanged);
        });
    }
    
    private void ShowToast(Notification notification)
    {
        var severity = GetSeverity(notification.Type);
        Snackbar.Add(notification.Message, severity, config =>
        {
            config.RequireInteraction = notification.Priority == NotificationPriority.Critical;
            config.ShowCloseIcon = true;
        });
    }
}
```

### 5. 自動通知トリガー

#### データ変更通知
```csharp
public class DataChangeNotificationService : IDataChangeNotificationService
{
    private readonly INotificationService _notificationService;
    private readonly IHubContext<NotificationHub> _hubContext;

    public async Task NotifyEmployeeCreatedAsync(Employee employee, string createdBy)
    {
        var notification = new Notification
        {
            Title = "新しい社員が登録されました",
            Message = $"{employee.Name} さんが登録されました。",
            Type = NotificationType.DataChange,
            Priority = NotificationPriority.Normal,
            CreatedBy = createdBy,
            ActionUrl = $"/employees/edit/{employee.Id}",
            ActionText = "詳細を見る"
        };

        await _notificationService.SendToAllUsersAsync(notification.Title, notification.Message, notification.Type);
    }

    public async Task NotifyEmployeeUpdatedAsync(Employee employee, string updatedBy)
    {
        // 類似実装
    }

    public async Task NotifyEmployeeDeletedAsync(string employeeName, string deletedBy)
    {
        // 類似実装
    }
}
```

## ファイル変更計画

### 新規作成ファイル

1. **ドメイン層**
   - `Domain/Models/Notification.cs`
   - `Domain/Models/NotificationSettings.cs`
   - `Domain/Models/NotificationTemplate.cs`
   - `Domain/Interfaces/INotificationRepository.cs`

2. **アプリケーション層**
   - `Application/Interfaces/INotificationService.cs`
   - `Application/Interfaces/INotificationDeliveryService.cs`
   - `Application/Interfaces/IDataChangeNotificationService.cs`
   - `Application/Services/NotificationService.cs`
   - `Application/Services/NotificationDeliveryService.cs`
   - `Application/Services/DataChangeNotificationService.cs`

3. **インフラストラクチャー層**
   - `Infrastructure/Repositories/InMemoryNotificationRepository.cs`
   - `Infrastructure/Hubs/NotificationHub.cs`

4. **UI コンポーネント**
   - `Components/Shared/NotificationBell.razor`
   - `Components/Shared/NotificationPanel.razor`
   - `Components/Shared/NotificationItem.razor`
   - `Components/Shared/ToastNotification.razor`
   - `Components/Pages/Notifications.razor`
   - `Components/Pages/NotificationSettings.razor`
   - `Components/Dialogs/NotificationDetailDialog.razor`

5. **ViewModels**
   - `ViewModels/NotificationViewModel.cs`
   - `ViewModels/NotificationSettingsViewModel.cs`

### 既存ファイル変更

1. **レイアウト拡張**
   - `Components/Layout/MainLayout.razor` (通知ベル追加)

2. **既存サービス拡張**
   - `Application/Services/EmployeeService.cs` (通知トリガー追加)
   - `Application/Services/DepartmentService.cs` (通知トリガー追加)

3. **データストア拡張**
   - `Infrastructure/DataStores/ConcurrentInMemoryDataStore.cs`

4. **依存性注入**
   - `Program.cs` (SignalR、通知サービス追加)

5. **設定ファイル**
   - `appsettings.json` (通知設定追加)

6. **ナビゲーション**
   - `Components/Layout/NavMenu.razor`

## 実装手順

### Phase 1: 基盤実装（優先度：最高）
1. Notification ドメインモデル作成
2. NotificationHub SignalR実装
3. 基本的な通知サービス
4. インメモリリポジトリ実装

### Phase 2: UI基本機能（優先度：高）
1. NotificationBell コンポーネント
2. NotificationPanel 実装
3. トースト通知システム
4. 通知一覧ページ

### Phase 3: 自動通知（優先度：高）
1. データ変更通知サービス
2. 既存サービスとの統合
3. システム通知機能
4. エラー・警告通知

### Phase 4: 高度機能（優先度：中）
1. 通知設定画面
2. 通知テンプレート機能
3. 通知フィルタリング
4. 一括操作機能

### Phase 5: 最適化（優先度：低）
1. パフォーマンス最適化
2. 通知統計機能
3. プッシュ通知準備
4. 通知アーカイブ

## テスト項目

### 単体テスト
- NotificationService メソッドテスト
- SignalR ハブ機能テスト
- 通知配信ロジックテスト
- テンプレート処理テスト

### 統合テスト
- リアルタイム通知フローテスト
- 複数ユーザー間通知テスト
- データ変更時の自動通知テスト

### UI テスト
- 通知パネル表示動作
- 未読数表示テスト
- トースト通知表示テスト

## 技術的考慮事項

### リアルタイム通信
- SignalR 接続管理
- 接続断時の再接続処理
- スケールアウト対応（Redis Backplane）

### パフォーマンス
- 大量通知時のメモリ使用量
- SignalR 接続数制限
- 通知配信の効率化

### ユーザー体験
- 通知の優先度制御
- 過剰な通知の抑制
- 直感的な通知UI

## 必要なNuGetパッケージ
```xml
<!-- SignalR (既存） -->
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.1.0" />
<!-- Redis Backplane (将来的) -->
<PackageReference Include="Microsoft.AspNetCore.SignalR.StackExchangeRedis" Version="7.0.0" />
```

## リスクと対策

### 技術的リスク
- **SignalR接続問題**: 接続断、再接続処理
- **通知スパム**: 大量通知によるパフォーマンス影響
- **ブラウザ制限**: 通知権限、バックグラウンド制限

### 対策
- 堅牢な再接続メカニズム
- 通知レート制限機能
- 適切な通知グループ分け
- フォールバック機能実装

## 見積もり
- **開発工数**: 4-5日
- **テスト工数**: 2-3日
- **総工数**: 6-8日

## 依存関係
- 監査ログ（通知操作ログ記録）
- 権限管理（通知送信権限制御）
- ファイルアップロード（アップロード完了通知）

## 成功指標
- ✅ 通知配信成功率 > 99%
- ✅ リアルタイム通知遅延 < 1秒
- ✅ 通知の既読率 > 80%
- ✅ ユーザー満足度（通知の有用性）
- ✅ システム安定性（SignalR接続維持率）

## 将来拡張
- プッシュ通知対応
- メール通知統合
- Slack/Teams連携
- 通知AI（重要度自動判定）
- 通知分析・レポート
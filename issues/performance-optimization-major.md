# パフォーマンス最適化 - Major課題

## Issue概要
部門管理画面UI一貫性改善の実装において、パフォーマンス分析で特定された重要度の高い最適化課題です。

## 課題レベル
🟡 **Major (重要度高)** - 短期間での対応が必要

## 特定された問題

### 1. 全件データ取得によるパフォーマンス劣化
**影響度**: 🟡 **High**  
**対象ファイル**: `Components/Dialogs/EmployeeSearchDialog.razor` (187-202行)

#### 問題詳細
```csharp
// 現在の実装
var allEmployees = await EmployeeRepository.GetAllAsync();
searchResults = allEmployees.Where(emp => 
    (string.IsNullOrWhiteSpace(searchCriteria.EmployeeNumber) || 
     emp.EmployeeNumber.Contains(searchCriteria.EmployeeNumber, StringComparison.OrdinalIgnoreCase)) &&
    (string.IsNullOrWhiteSpace(searchCriteria.Name) || 
     emp.Name.Contains(searchCriteria.Name, StringComparison.OrdinalIgnoreCase))
).OrderBy(emp => emp.EmployeeNumber);
```

**問題点**:
- 大量データ環境でのパフォーマンス劣化
- メモリ使用量の増大
- ネットワーク帯域の無駄な消費
- レスポンス時間の遅延

#### 推奨実装
```csharp
// リポジトリレイヤーでフィルタリング実装
public interface IEmployeeRepository
{
    Task<PagedResult<Employee>> SearchAsync(EmployeeSearchCriteria criteria, int page = 1, int pageSize = 20);
}

// コンポーネントでの使用
var searchResult = await EmployeeRepository.SearchAsync(searchCriteria, currentPage, PAGE_SIZE);
searchResults = searchResult.Items;
totalCount = searchResult.TotalCount;
```

### 2. メモリリーク対策の強化
**影響度**: 🟡 **High**  
**対象ファイル**: `Components/Pages/DepartmentEdit.razor` (308行)

#### 問題詳細
```csharp
// 現在の実装
allEmployees = (await EmployeeRepository.GetAllAsync()).ToList();
```

**問題点**:
- 大きなコレクションがコンポーネント生存期間中メモリに保持
- ガベージコレクション負荷の増大
- メモリ不足リスクの増大

#### 推奨実装
```csharp
// 遅延読み込みとキャッシュ戦略
private readonly Lazy<Task<IEnumerable<Employee>>> _employeesLazy;

public DepartmentEdit()
{
    _employeesLazy = new Lazy<Task<IEnumerable<Employee>>>(
        () => EmployeeRepository.GetAllAsync());
}

// または IAsyncEnumerable の使用
private async IAsyncEnumerable<Employee> GetEmployeesAsync()
{
    await foreach (var employee in EmployeeRepository.GetAllAsyncEnumerable())
    {
        yield return employee;
    }
}
```

### 3. 非同期処理の改善
**影響度**: 🟡 **Medium**  
**対象ファイル**: `Components/Pages/DepartmentEdit.razor` (650-652行)

#### 問題詳細
```csharp
// 現在の実装
await Task.Delay(1000);
Navigation.NavigateTo("/departments");
```

**問題点**:
- UIスレッドをブロックする可能性
- ユーザーエクスペリエンスの低下
- レスポンシブネスの問題

#### 推奨実装
```csharp
// UIスレッド非ブロック実装
_ = Task.Run(async () =>
{
    await Task.Delay(1000);
    await InvokeAsync(() => Navigation.NavigateTo("/departments"));
});

// または、タイマーベースの実装
private System.Timers.Timer? _navigationTimer;

private void ScheduleNavigation()
{
    _navigationTimer = new System.Timers.Timer(1000) { AutoReset = false };
    _navigationTimer.Elapsed += (_, _) => InvokeAsync(() => Navigation.NavigateTo("/departments"));
    _navigationTimer.Start();
}
```

### 4. オートコンプリート検索の最適化
**影響度**: 🟡 **Medium**  
**対象ファイル**: `Components/Pages/DepartmentEdit.razor` (423-436行)

#### 問題詳細
```csharp
// 現在の実装
private async Task<IEnumerable<Employee>> SearchEmployees(string value, CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
        return new List<Employee>();

    var results = allEmployees.Where(e => 
        e.EmployeeNumber.Contains(value, StringComparison.OrdinalIgnoreCase) ||
        e.Name.Contains(value, StringComparison.OrdinalIgnoreCase)
    ).Take(10);
    
    return await Task.FromResult(results);
}
```

**問題点**:
- 全件インメモリ検索による負荷
- リアルタイム検索時の処理遅延
- キャンセレーショントークンの未活用

#### 推奨実装
```csharp
private readonly SemaphoreSlim _searchSemaphore = new(1, 1);
private CancellationTokenSource? _lastSearchCts;

private async Task<IEnumerable<Employee>> SearchEmployees(string value, CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(value) || value.Length < MIN_SEARCH_LENGTH)
        return Enumerable.Empty<Employee>();

    // 前回の検索をキャンセル
    _lastSearchCts?.Cancel();
    _lastSearchCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

    await _searchSemaphore.WaitAsync(_lastSearchCts.Token);
    try
    {
        // デバウンス処理
        await Task.Delay(300, _lastSearchCts.Token);
        
        // データベース検索
        return await EmployeeRepository.SearchAsync(value, MAX_SEARCH_RESULTS, _lastSearchCts.Token);
    }
    finally
    {
        _searchSemaphore.Release();
    }
}
```

## 技術的対応手順

### 段階1: パフォーマンス測定 (1-2日)
1. **ベンチマーク測定**
   ```csharp
   [Fact]
   public async Task MeasureSearchPerformance()
   {
       var stopwatch = Stopwatch.StartNew();
       var result = await EmployeeRepository.GetAllAsync();
       stopwatch.Stop();
       
       Assert.True(stopwatch.ElapsedMilliseconds < 1000); // 1秒以内
   }
   ```

2. **メモリプロファイリング**
   - Visual Studio Diagnostic Tools使用
   - dotMemory Profilerによる詳細分析

### 段階2: 最適化実装 (3-5日)
1. **リポジトリレイヤー最適化**
2. **ページネーション実装**
3. **キャッシュ戦略導入**
4. **非同期処理改善**

### 段階3: パフォーマンステスト (1-2日)
1. **負荷テスト実行**
2. **メモリリークテスト**
3. **レスポンス時間測定**

## 実装提案

### 1. リポジトリインターフェース拡張
```csharp
public interface IEmployeeRepository
{
    // 既存
    Task<IEnumerable<Employee>> GetAllAsync();
    
    // 新規追加
    Task<PagedResult<Employee>> SearchAsync(EmployeeSearchCriteria criteria, int page = 1, int pageSize = 20);
    Task<IEnumerable<Employee>> SearchAsync(string query, int maxResults = 10, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Employee> GetAllAsyncEnumerable();
}
```

### 2. ページング結果クラス
```csharp
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
```

### 3. キャッシュサービス
```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
}
```

## パフォーマンス目標

### レスポンス時間目標
| 操作 | 現在 | 目標 | 改善率 |
|------|------|------|--------|
| 社員検索 | 2-5秒 | <500ms | 75-90% |
| 部門読み込み | 1-3秒 | <300ms | 70-90% |
| データ保存 | 1-2秒 | <500ms | 50-75% |

### メモリ使用量目標
| 項目 | 現在 | 目標 | 改善率 |
|------|------|------|--------|
| 社員データ | 全件メモリ | 必要分のみ | 80-95% |
| ページメモリ | 大きな変動 | 安定 | N/A |

## 検証項目チェックリスト

### パフォーマンス
- [ ] 検索レスポンス時間 < 500ms
- [ ] ページ読み込み時間 < 300ms
- [ ] メモリ使用量の安定化
- [ ] CPUリソース使用量の最適化

### スケーラビリティ
- [ ] 1000件データでの動作確認
- [ ] 10000件データでの動作確認
- [ ] 同時アクセス負荷テスト
- [ ] 長時間運用テスト

### ユーザビリティ
- [ ] UI応答性の改善確認
- [ ] 検索結果の適切な表示
- [ ] エラーハンドリングの確認

## 影響範囲

### 直接影響
- 社員検索機能
- 部門管理画面
- オートコンプリート機能

### 間接影響
- システム全体のレスポンス性
- サーバーリソース使用量
- ユーザーエクスペリエンス

## 優先度・対応期限

| 課題 | 優先度 | 対応期限 | 担当 |
|------|--------|----------|------|
| 全件取得最適化 | 🟡 高 | 1週間 | バックエンド |
| メモリリーク対策 | 🟡 高 | 1週間 | フロントエンド |
| 非同期処理改善 | 🔵 中 | 2週間 | フロントエンド |
| 検索最適化 | 🔵 中 | 2週間 | フルスタック |

## 完了基準

### 必須条件
- [ ] 全パフォーマンス目標の達成
- [ ] 負荷テスト合格
- [ ] メモリリークテスト合格
- [ ] コードレビュー完了

### 推奨条件
- [ ] パフォーマンス監視の設定
- [ ] 自動パフォーマンステストの統合
- [ ] 運用監視ダッシュボードの更新

## ステータス

- **作成日**: 2025-08-03
- **優先度**: 🟡 Major
- **対応状況**: 🟡 計画中
- **次回レビュー**: 1週間後
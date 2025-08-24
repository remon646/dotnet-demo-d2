# 詳細検索・フィルタリング機能実装計画

## 概要
現在の基本検索機能を大幅に拡張し、高度な検索・フィルタリング機能とデータエクスポート機能を実装します。

## 機能要件

### 基本機能
- ✅ 複数条件による高度な検索
- ✅ 日付範囲検索（入社日、更新日）
- ✅ 数値範囲検索（社員番号範囲）
- ✅ 複数選択フィルタ（部署、役職）
- ✅ 検索条件の保存・読み込み
- ✅ クイック検索（保存された条件）

### エクスポート機能
- ✅ CSV エクスポート
- ✅ Excel エクスポート (.xlsx)
- ✅ PDF エクスポート
- ✅ 検索結果のエクスポート
- ✅ カスタムフィールド選択

### UI/UX機能
- ✅ 検索条件の可視化（タグ表示）
- ✅ 検索履歴
- ✅ 検索結果のハイライト
- ✅ ソート・ページネーション最適化

## 技術実装詳細

### 1. 検索モデル拡張

#### 高度な検索条件モデル
```csharp
public class AdvancedSearchCriteria
{
    public string EmployeeNumber { get; set; }
    public NumberRangeFilter EmployeeNumberRange { get; set; }
    public string Name { get; set; }
    public List<string> DepartmentIds { get; set; } = new();
    public List<string> Positions { get; set; } = new();
    public DateRangeFilter JoinDateRange { get; set; }
    public DateRangeFilter UpdateDateRange { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public EmployeeStatus? Status { get; set; }
    public bool IncludeRetired { get; set; }
    public SearchLogicalOperator LogicalOperator { get; set; } = SearchLogicalOperator.And;
}

public class DateRangeFilter
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public DateRangePreset Preset { get; set; }
}

public class NumberRangeFilter
{
    public int? From { get; set; }
    public int? To { get; set; }
}

public enum SearchLogicalOperator
{
    And = 1,
    Or = 2
}

public enum DateRangePreset
{
    Today = 1,
    ThisWeek = 2,
    ThisMonth = 3,
    ThisYear = 4,
    Last7Days = 5,
    Last30Days = 6,
    Last90Days = 7,
    Custom = 8
}
```

#### 保存済み検索条件
```csharp
public class SavedSearchCriteria
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string UserId { get; set; }
    public AdvancedSearchCriteria Criteria { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUsedAt { get; set; }
    public int UseCount { get; set; }
    public bool IsPublic { get; set; }
}
```

### 2. エクスポート機能

#### エクスポート設定モデル
```csharp
public class ExportSettings
{
    public ExportFormat Format { get; set; }
    public List<string> SelectedFields { get; set; } = new();
    public bool IncludeHeaders { get; set; } = true;
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IncludeTimestamp { get; set; } = true;
    public ExportDateFormat DateFormat { get; set; } = ExportDateFormat.Japanese;
}

public enum ExportFormat
{
    Csv = 1,
    Excel = 2,
    Pdf = 3,
    Json = 4
}

public enum ExportDateFormat
{
    Japanese = 1,      // yyyy/MM/dd
    International = 2, // yyyy-MM-dd
    US = 3            // MM/dd/yyyy
}
```

### 3. サービス層実装

#### 高度検索サービス
```csharp
public interface IAdvancedSearchService
{
    Task<SearchResult<Employee>> SearchEmployeesAsync(AdvancedSearchCriteria criteria, PaginationParams pagination);
    Task<SearchResult<DepartmentMaster>> SearchDepartmentsAsync(AdvancedSearchCriteria criteria, PaginationParams pagination);
    Task<int> SaveSearchCriteriaAsync(SavedSearchCriteria savedCriteria);
    Task<List<SavedSearchCriteria>> GetSavedSearchCriteriaAsync(string userId);
    Task<bool> DeleteSavedSearchCriteriaAsync(int id, string userId);
    Task<SearchStatistics> GetSearchStatisticsAsync(AdvancedSearchCriteria criteria);
}
```

#### エクスポートサービス
```csharp
public interface IExportService
{
    Task<ExportResult> ExportEmployeesAsync(IEnumerable<Employee> employees, ExportSettings settings);
    Task<ExportResult> ExportDepartmentsAsync(IEnumerable<DepartmentMaster> departments, ExportSettings settings);
    Task<ExportResult> ExportSearchResultsAsync<T>(SearchResult<T> searchResults, ExportSettings settings) where T : class;
    Task<List<ExportFieldDefinition>> GetAvailableFieldsAsync<T>() where T : class;
}
```

### 4. UI コンポーネント

#### 高度検索コンポーネント
```razor
<!-- AdvancedSearchPanel.razor -->
<MudExpansionPanels>
    <MudExpansionPanel Text="基本検索">
        <!-- 既存の検索フィールド -->
    </MudExpansionPanel>
    <MudExpansionPanel Text="詳細条件">
        <!-- 日付範囲、数値範囲フィルタ -->
    </MudExpansionPanel>
    <MudExpansionPanel Text="保存された検索">
        <!-- 保存済み検索条件一覧 -->
    </MudExpansionPanel>
</MudExpansionPanels>
```

#### エクスポートダイアログ
```razor
<!-- ExportDialog.razor -->
<MudDialog>
    <DialogContent>
        <!-- フォーマット選択 -->
        <!-- フィールド選択 -->
        <!-- エクスポート設定 -->
    </DialogContent>
    <DialogActions>
        <!-- エクスポート実行 -->
    </DialogActions>
</MudDialog>
```

## ファイル変更計画

### 新規作成ファイル

1. **ドメイン層**
   - `Domain/Models/AdvancedSearchCriteria.cs`
   - `Domain/Models/SavedSearchCriteria.cs`
   - `Domain/Models/ExportSettings.cs`
   - `Domain/Interfaces/IAdvancedSearchRepository.cs`

2. **アプリケーション層**
   - `Application/Interfaces/IAdvancedSearchService.cs`
   - `Application/Interfaces/IExportService.cs`
   - `Application/Services/AdvancedSearchService.cs`
   - `Application/Services/ExportService.cs`
   - `Application/Services/PdfExportService.cs`
   - `Application/Services/ExcelExportService.cs`

3. **インフラストラクチャー層**
   - `Infrastructure/Repositories/InMemoryAdvancedSearchRepository.cs`

4. **UI コンポーネント**
   - `Components/Shared/AdvancedSearchPanel.razor`
   - `Components/Shared/SearchCriteriaTag.razor`
   - `Components/Shared/DateRangePicker.razor`
   - `Components/Shared/NumberRangePicker.razor`
   - `Components/Dialogs/SaveSearchDialog.razor`
   - `Components/Dialogs/ExportDialog.razor`
   - `Components/Dialogs/FieldSelectorDialog.razor`

5. **ViewModels**
   - `ViewModels/AdvancedSearchViewModel.cs`
   - `ViewModels/ExportViewModel.cs`

### 既存ファイル変更

1. **社員一覧ページ拡張**
   - `Components/Pages/Employees.razor`
   - `Components/Pages/Employees.razor.cs`

2. **部署一覧ページ拡張**
   - `Components/Pages/Departments.razor`

3. **既存サービス拡張**
   - `Application/Services/EmployeeService.cs`
   - `Application/Services/DepartmentService.cs`

4. **データストア拡張**
   - `Infrastructure/DataStores/ConcurrentInMemoryDataStore.cs`

5. **依存性注入**
   - `Program.cs`

6. **設定ファイル**
   - `appsettings.json` (エクスポート設定追加)

## 実装手順

### Phase 1: 検索機能拡張（優先度：最高）
1. AdvancedSearchCriteria モデル作成
2. AdvancedSearchService 実装
3. 複数条件検索ロジック
4. 基本UI拡張

### Phase 2: 保存・管理機能（優先度：高）
1. SavedSearchCriteria モデル
2. 検索条件保存機能
3. 保存済み検索UI
4. 検索履歴機能

### Phase 3: エクスポート機能（優先度：高）
1. ExportService 基盤実装
2. CSV エクスポート
3. Excel エクスポート
4. エクスポート設定UI

### Phase 4: PDF・高度機能（優先度：中）
1. PDF エクスポート実装
2. カスタムフィールド選択
3. エクスポートテンプレート
4. バッチエクスポート

### Phase 5: UI/UX最適化（優先度：低）
1. 検索条件タグ表示
2. 検索結果ハイライト
3. パフォーマンス最適化
4. ユーザビリティ向上

## テスト項目

### 単体テスト
- 複数条件検索ロジック
- 日付・数値範囲フィルタ
- エクスポート機能各フォーマット
- 検索条件保存・読み込み

### 統合テスト
- 複合検索シナリオ
- エクスポート フロー テスト
- UI コンポーネント連携

### パフォーマンステスト
- 大量データでの検索性能
- エクスポート処理時間
- メモリ使用量測定

## 技術的考慮事項

### パフォーマンス最適化
- インデックス設計考慮
- 検索クエリ最適化
- ページネーション効率化
- キャッシュ戦略

### ユーザビリティ
- 直感的な検索UI
- 検索結果の可読性
- エクスポート進捗表示
- エラーハンドリング

### 拡張性
- プラグイン型フィルタ
- カスタムエクスポート形式
- 外部システム連携準備

## 必要なNuGetパッケージ
```xml
<!-- Excel処理用 -->
<PackageReference Include="EPPlus" Version="6.2.9" />
<!-- PDF生成用 -->
<PackageReference Include="iTextSharp.LGPLv2.Core" Version="3.4.0" />
<!-- または -->
<PackageReference Include="PuppeteerSharp" Version="11.0.0" />
```

## リスクと対策

### 技術的リスク
- **検索性能**: 複雑な検索条件での性能低下
- **メモリ使用量**: 大量データエクスポート時
- **UI複雑化**: 高度検索UIの使いやすさ

### 対策
- 検索インデックス最適化
- ストリーミングエクスポート
- 段階的UI開示
- パフォーマンス監視

## 見積もり
- **開発工数**: 5-6日
- **テスト工数**: 2-3日
- **総工数**: 7-9日

## 依存関係
- 監査ログ（検索・エクスポート操作記録）
- ファイルアップロード（エクスポートファイル管理）
- 権限管理（検索・エクスポート権限制御）

## 成功指標
- ✅ 検索レスポンス時間 < 2秒
- ✅ エクスポート処理時間 < 30秒（1000件）
- ✅ 検索条件保存利用率 > 70%
- ✅ UI操作の直感性（ユーザビリティテスト）
- ✅ エクスポート成功率 > 99%

## 将来拡張
- 全文検索機能
- 機械学習による検索候補
- リアルタイム検索結果更新
- 検索API公開
- 検索アナリティクス
# レポート機能実装計画

## 概要
データ分析・可視化機能を実装し、社員・部署データの統計レポートとグラフィカルな表示機能を提供します。経営判断に役立つ洞察を得られるレポートシステムを構築します。

## 機能要件

### 基本レポート
- ✅ 部署別社員数レポート
- ✅ 入社年別統計レポート
- ✅ 年齢分布レポート
- ✅ 勤続年数分析レポート
- ✅ 部署別平均勤続年数
- ✅ 退職者統計（退職率、退職理由）

### グラフ・可視化
- ✅ 円グラフ（部署別分布）
- ✅ 棒グラフ（年別入社数推移）
- ✅ 折れ線グラフ（社員数推移）
- ✅ ヒートマップ（部署×年齢分布）
- ✅ ダッシュボード形式の統合表示

### エクスポート・出力
- ✅ PDF レポート出力
- ✅ Excel レポート出力
- ✅ CSV データエクスポート
- ✅ 画像形式でのグラフ保存
- ✅ 定期レポート（スケジュール機能）

### インタラクティブ機能
- ✅ フィルタリング（期間、部署、条件）
- ✅ ドリルダウン機能
- ✅ 比較分析（期間比較、部署比較）
- ✅ カスタムレポート作成

## 技術実装詳細

### 1. レポートモデル

#### 基本レポートモデル
```csharp
public class Report
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ReportType Type { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastGeneratedAt { get; set; }
    public ReportSettings Settings { get; set; }
    public bool IsPublic { get; set; }
    public bool IsScheduled { get; set; }
    public ScheduleSettings Schedule { get; set; }
}

public enum ReportType
{
    EmployeeStatistics = 1,
    DepartmentAnalysis = 2,
    HiringTrends = 3,
    TurnoverAnalysis = 4,
    AgeDistribution = 5,
    TenureAnalysis = 6,
    CustomReport = 7
}
```

#### レポート設定モデル
```csharp
public class ReportSettings
{
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateRange DateRange { get; set; }
    public List<string> SelectedDepartments { get; set; } = new();
    public List<string> SelectedFields { get; set; } = new();
    public ChartSettings ChartSettings { get; set; }
    public ExportSettings ExportSettings { get; set; }
}

public class ChartSettings
{
    public ChartType ChartType { get; set; }
    public string Title { get; set; }
    public string XAxisLabel { get; set; }
    public string YAxisLabel { get; set; }
    public List<string> Colors { get; set; } = new();
    public bool ShowLegend { get; set; } = true;
    public bool ShowDataLabels { get; set; } = true;
}

public enum ChartType
{
    Bar = 1,
    Line = 2,
    Pie = 3,
    Area = 4,
    Scatter = 5,
    Heatmap = 6,
    Gauge = 7
}
```

#### レポートデータモデル
```csharp
public class ReportData
{
    public string Title { get; set; }
    public DateTime GeneratedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<ReportDataSet> DataSets { get; set; } = new();
    public ReportSummary Summary { get; set; }
}

public class ReportDataSet
{
    public string Name { get; set; }
    public List<ReportDataPoint> DataPoints { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ReportDataPoint
{
    public string Label { get; set; }
    public decimal Value { get; set; }
    public DateTime? Date { get; set; }
    public string Category { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class ReportSummary
{
    public int TotalEmployees { get; set; }
    public int TotalDepartments { get; set; }
    public decimal AverageAge { get; set; }
    public decimal AverageTenure { get; set; }
    public decimal TurnoverRate { get; set; }
    public Dictionary<string, object> CustomMetrics { get; set; } = new();
}
```

### 2. サービス層実装

#### レポートサービス
```csharp
public interface IReportService
{
    Task<ReportData> GenerateReportAsync(ReportType reportType, ReportSettings settings);
    Task<List<Report>> GetUserReportsAsync(string userId);
    Task<int> SaveReportAsync(Report report);
    Task<bool> DeleteReportAsync(int reportId, string userId);
    Task<ReportData> GetEmployeeStatisticsAsync(ReportSettings settings);
    Task<ReportData> GetDepartmentAnalysisAsync(ReportSettings settings);
    Task<ReportData> GetHiringTrendsAsync(ReportSettings settings);
    Task<ReportData> GetTurnoverAnalysisAsync(ReportSettings settings);
    Task<ReportData> GetAgeDistributionAsync(ReportSettings settings);
    Task<ReportData> GetTenureAnalysisAsync(ReportSettings settings);
    Task<byte[]> ExportReportAsync(ReportData reportData, ExportFormat format);
}
```

#### データ分析サービス
```csharp
public interface IDataAnalysisService
{
    Task<StatisticsResult> CalculateEmployeeStatisticsAsync(List<Employee> employees);
    Task<TrendAnalysisResult> AnalyzeHiringTrendsAsync(List<Employee> employees, int years = 5);
    Task<DistributionResult> CalculateAgeDistributionAsync(List<Employee> employees);
    Task<DistributionResult> CalculateTenureDistributionAsync(List<Employee> employees);
    Task<TurnoverAnalysisResult> AnalyzeTurnoverAsync(List<Employee> employees);
    Task<ComparisonResult> ComparePeriodsAsync(DateRange period1, DateRange period2);
    Task<PredictionResult> PredictFutureTrendsAsync(List<Employee> employees, int monthsAhead = 12);
}
```

### 3. チャートライブラリ統合

#### Chart.js 統合
```razor
<!-- ChartComponent.razor -->
@using ChartJs.Blazor
@using ChartJs.Blazor.Common
@using ChartJs.Blazor.BarChart

<Chart Config="_chartConfig" @ref="_chartRef" />

@code {
    [Parameter] public ReportDataSet DataSet { get; set; }
    [Parameter] public ChartSettings Settings { get; set; }
    
    private Chart _chartRef;
    private ChartConfig _chartConfig;
    
    protected override void OnParametersSet()
    {
        _chartConfig = CreateChartConfig();
    }
    
    private ChartConfig CreateChartConfig()
    {
        return Settings.ChartType switch
        {
            ChartType.Bar => CreateBarChart(),
            ChartType.Line => CreateLineChart(),
            ChartType.Pie => CreatePieChart(),
            _ => CreateBarChart()
        };
    }
}
```

### 4. レポートテンプレート

#### レポートビルダー
```csharp
public class ReportBuilder
{
    private readonly IDataAnalysisService _analysisService;
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;

    public async Task<ReportData> BuildEmployeeStatisticsReport(ReportSettings settings)
    {
        var employees = await GetFilteredEmployees(settings);
        var statistics = await _analysisService.CalculateEmployeeStatisticsAsync(employees);
        
        return new ReportData
        {
            Title = "社員統計レポート",
            GeneratedAt = DateTime.Now,
            DataSets = new List<ReportDataSet>
            {
                CreateDepartmentDistributionDataSet(employees),
                CreateAgeDistributionDataSet(employees),
                CreateTenureDistributionDataSet(employees)
            },
            Summary = CreateSummary(statistics)
        };
    }
    
    private ReportDataSet CreateDepartmentDistributionDataSet(List<Employee> employees)
    {
        var departmentCounts = employees
            .GroupBy(e => e.DepartmentName)
            .Select(g => new ReportDataPoint
            {
                Label = g.Key,
                Value = g.Count(),
                Category = "部署"
            })
            .ToList();
            
        return new ReportDataSet
        {
            Name = "部署別社員数",
            DataPoints = departmentCounts
        };
    }
}
```

### 5. UI コンポーネント

#### レポートダッシュボード
```razor
<!-- ReportDashboard.razor -->
<MudContainer MaxWidth="MaxWidth.ExtraLarge">
    <MudGrid>
        <MudItem xs="12" md="3">
            <ReportSummaryCard Title="総社員数" Value="@_summary.TotalEmployees" Icon="@Icons.Material.Filled.People" />
        </MudItem>
        <MudItem xs="12" md="3">
            <ReportSummaryCard Title="部署数" Value="@_summary.TotalDepartments" Icon="@Icons.Material.Filled.Business" />
        </MudItem>
        <MudItem xs="12" md="3">
            <ReportSummaryCard Title="平均年齢" Value="@($"{_summary.AverageAge:F1}歳")" Icon="@Icons.Material.Filled.Cake" />
        </MudItem>
        <MudItem xs="12" md="3">
            <ReportSummaryCard Title="平均勤続" Value="@($"{_summary.AverageTenure:F1}年")" Icon="@Icons.Material.Filled.Timeline" />
        </MudItem>
    </MudGrid>
    
    <MudGrid Class="mt-4">
        <MudItem xs="12" md="6">
            <MudPaper Class="pa-4" Elevation="3">
                <MudText Typo="Typo.h6">部署別社員数</MudText>
                <ChartComponent DataSet="@_departmentChart" Settings="@_pieChartSettings" />
            </MudPaper>
        </MudItem>
        <MudItem xs="12" md="6">
            <MudPaper Class="pa-4" Elevation="3">
                <MudText Typo="Typo.h6">年別入社数推移</MudText>
                <ChartComponent DataSet="@_hiringTrendChart" Settings="@_lineChartSettings" />
            </MudPaper>
        </MudItem>
    </MudGrid>
</MudContainer>
```

#### カスタムレポートビルダー
```razor
<!-- CustomReportBuilder.razor -->
<MudStepper @ref="_stepper" ActiveStepChanged="OnStepChanged">
    <MudStep Title="レポート設定">
        <ReportSettingsStep @bind-Settings="_reportSettings" />
    </MudStep>
    <MudStep Title="データ選択">
        <DataSelectionStep @bind-SelectedFields="_selectedFields" />
    </MudStep>
    <MudStep Title="グラフ設定">
        <ChartSettingsStep @bind-ChartSettings="_chartSettings" />
    </MudStep>
    <MudStep Title="プレビュー">
        <ReportPreviewStep ReportData="@_previewData" />
    </MudStep>
</MudStepper>
```

## ファイル変更計画

### 新規作成ファイル

1. **ドメイン層**
   - `Domain/Models/Report.cs`
   - `Domain/Models/ReportData.cs`
   - `Domain/Models/ReportSettings.cs`
   - `Domain/Interfaces/IReportRepository.cs`

2. **アプリケーション層**
   - `Application/Interfaces/IReportService.cs`
   - `Application/Interfaces/IDataAnalysisService.cs`
   - `Application/Services/ReportService.cs`
   - `Application/Services/DataAnalysisService.cs`
   - `Application/Services/ReportBuilderService.cs`
   - `Application/Services/ReportExportService.cs`

3. **インフラストラクチャー層**
   - `Infrastructure/Repositories/InMemoryReportRepository.cs`

4. **UI コンポーネント**
   - `Components/Pages/Reports.razor`
   - `Components/Pages/ReportDashboard.razor`
   - `Components/Pages/CustomReportBuilder.razor`
   - `Components/Shared/ChartComponent.razor`
   - `Components/Shared/ReportSummaryCard.razor`
   - `Components/Dialogs/ReportSettingsDialog.razor`
   - `Components/Dialogs/ReportExportDialog.razor`

5. **ViewModels**
   - `ViewModels/ReportViewModel.cs`
   - `ViewModels/DashboardViewModel.cs`

### 既存ファイル変更

1. **データストア拡張**
   - `Infrastructure/DataStores/ConcurrentInMemoryDataStore.cs`

2. **依存性注入**
   - `Program.cs`

3. **ナビゲーション**
   - `Components/Layout/NavMenu.razor`

4. **設定ファイル**
   - `appsettings.json` (レポート設定追加)

## 実装手順

### Phase 1: 基盤実装（優先度：最高）
1. Report ドメインモデル作成
2. 基本的なレポートサービス
3. データ分析サービス基盤
4. InMemory リポジトリ実装

### Phase 2: 基本レポート（優先度：高）
1. 社員統計レポート
2. 部署分析レポート
3. 基本的なグラフ表示
4. レポート一覧画面

### Phase 3: 可視化・チャート（優先度：高）
1. Chart.js 統合
2. 複数種類のチャート実装
3. インタラクティブ機能
4. レスポンシブ対応

### Phase 4: 高度機能（優先度：中）
1. カスタムレポートビルダー
2. エクスポート機能拡張
3. 比較分析機能
4. フィルタリング強化

### Phase 5: ダッシュボード（優先度：中）
1. 統合ダッシュボード
2. リアルタイム更新
3. パーソナライズ機能
4. 定期レポート機能

## テスト項目

### 単体テスト
- データ分析ロジック
- レポート生成機能
- エクスポート機能
- 統計計算の精度

### 統合テスト
- レポート生成フロー
- チャート表示機能
- エクスポート処理
- ダッシュボード表示

### パフォーマンステスト
- 大量データでのレポート生成
- チャート描画速度
- エクスポート処理時間

## 必要なNuGetパッケージ
```xml
<!-- チャートライブラリ -->
<PackageReference Include="ChartJs.Blazor" Version="2.0.2" />
<!-- PDF生成 -->
<PackageReference Include="iText7" Version="8.0.2" />
<!-- Excel生成 -->
<PackageReference Include="EPPlus" Version="6.2.9" />
<!-- 画像処理 -->
<PackageReference Include="SkiaSharp" Version="2.88.0" />
```

## 技術的考慮事項

### パフォーマンス
- 大量データの効率的処理
- チャート描画の最適化
- キャッシュ戦略
- 非同期処理活用

### ユーザビリティ
- 直感的なレポートUI
- レスポンシブデザイン
- エクスポート進捗表示
- エラーハンドリング

### 拡張性
- プラグイン型レポート
- カスタムチャート追加
- 外部データソース統合
- API連携準備

## リスクと対策

### 技術的リスク
- **メモリ使用量**: 大量データ処理時
- **描画性能**: 複雑なチャート表示
- **データ精度**: 統計計算の正確性

### 対策
- ストリーミング処理
- 仮想化・ページネーション
- 統計アルゴリズムの検証
- パフォーマンス監視

## 見積もり
- **開発工数**: 6-7日
- **テスト工数**: 2-3日
- **総工数**: 8-10日

## 依存関係
- 詳細検索機能（レポート用データフィルタ）
- ファイルアップロード（レポート結果保存）
- 権限管理（レポートアクセス制御）
- 監査ログ（レポート生成履歴）

## 成功指標
- ✅ レポート生成時間 < 10秒
- ✅ チャート描画速度 < 3秒
- ✅ エクスポート成功率 > 99%
- ✅ ユーザビリティ評価（直感性）
- ✅ データ精度100%（統計計算）

## 将来拡張
- 機械学習予測機能
- リアルタイムストリーミング分析
- 外部BI ツール連携
- モバイルアプリ対応
- データウェアハウス統合
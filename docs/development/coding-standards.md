# コーディング標準・規約

このドキュメントは、社員情報管理モックアプリケーションにおけるコーディング標準とベストプラクティスを定義します。

## 🎯 **Core Coding Principles**

このプロジェクトでは、以下の3つの基本原則を厳守してください：

### 1. **シンプルで理解しやすいコード (Simple & Understandable Code)**

複雑な実装よりも、理解しやすく保守しやすいコードを優先します。

```csharp
// ✅ Good - シンプルで明確
public async Task<bool> IsEmployeeExistsAsync(string employeeNumber)
{
    if (string.IsNullOrWhiteSpace(employeeNumber))
    {
        return false;
    }
    
    var employee = await _repository.GetByEmployeeNumberAsync(employeeNumber);
    return employee != null;
}

// ❌ Bad - 複雑で理解困難
public async Task<bool> IsEmployeeExistsAsync(string employeeNumber) =>
    !string.IsNullOrWhiteSpace(employeeNumber) && 
    (await _repository.GetByEmployeeNumberAsync(employeeNumber)) != null;
```

**ガイドライン:**
- 1つのメソッドは1つの処理に専念
- ネストは3層以内に収める
- 条件分岐は明確に記述
- LINQ の複雑な連鎖は避け、分割して記述

### 2. **豊富なコメント (Extensive Comments)**

コードの意図、理由、制約を明確に記述します。

```csharp
/// <summary>
/// 社員の存在確認を行う
/// データベースから指定された社員番号の社員を検索し、存在するかどうかを判定する
/// </summary>
/// <param name="employeeNumber">検索対象の社員番号</param>
/// <returns>社員が存在する場合はtrue、存在しない場合はfalse</returns>
public async Task<bool> IsEmployeeExistsAsync(string employeeNumber)
{
    // 入力値の検証 - null や空文字の場合は存在しないと判定
    if (string.IsNullOrWhiteSpace(employeeNumber))
    {
        _logger.LogDebug("社員番号が空のため、存在しないと判定: {EmployeeNumber}", employeeNumber);
        return false;
    }
    
    // リポジトリから社員情報を取得
    var employee = await _repository.GetByEmployeeNumberAsync(employeeNumber);
    
    // 取得結果の判定
    var exists = employee != null;
    _logger.LogDebug("社員存在確認結果: {EmployeeNumber} -> {Exists}", employeeNumber, exists);
    
    return exists;
}
```

### 3. **役割毎の細かい分割と疎結合 (Fine-grained Separation & Loose Coupling)**

単一責任原則に従い、各クラス・メソッドは明確な責務を持ちます。

**サービス分離の例:**
```csharp
// ✅ Good - 責務が明確に分離されている
public interface IEmployeeValidationService
{
    Task<ValidationResult> ValidateEmployeeAsync(Employee employee);
}

public interface IEmployeeDataService  
{
    Task<Employee?> GetByIdAsync(string employeeNumber);
    Task<bool> SaveAsync(Employee employee);
}

public interface IEmployeeUIService
{
    void ShowSuccessMessage(string message);
    void ShowErrorMessage(string message);
}

// 各サービスが独立して動作し、依存性注入で疎結合を実現
public class EmployeeEditService
{
    private readonly IEmployeeValidationService _validationService;
    private readonly IEmployeeDataService _dataService;
    private readonly IEmployeeUIService _uiService;
    
    // 各責務を担当するサービスに処理を委譲
    public async Task<bool> SaveEmployeeAsync(Employee employee)
    {
        // バリデーション担当サービスに委譲
        var validationResult = await _validationService.ValidateEmployeeAsync(employee);
        if (!validationResult.IsValid)
        {
            _uiService.ShowErrorMessage(validationResult.ErrorMessage);
            return false;
        }
        
        // データ保存担当サービスに委譲
        var success = await _dataService.SaveAsync(employee);
        if (success)
        {
            _uiService.ShowSuccessMessage("社員情報を保存しました");
        }
        
        return success;
    }
}
```

## 📝 **コメント記述ガイドライン**

### XML Documentation Comments

すべてのpublic/protectedメソッド、プロパティにXMLコメントを記述します。

```csharp
/// <summary>
/// [メソッドの目的を1-2行で明確に説明]
/// </summary>
/// <param name="paramName">[パラメータの説明]</param>
/// <returns>[戻り値の説明]</returns>
/// <exception cref="ExceptionType">[例外が発生する条件]</exception>
```

**記述例:**
```csharp
/// <summary>
/// 部門情報を更新する
/// 部門コードの重複チェックと責任者の有効性を検証後、データベースに保存
/// </summary>
/// <param name="department">更新対象の部門情報</param>
/// <param name="cancellationToken">キャンセレーショントークン</param>
/// <returns>更新に成功した場合はtrue、失敗した場合はfalse</returns>
/// <exception cref="ArgumentNullException">departmentがnullの場合</exception>
/// <exception cref="DuplicateDepartmentException">部門コードが重複している場合</exception>
```

### インラインコメント

複雑なロジックには段落ごとにコメントを追加します。

```csharp
public async Task ProcessAsync()
{
    // 1. データ準備フェーズ - 処理に必要な情報を収集
    var data = await PrepareDataAsync();
    
    // 2. バリデーションフェーズ - データの整合性を確認
    if (!ValidateData(data))
    {
        // バリデーション失敗時は早期リターン
        return;
    }
    
    // 3. メイン処理フェーズ - 実際のビジネスロジックを実行
    await ExecuteMainLogicAsync(data);
}
```

### コメント記述のベストプラクティス

- **What（何を）**よりも**Why（なぜ）**を説明
- 業務的な制約や仕様の背景を記録
- 将来の拡張ポイントをTODOコメントで明記
- 一時的な実装にはHACKコメントを付与

```csharp
// 業務仕様: 責任者は他部門との兼任が可能だが、警告表示が必要
var isDuplicateManager = await IsManagerInOtherDepartmentAsync(employeeNumber);
if (isDuplicateManager)
{
    // 警告メッセージを表示するが処理は継続
    ShowWarningMessage("この社員は他の部門でも責任者として設定されています");
}

// TODO: 将来的に部門ごとの責任者制限設定機能を追加予定
// HACK: 現在は暫定的にハードコードしているが、設定ファイル化が必要
```

## 🏗️ **アーキテクチャパターン**

### Service-Oriented Architecture

UIコンポーネントからビジネスロジックを分離し、各サービスが明確な責務を持つ構造にします。

```
Components/Pages/
├── EmployeeEdit.razor           # UI表示のみ担当
└── DepartmentEdit.razor         # UI表示のみ担当

Application/Services/
├── EmployeeValidationService    # バリデーション専門
├── EmployeeDataService         # データアクセス専門  
├── EmployeeUIService           # UI操作専門
└── EmployeeSearchService       # 検索機能専門

ViewModels/
├── EmployeeEditViewModel       # 状態管理専門
└── DepartmentEditViewModel     # 状態管理専門

Constants/
├── ValidationConstants.cs      # バリデーション定数
└── DepartmentEditConstants.cs  # UI表示定数
```

### 依存性注入パターン

```csharp
// Program.cs - サービス登録
builder.Services.AddScoped<IEmployeeValidationService, EmployeeValidationService>();
builder.Services.AddScoped<IEmployeeDataService, EmployeeDataService>();
builder.Services.AddScoped<IEmployeeUIService, EmployeeUIService>();
builder.Services.AddMemoryCache(); // キャッシュサービス

// Component - サービス注入
@inject IEmployeeValidationService ValidationService
@inject IEmployeeDataService DataService  
@inject IEmployeeUIService UIService
```

### インターフェース分離原則

各サービスは明確なインターフェースを実装し、具象に依存しない設計とします。

```csharp
// ✅ Good - 責務が明確に分かれている
public interface IEmployeeValidationService
{
    Task<ValidationResult> ValidateAsync(Employee employee);
    Task<bool> IsEmployeeNumberDuplicateAsync(string employeeNumber);
}

public interface IEmployeeSearchService  
{
    Task<IEnumerable<Employee>> SearchAsync(string keyword);
    Task<Employee?> GetByEmployeeNumberAsync(string employeeNumber);
}

// ❌ Bad - 責務が曖昧で肥大化
public interface IEmployeeService
{
    Task<ValidationResult> ValidateAsync(Employee employee);
    Task<bool> SaveAsync(Employee employee);
    Task<IEnumerable<Employee>> SearchAsync(string keyword);
    void ShowMessage(string message);
    // ... 責務が混在
}
```

## ⚡ **パフォーマンス考慮事項**

### 非同期処理の適切な実装

```csharp
// ✅ Good - 適切な非同期実装
public async Task<IEnumerable<Employee>> SearchEmployeesAsync(string keyword)
{
    // 短いキーワードは早期リターン
    if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
    {
        return Enumerable.Empty<Employee>();
    }
    
    // 非同期でデータ取得
    var employees = await _repository.SearchAsync(keyword);
    
    // 結果をフィルタリング（メモリ上で高速処理）
    return employees
        .Where(emp => emp.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        .OrderBy(emp => emp.EmployeeNumber)
        .Take(10);
}
```

### キャッシュ戦略

頻繁にアクセスされるデータはメモリキャッシュを活用します。

```csharp
public class EmployeeSearchService : IEmployeeSearchService
{
    private readonly IMemoryCache _memoryCache;
    private const string ALL_EMPLOYEES_CACHE_KEY = "EmployeeSearch_AllEmployees";
    private const int CACHE_DURATION_MINUTES = 10;
    
    public async Task<IEnumerable<Employee>> GetCachedAllEmployeesAsync()
    {
        // キャッシュから取得を試行
        if (_memoryCache.TryGetValue(ALL_EMPLOYEES_CACHE_KEY, out IEnumerable<Employee>? cached))
        {
            return cached!;
        }
        
        // キャッシュミスの場合はデータベースから取得
        var employees = await _repository.GetAllAsync();
        var employeeList = employees.ToList();
        
        // キャッシュに保存
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
            Priority = CacheItemPriority.Normal
        };
        
        _memoryCache.Set(ALL_EMPLOYEES_CACHE_KEY, employeeList, cacheOptions);
        return employeeList;
    }
}
```

## 🔒 **エラーハンドリング標準**

### 統一されたエラー処理パターン

Result パターンを使用して、例外に頼らない安全なエラー処理を実装します。

```csharp
public async Task<Result<Employee>> GetEmployeeAsync(string employeeNumber)
{
    try
    {
        // 入力値検証
        if (string.IsNullOrWhiteSpace(employeeNumber))
        {
            return Result<Employee>.Failure("社員番号が指定されていません");
        }
        
        // データ取得処理
        var employee = await _repository.GetByEmployeeNumberAsync(employeeNumber);
        
        // 結果判定
        return employee != null 
            ? Result<Employee>.Success(employee)
            : Result<Employee>.Failure("指定された社員が見つかりません");
    }
    catch (Exception ex)
    {
        // 例外ログ出力
        _logger.LogError(ex, "社員取得中にエラーが発生: {EmployeeNumber}", employeeNumber);
        
        // 安全なエラーメッセージを返却
        return Result<Employee>.Failure("社員情報の取得中にエラーが発生しました");
    }
}
```

### ログ出力標準

構造化ログを使用して、運用監視に適したログ出力を行います。

```csharp
public class DepartmentValidationService
{
    private readonly ILogger<DepartmentValidationService> _logger;
    
    public async Task<ValidationResult> ValidateDepartmentAsync(DepartmentMaster department)
    {
        _logger.LogDebug("部門バリデーション開始: {DepartmentCode}", department?.DepartmentCode);
        
        // ... バリデーション処理 ...
        
        if (validationResult.IsValid)
        {
            _logger.LogInformation("部門バリデーション成功: {DepartmentCode} - {DepartmentName}", 
                department.DepartmentCode, department.DepartmentName);
        }
        else
        {
            _logger.LogWarning("部門バリデーション失敗: {DepartmentCode}, エラー数: {ErrorCount}", 
                department.DepartmentCode, validationResult.ErrorMessages.Count);
        }
        
        return validationResult;
    }
}
```

## 📊 **品質保証**

### 単体テストの指針

各サービスクラスには対応する単体テストを実装します。

```csharp
[TestClass]
public class EmployeeValidationServiceTests
{
    private Mock<IEmployeeRepository> _mockRepository;
    private Mock<ILogger<EmployeeValidationService>> _mockLogger;
    private EmployeeValidationService _service;
    
    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IEmployeeRepository>();
        _mockLogger = new Mock<ILogger<EmployeeValidationService>>();
        _service = new EmployeeValidationService(_mockRepository.Object, _mockLogger.Object);
    }
    
    [TestMethod]
    public async Task ValidateEmployeeAsync_ValidEmployee_ReturnsSuccess()
    {
        // Arrange
        var employee = new Employee 
        { 
            EmployeeNumber = "EMP0000001", 
            Name = "田中 太郎" 
        };
        
        // Act
        var result = await _service.ValidateEmployeeAsync(employee);
        
        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual("社員情報は有効です", result.Message);
    }
}
```

### コードレビューチェックリスト

- [ ] XMLコメントがすべてのpublicメンバーに記述されている
- [ ] インラインコメントが複雑な処理に追加されている
- [ ] 単一責任原則が守られている
- [ ] インターフェースによる抽象化が適切に行われている
- [ ] 例外処理とログ出力が適切に実装されている
- [ ] マジックナンバーが定数として定義されている
- [ ] null安全性が考慮されている
- [ ] 非同期処理が適切に実装されている

## 📋 **必須実装項目**

### 1. コメント記述
- **XMLコメント**: すべてのpublic/protectedメンバー
- **インラインコメント**: 複雑なロジック、業務仕様、将来の拡張ポイント
- **TODOコメント**: 将来の改善点や課題

### 2. サービス分離
- **単一責任原則**: 1クラス1責務
- **インターフェース実装**: テスタビリティの確保
- **依存性注入**: 疎結合の実現

### 3. エラーハンドリング
- **適切な例外処理**: try-catch-finallyの適切な使用
- **構造化ログ出力**: 運用監視に適したログ
- **Result パターン**: 安全なエラー伝播

### 4. コード品質
- **マジックナンバー撲滅**: 定数クラスの活用
- **null安全性**: nullチェックとガードクローズ
- **パフォーマンス考慮**: 非同期処理とキャッシュ

### 5. テスタビリティ
- **モック可能な設計**: インターフェース依存
- **単体テスト実装**: 各サービスクラスに対応
- **統合テスト準備**: エンドツーエンドテストの基盤

## 🚀 **実装順序**

### フェーズ1: 設計
1. 責務の明確化とサービス分割の設計
2. インターフェース定義
3. エラー処理戦略の決定

### フェーズ2: 実装
1. 定数クラスとモデルクラスの実装
2. サービスクラスの実装（インターフェース → 実装の順）
3. 依存性注入の設定

### フェーズ3: 統合
1. UIコンポーネントのリファクタリング
2. エラーハンドリングの統合
3. ログ出力の統合

### フェーズ4: 検証
1. 単体テストの実装と実行
2. 統合テストの実行
3. コードレビューとリファクタリング

このコーディング標準に従うことで、保守性・可読性・テスタビリティの高いコードベースを維持し、チーム開発における品質と効率性を向上させることができます。
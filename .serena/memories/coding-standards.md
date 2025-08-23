# Coding Standards & Conventions

## Core Principles (必須遵守)

### 1. シンプルで理解しやすいコード (Simple & Understandable Code)
- 複雑な実装より保守しやすいコードを優先
- 1つのメソッドは1つの処理に専念
- ネストは3層以内に収める  
- LINQ の複雑な連鎖は避け、分割して記述

### 2. 豊富なコメント (Extensive Comments)
- **XMLコメント**: 全public/protectedメンバーに必須
- **インラインコメント**: 複雑な処理、業務仕様、制約を記述
- **TODOコメント**: 将来の改善点や課題を明記
- **WHY重視**: 何をするかではなく、なぜするかを説明

### 3. 役割毎の細かい分割と疎結合 (Fine-grained Separation & Loose Coupling)
- **単一責任原則**: 1クラス1責務を厳守
- **インターフェース実装**: テスタビリティと疎結合を確保
- **サービス分離**: バリデーション、データアクセス、UI操作を分離
- **依存性注入**: 依存関係は全てDIコンテナで管理

## 必須実装項目

### コメント記述
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

### サービス分離パターン
```csharp
// インターフェース分離 - 各責務に特化
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
```

### エラーハンドリング標準
- **Result パターン**: 例外に頼らない安全なエラー処理
- **構造化ログ**: 運用監視に適したログ出力
- **適切な例外処理**: try-catch-finallyの適切な使用

### 品質保証項目
- **マジックナンバー撲滅**: 定数クラスの活用
- **null安全性**: nullチェックとガードクローズ  
- **非同期処理**: async/awaitの適切な実装
- **パフォーマンス考慮**: メモリキャッシュの活用

## アーキテクチャパターン

### ディレクトリ構造
```
Components/Pages/          # UI表示のみ担当
Application/Services/      # ビジネスロジック分離
├── ValidationService     # バリデーション専門
├── DataService          # データアクセス専門  
├── UIService            # UI操作専門
└── SearchService        # 検索機能専門
ViewModels/               # 状態管理専門
Constants/                # 定数定義
```

### 依存性注入パターン
```csharp
// Program.cs - サービス登録
builder.Services.AddScoped<IEmployeeValidationService, EmployeeValidationService>();
builder.Services.AddScoped<IEmployeeDataService, EmployeeDataService>();

// Component - サービス注入  
@inject IEmployeeValidationService ValidationService
@inject IEmployeeDataService DataService
```
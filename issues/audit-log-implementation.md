# 監査ログ機能実装計画

## 概要
全ての操作履歴を記録し、データ変更の追跡可能性を提供する監査ログ機能を実装します。

## 機能要件

### 基本機能
- ✅ 全CRUD操作の記録（社員・部署管理）
- ✅ ログイン/ログアウト履歴
- ✅ データ変更前後の差分表示
- ✅ 操作者、操作日時、操作内容の詳細記録
- ✅ 監査ログ一覧表示画面
- ✅ 検索・フィルタリング機能

### 拡張機能
- ✅ ログレベル設定（Info, Warning, Error）
- ✅ 自動削除機能（保存期間設定）
- ✅ CSV エクスポート機能
- ✅ リアルタイム監査ログ表示

## 技術実装詳細

### 1. ドメインモデル追加

#### AuditLog モデル
```csharp
public class AuditLog
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Action { get; set; }        // Create, Update, Delete, Login, Logout
    public string EntityType { get; set; }    // Employee, Department, User
    public string EntityId { get; set; }
    public string OldValues { get; set; }     // JSON形式
    public string NewValues { get; set; }     // JSON形式
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; }
    public AuditLogLevel Level { get; set; }
}

public enum AuditLogLevel
{
    Info = 1,
    Warning = 2,
    Error = 3
}
```

### 2. インフラストラクチャー層

#### リポジトリ実装
- `IAuditLogRepository` インターフェース
- `InMemoryAuditLogRepository` 実装クラス
- データストア統合（`ConcurrentInMemoryDataStore`）

#### サービス層
- `IAuditLogService` インターフェース
- `AuditLogService` 実装クラス
- 自動ログ記録機能
- 差分計算機能

### 3. UI コンポーネント

#### 監査ログ一覧ページ
- `/audit-logs` ルート
- MudBlazor データグリッド使用
- 検索・フィルタリング機能
- ページネーション対応

#### 詳細表示ダイアログ
- 変更差分表示
- JSON データの整形表示
- 関連データの表示

### 4. 横断的関心事

#### 自動ログ記録
- AOP（Aspect-Oriented Programming）パターン使用
- サービス層でのインターセプト実装
- 属性ベースログ記録（`[AuditLog]`）

## ファイル変更計画

### 新規作成ファイル

1. **ドメイン層**
   - `Domain/Models/AuditLog.cs`
   - `Domain/Interfaces/IAuditLogRepository.cs`

2. **アプリケーション層**
   - `Application/Interfaces/IAuditLogService.cs`
   - `Application/Services/AuditLogService.cs`

3. **インフラストラクチャー層**
   - `Infrastructure/Repositories/InMemoryAuditLogRepository.cs`

4. **UI コンポーネント**
   - `Components/Pages/AuditLogs.razor`
   - `Components/Pages/AuditLogs.razor.cs`
   - `Components/Dialogs/AuditLogDetailDialog.razor`
   - `ViewModels/AuditLogViewModel.cs`

### 既存ファイル変更

1. **データストア統合**
   - `Infrastructure/DataStores/ConcurrentInMemoryDataStore.cs`

2. **サービス拡張**
   - `Application/Services/EmployeeService.cs`
   - `Application/Services/DepartmentService.cs`
   - `Application/Services/AuthenticationService.cs`

3. **依存性注入**
   - `Program.cs`

4. **ナビゲーション追加**
   - `Components/Layout/NavMenu.razor`
   - `Components/Routes.razor`

## 実装手順

### Phase 1: 基盤実装（優先度：最高）
1. AuditLog ドメインモデル作成
2. リポジトリ・サービス インターフェース定義
3. InMemory データストア拡張
4. 基本的なログ記録機能実装

### Phase 2: サービス統合（優先度：高）
1. 既存サービスへのログ記録統合
2. ログイン/ログアウト履歴記録
3. データ変更差分計算機能
4. 自動ログ記録システム

### Phase 3: UI実装（優先度：中）
1. 監査ログ一覧ページ作成
2. 検索・フィルタリング機能
3. 詳細表示ダイアログ
4. エクスポート機能

### Phase 4: 最適化（優先度：低）
1. パフォーマンス最適化
2. 自動削除機能
3. リアルタイム更新
4. 高度な検索機能

## テスト項目

### 単体テスト
- AuditLogService メソッドテスト
- データ変更差分計算テスト
- リポジトリ CRUD操作テスト

### 統合テスト
- 自動ログ記録テスト
- UI コンポーネント表示テスト
- 検索・フィルタリング動作テスト

### シナリオテスト
- 社員データ変更時のログ記録
- ログイン/ログアウト履歴記録
- 大量データでのパフォーマンス

## リスクと考慮事項

### 技術的リスク
- **パフォーマンス影響**: 全操作でのログ記録によるパフォーマンス低下
- **メモリ使用量**: InMemory データストアでの大量ログデータ蓄積
- **データ整合性**: 並行処理時のログ記録整合性

### 対策
- 非同期ログ記録実装
- ログレベル による記録制御
- バックグラウンド処理での自動削除
- スレッドセーフなデータストア活用

### データベース移行準備
- Entity Framework Core 対応設計
- インデックス設計考慮
- パーティション戦略検討

## 見積もり
- **開発工数**: 3-4日
- **テスト工数**: 1-2日
- **総工数**: 4-6日

## 依存関係
- 通知システム（監査ログアラート）
- レポート機能（監査レポート）
- 権限管理（監査ログアクセス制御）

## 成功指標
- ✅ 全CRUD操作のログ記録100%実現
- ✅ ログ検索レスポンス時間 < 1秒
- ✅ UI操作の直感性（ユーザビリティテスト）
- ✅ メモリ使用量の許容範囲内維持
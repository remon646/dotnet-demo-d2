# 社員検索Autocompleteに退職者切り替え機能実装

## 📋 Issue概要

**タイトル**: 社員検索Autocompleteで退職者を含める/除外する切り替え機能の実装  
**優先度**: Medium  
**カテゴリ**: Feature Enhancement  
**作成日**: 2025-08-15  

## 🎯 目的

社員検索のオートコンプリート機能において、退職者を検索結果に含めるか除外するかをUIで切り替え可能にし、より柔軟な社員検索機能を提供する。

## 📋 要件定義

### 機能要件

1. **退職日管理**
   - Employeeモデルに退職日フィールドを追加
   - 退職状態の判定ロジック実装

2. **検索フィルタリング**
   - 退職者を含む/除外する検索オプション
   - デフォルトは退職者除外

3. **UI切り替え機能**
   - EmployeeSelectorComponentに切り替えスイッチ追加
   - EmployeeSearchDialogに退職者フィルタオプション追加

4. **視覚的区別**
   - 退職者は検索結果で視覚的に区別表示

### 非機能要件

1. **パフォーマンス**
   - 既存の検索パフォーマンスを維持
   - キャッシュ戦略の最適化

2. **下位互換性**
   - 既存コンポーネントの動作に影響なし
   - 新機能はオプション実装

## 🏗️ 技術仕様

### 1. データモデル拡張

```csharp
// Employee.cs
public DateTime? RetirementDate { get; set; }
public bool IsRetired => RetirementDate.HasValue && RetirementDate.Value <= DateTime.Today;
```

### 2. 検索サービス拡張

```csharp
// IEmployeeSearchService.cs
Task<IEnumerable<Employee>> SearchForAutocompleteAsync(
    string keyword, 
    int maxResults = 10,
    bool includeRetired = false,
    CancellationToken cancellationToken = default);

// EmployeeSearchCriteria.cs
public bool IncludeRetired { get; set; } = false;
```

### 3. UI コンポーネント拡張

```razor
<!-- EmployeeSelectorComponent.razor -->
<MudSwitch @bind-Checked="IncludeRetired" 
           Label="退職者を含む" 
           Color="Color.Primary" />
```

## 🔄 実装計画

### Phase 1: モデル・サービス拡張
1. Employeeモデルに退職日フィールド追加
2. EmployeeSearchCriteriaに退職者フラグ追加
3. IEmployeeSearchServiceインターフェース拡張
4. EmployeeSearchServiceに退職者フィルタリング実装

### Phase 2: UI実装
1. EmployeeSelectorComponentに切り替えUI追加
2. EmployeeSearchDialogに退職者フィルタ追加
3. 退職者の視覚的区別実装

### Phase 3: データ・テスト
1. サンプルデータに退職者追加
2. 機能テスト実施
3. パフォーマンステスト

## 📊 影響範囲

### 変更ファイル
- `Domain/Models/Employee.cs`
- `Application/Interfaces/IEmployeeSearchService.cs`
- `Application/Services/EmployeeSearchService.cs`
- `Components/Shared/EmployeeSelectorComponent.razor`
- `Components/Dialogs/EmployeeSearchDialog.razor`
- `Infrastructure/DataStores/ConcurrentInMemoryDataStore.cs`

### 影響するコンポーネント
- 社員選択コンポーネント（全箇所）
- 社員検索ダイアログ
- 部門責任者選択

## ✅ 検証項目

### 機能テスト
- [ ] 退職者除外検索の動作確認
- [ ] 退職者含む検索の動作確認
- [ ] UI切り替えの動作確認
- [ ] 検索結果の視覚的区別確認

### 非機能テスト
- [ ] 検索パフォーマンスの確認
- [ ] 既存機能への影響なし確認
- [ ] メモリ使用量の確認

### 互換性テスト
- [ ] 既存の社員選択画面で正常動作
- [ ] 部門責任者選択で正常動作
- [ ] オートコンプリートのレスポンス性能

## 🎉 完了基準

1. 退職者の切り替え機能が正常に動作する
2. 既存機能に影響がない
3. パフォーマンスが劣化しない
4. UIが直感的で使いやすい
5. コードレビューをパスする

## 📝 実装ログ

### 2025-08-15
- Issue作成
- 技術仕様策定
- 実装プラン確定

---

**作成者**: Claude Code  
**レビュー者**: TBD  
**完了予定**: 2025-08-15
# 部門管理画面の責任者選択機能強化

## Issue概要
部門管理画面において、責任者の選択機能を改善し、社員マスタとの連携による使いやすい責任者選択を実現する。

## 要求内容
1. **部門の新規追加・変更機能の強化** - 現在既に実装済みですが、責任者選択機能を改善
2. **責任者選択機能の改善** - 社員マスタから社員コードで責任者を指定できるように改善

## 実装内容

### 1. ドキュメント更新
- **ファイル**: `docs/specifications/feature-specifications.md`
  - 部門管理機能の仕様を詳細化
  - 責任者選択機能の新仕様を追加
  - UI/UX改善内容を文書化

- **ファイル**: `docs/specifications/社員情報管理モックアプリ仕様書.md`
  - 部門マスタ管理機能の詳細仕様を追加
  - 責任者選択機能の技術的詳細を記載

### 2. 部門編集ダイアログの機能強化
- **ファイル**: `EmployeeManagement/Components/Dialogs/DepartmentEditDialog.razor`

#### 主要変更点:
1. **依存性注入の追加**
   ```csharp
   @inject IEmployeeRepository EmployeeRepository
   ```

2. **UI改善**
   - 責任者選択を`MudTextField`から`MudAutocomplete`に変更
   - 社員番号フィールドを読み取り専用に変更
   - リッチなアイテム表示テンプレートを実装

3. **検索機能の実装**
   ```csharp
   private async Task<IEnumerable<Employee>> SearchEmployees(string searchText, CancellationToken cancellationToken)
   ```
   - 社員コード・名前による検索
   - 部分一致検索対応
   - 最大10件表示

4. **自動反映機能**
   ```csharp
   private void OnManagerSelected(Employee? manager)
   ```
   - 選択した社員の名前・番号を自動設定
   - クリア機能対応

5. **バリデーション強化**
   - 責任者の存在確認
   - 不正なデータのクリア機能

## 技術的詳細

### 新機能の特徴
- **オートコンプリート検索**: 社員コードまたは名前で検索可能
- **リッチな表示**: 社員名、社員番号、所属部門を表示
- **自動反映**: 選択した社員の情報が自動的にフィールドに設定
- **バリデーション**: 存在しない社員の場合はデータクリア
- **UX向上**: クリア可能、プログレスインジケーター、最大表示件数制限

### UIコンポーネント詳細
```razor
<MudAutocomplete T="Employee" Value="selectedManager"
               Label="責任者"
               SearchFunc="SearchEmployees"
               ToStringFunc="@(e => e == null ? string.Empty : $"{e.EmployeeNumber} - {e.Name}")"
               Variant="Variant.Outlined"
               MaxItems="10"
               ShowProgressIndicator="true"
               Class="mb-3"
               Clearable="true"
               ValueChanged="OnManagerSelected">
    <ItemTemplate Context="e">
        <div style="display: flex; flex-direction: column;">
            <MudText Typo="Typo.subtitle2">@e.Name</MudText>
            <MudText Typo="Typo.caption" Style="color: gray;">@e.EmployeeNumber (@e.CurrentDepartmentDisplayName)</MudText>
        </div>
    </ItemTemplate>
</MudAutocomplete>
```

## テスト結果
- ✅ ビルド成功 - コンパイルエラーなし
- ✅ アプリケーション起動確認
- ✅ 警告は既存のもののみ（新規警告なし）

## 改善効果
1. **ユーザビリティ向上**: 手動入力からオートコンプリート選択へ
2. **データ整合性**: 社員マスタとの連携による正確な責任者情報
3. **操作効率**: 検索機能による迅速な責任者選択
4. **エラー防止**: バリデーションによる不正データの防止

## 実装日時
- **開始**: 2025-08-03
- **完了**: 2025-08-03
- **ステータス**: ✅ 完了

## 関連ファイル
- `docs/specifications/feature-specifications.md`
- `docs/specifications/社員情報管理モックアプリ仕様書.md`
- `EmployeeManagement/Components/Dialogs/DepartmentEditDialog.razor`

## 今後の拡張可能性
- 部門履歴との連携強化
- 責任者変更履歴の記録
- 複数責任者対応
- 責任者権限の詳細管理
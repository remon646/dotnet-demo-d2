# EmployeeSelectorComponent 使用ガイド

## 概要

`EmployeeSelectorComponent` は、社員選択機能を提供する再利用可能なBlazorコンポーネントです。オートコンプリート機能と詳細検索ダイアログの両方を統合し、様々な用途に対応できる柔軟な設計になっています。

## 機能特徴

### 🎯 主な機能
- **オートコンプリート検索**: リアルタイムでの社員検索
- **詳細検索ダイアログ**: 複数条件による詳細検索
- **3つの選択モード**: 用途に応じた検索対象の制限
- **柔軟なカスタマイズ**: パラメータによる外観・動作の調整

### 🔍 検索モード

| モード | 説明 | 用途 |
|--------|------|------|
| `Standard` | 全社員検索可能 | 一般的な社員選択 |
| `ManagerOnly` | 責任者候補のみ検索 | 部門責任者選択 |
| `AutocompleteOnly` | 軽量オートコンプリートのみ | 簡易入力フォーム |

## 基本的な使用方法

### 最小限の実装

```razor
<EmployeeSelectorComponent @bind-SelectedEmployee="selectedEmployee" />
```

### 責任者選択（推奨設定）

```razor
<EmployeeSelectorComponent Mode="EmployeeSelectorMode.ManagerOnly"
                         @bind-SelectedEmployee="selectedManager"
                         Label="責任者選択"
                         DialogTitle="責任者を選択してください"
                         ShowSelectedEmployeeInfo="true"
                         Class="mb-3" />
```

### イベントハンドラー付き

```razor
<EmployeeSelectorComponent SelectedEmployee="selectedEmployee"
                         SelectedEmployeeChanged="OnEmployeeSelected"
                         Label="担当者選択"
                         Required="true" />

@code {
    private Employee? selectedEmployee;
    
    private async Task OnEmployeeSelected(Employee? employee)
    {
        selectedEmployee = employee;
        if (employee != null)
        {
            // 選択時の処理
            await ProcessEmployeeSelection(employee);
        }
    }
}
```

## パラメータ一覧

### 必須パラメータ

| パラメータ | 型 | 説明 |
|------------|-----|------|
| `SelectedEmployee` | `Employee?` | 選択された社員オブジェクト |
| `SelectedEmployeeChanged` | `EventCallback<Employee?>` | 選択変更時のコールバック |

### オプションパラメータ

#### 表示・スタイル設定

| パラメータ | 型 | デフォルト値 | 説明 |
|------------|-----|-------------|------|
| `Label` | `string` | "社員選択" | コンポーネントのラベル |
| `Variant` | `Variant` | `Variant.Outlined` | MudBlazorのバリアント |
| `Class` | `string` | `""` | 追加CSSクラス |
| `HelperText` | `string` | "社員番号を入力する..." | ヘルプテキスト |

#### 動作設定

| パラメータ | 型 | デフォルト値 | 説明 |
|------------|-----|-------------|------|
| `Mode` | `EmployeeSelectorMode` | `Standard` | 検索モード |
| `Required` | `bool` | `false` | 必須入力フラグ |
| `Disabled` | `bool` | `false` | 無効化フラグ |
| `AllowClear` | `bool` | `true` | クリアボタン表示 |
| `ShowSearchButton` | `bool` | `true` | 検索ボタン表示 |
| `ShowSelectedEmployeeInfo` | `bool` | `false` | 選択情報表示 |

#### ダイアログ設定

| パラメータ | 型 | デフォルト値 | 説明 |
|------------|-----|-------------|------|
| `DialogTitle` | `string` | "社員選択" | ダイアログタイトル |
| `MaxAutocompleteResults` | `int` | `10` | オートコンプリート最大表示件数 |

## 実装パターン

### 1. 部門責任者選択

```razor
@* 部門編集画面での責任者選択例 *@
<EmployeeSelectorComponent Mode="EmployeeSelectorMode.ManagerOnly"
                         SelectedEmployee="department.Manager"
                         SelectedEmployeeChanged="OnManagerChanged"
                         Label="責任者社員番号（任意）"
                         Variant="Variant.Outlined"
                         ShowSelectedEmployeeInfo="false"
                         AllowClear="true"
                         DialogTitle="責任者選択"
                         Class="mb-3"
                         HelperText="責任者候補から選択してください" />

@code {
    private async Task OnManagerChanged(Employee? manager)
    {
        // 責任者バリデーション実行
        if (manager != null)
        {
            var validationResult = await ManagerValidationService.ValidateManagerAsync(manager.EmployeeNumber);
            if (validationResult.IsValid)
            {
                department.Manager = manager;
                await UpdateDepartment();
            }
            else
            {
                // バリデーションエラー処理
                ShowError(validationResult.ErrorMessage);
            }
        }
    }
}
```

### 2. 軽量検索フィールド

```razor
@* 簡単な社員入力フィールドとして使用 *@
<EmployeeSelectorComponent Mode="EmployeeSelectorMode.AutocompleteOnly"
                         @bind-SelectedEmployee="assignee"
                         Label="担当者"
                         ShowSearchButton="false"
                         ShowSelectedEmployeeInfo="false"
                         Class="compact-selector" />
```

### 3. 必須入力フォーム

```razor
@* 必須入力として使用 *@
<EditForm Model="@model" OnValidSubmit="@HandleSubmit">
    <EmployeeSelectorComponent @bind-SelectedEmployee="model.ResponsiblePerson"
                             Label="責任者 *"
                             Required="true"
                             ShowSelectedEmployeeInfo="true"
                             HelperText="プロジェクトの責任者を選択してください" />
    
    <ValidationMessage For="@(() => model.ResponsiblePerson)" />
    
    <MudButton ButtonType="ButtonType.Submit" 
               Variant="Variant.Filled" 
               Color="Color.Primary">
        保存
    </MudButton>
</EditForm>
```

## スタイリング

### CSS クラス

コンポーネントは以下のCSSクラスを持ちます：

```css
.employee-selector-component {
    /* メインコンテナ */
}

.employee-selector-component .mud-autocomplete {
    /* オートコンプリートフィールド */
}

.employee-selector-component .mud-card {
    /* 選択情報表示カード */
}
```

### カスタムスタイル例

```css
.compact-selector .mud-input-control {
    margin-bottom: 0 !important;
}

.manager-selector .mud-input-adornment {
    color: var(--mud-palette-primary);
}
```

## 利用可能なサービス

コンポーネントは以下のサービスを利用します：

- **IEmployeeSearchService**: 社員検索機能
- **IDialogService**: 詳細検索ダイアログ表示

これらのサービスはDIコンテナに登録されている必要があります。

## エラーハンドリング

コンポーネント内でのエラーは静かに処理され、ユーザーに空の結果が表示されます。詳細なエラー情報が必要な場合は、`SelectedEmployeeChanged` イベントで独自のエラーハンドリングを実装してください。

## パフォーマンス考慮事項

### キャッシュ機能
- 検索結果は `IEmployeeSearchService` によってキャッシュされます
- オートコンプリート検索は高速レスポンスのため最適化されています

### 推奨事項
- 大量データでは `MaxAutocompleteResults` を調整してください
- 頻繁に使用される画面では `Mode` を適切に設定してください

## トラブルシューティング

### よくある問題

1. **コンポーネントが表示されない**
   - `_Imports.razor` に `@using EmployeeManagement.Components.Shared` が追加されているか確認

2. **検索結果が表示されない**
   - `IEmployeeSearchService` がDIコンテナに登録されているか確認
   - データベース接続やリポジトリの設定を確認

3. **ダイアログが開かない**
   - `IDialogService` の登録を確認
   - `EmployeeSearchDialog` コンポーネントが存在するか確認

## 更新履歴

| バージョン | 日付 | 変更内容 |
|------------|------|----------|
| 1.0 | 2025-08-10 | 初回リリース |

## 関連コンポーネント

- **EmployeeSearchDialog**: 詳細検索ダイアログ
- **MudAutocomplete**: ベースとなるMudBlazorコンポーネント

---

このガイドに関する質問やフィードバックは、開発チームまでお問い合わせください。
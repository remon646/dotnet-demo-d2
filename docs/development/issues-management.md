# Issues Management

## 📁 issues/ ディレクトリの使用方法

**目的**: 開発タスクの計画、実行、完了管理

### 開発開始時
- `issues/` ディレクトリに新しい課題・機能のMarkdownファイルを作成
- ファイル名例: `feature-name-implementation.md`, `bug-fix-description.md`
- 要件、計画、技術的検討事項を記載

### 開発中
- 進捗状況をissuesファイルに随時更新
- 技術的発見、問題、解決策を記録
- コードレビューやテスト結果を文書化

### 開発完了時
- **必須**: 完成したissuesファイルを `issues/done/` ディレクトリに移動
- 最終的な実装内容、テスト結果、品質保証プロセスの記録を完了
- 将来の参考資料として保存

### ディレクトリ構造
```
issues/
├── development-process-guidelines.md    # 開発プロセス（docs/development/に移動済み）
├── [active-issue-1].md                 # 開発中の課題
├── [active-issue-2].md                 # 開発中の課題
└── done/                                # 完了済み課題
    ├── employee-auto-numbering-implementation.md
    ├── employee-number-duplication-fix.md
    └── employee-auto-numbering-duplication-fix.md
```

## Issues管理のベストプラクティス

### 1. 課題ファイルの命名規則
- **機能追加**: `feature-[機能名]-implementation.md`
- **バグ修正**: `bug-[問題内容]-fix.md`
- **リファクタリング**: `refactor-[対象]-improvement.md`
- **パフォーマンス**: `performance-[対象]-optimization.md`

### 2. 課題ファイルの構成

```markdown
# [課題タイトル]

## 概要
- 課題の背景と目的
- 期待される結果

## 要件
- 具体的な機能要件
- 非機能要件

## 技術的検討
- 実装方針
- 技術的課題
- 代替案

## 実装計画
- [ ] タスク1
- [ ] タスク2
- [ ] タスク3

## 進捗記録
### YYYY-MM-DD
- 実装内容
- 発見事項
- 問題と解決策

## テスト結果
- 単体テスト
- 統合テスト
- E2Eテスト

## 完了基準
- 機能要件の達成
- 品質基準の満足
- ドキュメント更新
```

### 3. 進捗管理
- 日々の作業内容を記録
- 技術的発見や学習内容を詳細に記載
- 問題発生時の対応プロセスを文書化
- コードレビューやテスト結果を記録

### 4. 完了時の処理
- すべての要件が満たされていることを確認
- 最終テスト結果を記録
- `issues/done/` に移動
- 関連ドキュメントの更新状況を確認
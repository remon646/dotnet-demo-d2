# ファイルアップロード機能実装計画

## 概要
社員のプロフィール画像アップロードとCSV一括処理機能を実装し、ファイル管理機能を充実させます。

## 機能要件

### 基本機能
- ✅ 社員プロフィール画像アップロード
- ✅ CSV 社員データ一括インポート
- ✅ CSV 社員データエクスポート
- ✅ ファイル形式・サイズ バリデーション
- ✅ 画像プレビュー機能
- ✅ プログレス表示

### 拡張機能
- ✅ 複数ファイル同時アップロード
- ✅ ドラッグ&ドロップ対応
- ✅ 画像リサイズ・圧縮
- ✅ ファイル管理画面（一覧・削除）
- ✅ アップロード履歴記録

## 技術実装詳細

### 1. ドメインモデル追加

#### FileUpload モデル
```csharp
public class FileUpload
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string OriginalFileName { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public string FilePath { get; set; }
    public string UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public FileUploadType FileType { get; set; }
    public string RelatedEntityId { get; set; }  // 関連エンティティID
    public FileUploadStatus Status { get; set; }
}

public enum FileUploadType
{
    ProfileImage = 1,
    CsvImport = 2,
    Document = 3
}

public enum FileUploadStatus
{
    Uploading = 1,
    Completed = 2,
    Failed = 3,
    Deleted = 4
}
```

#### CSV インポート結果モデル
```csharp
public class CsvImportResult
{
    public int TotalRows { get; set; }
    public int SuccessRows { get; set; }
    public int ErrorRows { get; set; }
    public List<CsvImportError> Errors { get; set; } = new();
    public DateTime ImportedAt { get; set; }
    public string ImportedBy { get; set; }
}

public class CsvImportError
{
    public int RowNumber { get; set; }
    public string FieldName { get; set; }
    public string ErrorMessage { get; set; }
    public string RowData { get; set; }
}
```

### 2. 設定とバリデーション

#### アップロード設定
```csharp
public class FileUploadSettings
{
    public long MaxFileSize { get; set; } = 5 * 1024 * 1024; // 5MB
    public string[] AllowedImageTypes { get; set; } = { ".jpg", ".jpeg", ".png", ".gif" };
    public string[] AllowedCsvTypes { get; set; } = { ".csv" };
    public string UploadDirectory { get; set; } = "uploads";
    public string ProfileImageDirectory { get; set; } = "uploads/profiles";
    public string CsvDirectory { get; set; } = "uploads/csv";
    public int MaxImageWidth { get; set; } = 800;
    public int MaxImageHeight { get; set; } = 600;
}
```

### 3. サービス層実装

#### ファイルアップロードサービス
```csharp
public interface IFileUploadService
{
    Task<FileUploadResult> UploadProfileImageAsync(IBrowserFile file, string employeeId);
    Task<FileUploadResult> UploadCsvFileAsync(IBrowserFile file);
    Task<CsvImportResult> ImportEmployeesFromCsvAsync(int fileId);
    Task<byte[]> ExportEmployeesToCsvAsync();
    Task<bool> DeleteFileAsync(int fileId);
    Task<Stream> GetFileStreamAsync(int fileId);
    Task<List<FileUpload>> GetFilesByTypeAsync(FileUploadType fileType);
}
```

#### 画像処理サービス
```csharp
public interface IImageProcessingService
{
    Task<byte[]> ResizeImageAsync(Stream imageStream, int maxWidth, int maxHeight);
    Task<bool> ValidateImageAsync(Stream imageStream);
    string GetImageMimeType(string fileName);
}
```

### 4. UI コンポーネント

#### プロフィール画像アップロード
- `ProfileImageUpload.razor` コンポーネント
- ドラッグ&ドロップ対応
- プレビュー機能
- プログレス表示

#### CSV インポート画面
- `/employees/import` ページ
- アップロード進捗表示
- インポート結果表示
- エラー詳細表示

#### ファイル管理画面
- `/files` ページ
- ファイル一覧表示
- ダウンロード・削除機能

## ファイル変更計画

### 新規作成ファイル

1. **ドメイン層**
   - `Domain/Models/FileUpload.cs`
   - `Domain/Models/CsvImportResult.cs`
   - `Domain/Interfaces/IFileUploadRepository.cs`

2. **アプリケーション層**
   - `Application/Interfaces/IFileUploadService.cs`
   - `Application/Interfaces/IImageProcessingService.cs`
   - `Application/Services/FileUploadService.cs`
   - `Application/Services/ImageProcessingService.cs`
   - `Application/Services/CsvImportService.cs`

3. **インフラストラクチャー層**
   - `Infrastructure/Repositories/InMemoryFileUploadRepository.cs`

4. **設定**
   - `Models/FileUploadSettings.cs`

5. **UI コンポーネント**
   - `Components/Shared/ProfileImageUpload.razor`
   - `Components/Pages/FileManagement.razor`
   - `Components/Pages/EmployeeImport.razor`
   - `Components/Dialogs/CsvImportResultDialog.razor`
   - `ViewModels/FileUploadViewModel.cs`

### 既存ファイル変更

1. **社員モデル拡張**
   - `Domain/Models/Employee.cs` (プロフィール画像パス追加)

2. **社員編集画面**
   - `Components/Pages/EmployeeEdit.razor`

3. **設定ファイル**
   - `appsettings.json` (アップロード設定追加)

4. **依存性注入**
   - `Program.cs`

5. **ナビゲーション**
   - `Components/Layout/NavMenu.razor`

## 実装手順

### Phase 1: 基盤実装（優先度：最高）
1. FileUpload ドメインモデル作成
2. ファイルアップロード設定
3. 基本的なファイルサービス実装
4. ファイルバリデーション機能

### Phase 2: プロフィール画像（優先度：高）
1. 画像処理サービス実装
2. ProfileImageUpload コンポーネント
3. 社員編集画面への統合
4. 画像表示・プレビュー機能

### Phase 3: CSV インポート/エクスポート（優先度：高）
1. CSV処理サービス実装
2. インポート画面作成
3. エクスポート機能実装
4. バリデーション・エラーハンドリング

### Phase 4: ファイル管理（優先度：中）
1. ファイル管理画面実装
2. アップロード履歴機能
3. ファイル削除機能
4. 管理者向け機能

### Phase 5: 最適化（優先度：低）
1. 複数ファイル同時アップロード
2. チャンク アップロード
3. クライアントサイド画像圧縮
4. キャッシュ機能

## テスト項目

### 単体テスト
- ファイルバリデーション テスト
- 画像リサイズ テスト
- CSV パース テスト
- ファイルサービス メソッドテスト

### 統合テスト
- ファイルアップロード フロー テスト
- CSV インポート シナリオ テスト
- プロフィール画像表示テスト

### UI テスト
- ドラッグ&ドロップ動作
- プログレス表示
- エラーハンドリング表示

## 技術的考慮事項

### セキュリティ
- ファイルタイプ検証（拡張子 + MIME Type）
- ファイルサイズ制限
- 悪意あるファイル検出
- アップロードパス トラバーサル対策

### パフォーマンス
- 大きなファイルの非同期処理
- メモリ効率的なストリーム処理
- 画像圧縮による容量最適化

### ストレージ戦略
- 現在：ローカルファイルシステム
- 将来：Azure Blob Storage 移行準備
- ファイルパス管理の抽象化

## リスクと対策

### 技術的リスク
- **メモリ使用量**: 大きなファイル処理時のメモリ消費
- **ストレージ容量**: アップロードファイルの蓄積
- **セキュリティ**: 悪意あるファイルアップロード

### 対策
- ストリーミング処理によるメモリ最適化
- 定期的なファイルクリーンアップ
- 厳格なファイル検証
- サンドボックス環境での処理

## 見積もり
- **開発工数**: 4-5日
- **テスト工数**: 2-3日
- **総工数**: 6-8日

## 依存関係
- 監査ログ（ファイル操作ログ記録）
- 権限管理（ファイルアクセス制御）
- 通知システム（アップロード完了通知）

## 成功指標
- ✅ ファイルアップロード成功率 > 99%
- ✅ 画像表示速度 < 2秒
- ✅ CSV インポート処理時間 < 10秒（100件）
- ✅ UI操作の直感性
- ✅ セキュリティ要件100%達成

## 将来拡張
- クラウドストレージ統合
- 画像編集機能
- バルクファイル操作
- ファイル共有機能
- バックアップ・復元機能
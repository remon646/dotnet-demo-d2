using System;

namespace EmployeeManagement.Domain.Models
{
    /// <summary>
    /// 監査ログを表すドメインモデル
    /// 全ての操作履歴を記録し、データ変更の追跡可能性を提供します
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// 監査ログの一意識別子
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 操作を実行したユーザーのID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 操作を実行したユーザーの表示名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 実行された操作の種類
        /// Create, Update, Delete, Login, Logout など
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// 操作対象となったエンティティの種類
        /// Employee, Department, User など
        /// </summary>
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// 操作対象となったエンティティのID
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// 変更前の値（JSON形式）
        /// Create操作の場合は空文字列
        /// </summary>
        public string OldValues { get; set; } = string.Empty;

        /// <summary>
        /// 変更後の値（JSON形式）
        /// Delete操作の場合は空文字列
        /// </summary>
        public string NewValues { get; set; } = string.Empty;

        /// <summary>
        /// 操作が実行された日時
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 操作が実行されたIPアドレス
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// 監査ログのレベル
        /// Info, Warning, Error
        /// </summary>
        public AuditLogLevel Level { get; set; }

        /// <summary>
        /// 操作に関する追加情報やコメント
        /// </summary>
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// 監査ログのレベルを定義する列挙型
    /// </summary>
    public enum AuditLogLevel
    {
        /// <summary>
        /// 情報レベル - 通常の操作
        /// </summary>
        Info = 1,

        /// <summary>
        /// 警告レベル - 注意が必要な操作
        /// </summary>
        Warning = 2,

        /// <summary>
        /// エラーレベル - 失敗した操作やエラー
        /// </summary>
        Error = 3
    }
}
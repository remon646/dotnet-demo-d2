using System;
using System.Collections.Generic;

namespace EmployeeManagement.Domain.Models
{
    /// <summary>
    /// 通知を表すドメインモデル
    /// リアルタイム通知とアラートシステムの基盤として機能します
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// 通知の一意識別子
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 通知のタイトル
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 通知メッセージ本文
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 通知の種類
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// 通知の優先度
        /// </summary>
        public NotificationPriority Priority { get; set; }

        /// <summary>
        /// 通知対象ユーザーID（nullの場合は全ユーザー）
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// 通知を作成したユーザーまたはシステム名
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// 通知作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 通知を読んだ日時（未読の場合はnull）
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// 既読フラグ
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// 通知に関連するアクションのURL
        /// </summary>
        public string ActionUrl { get; set; } = string.Empty;

        /// <summary>
        /// アクションボタンのテキスト
        /// </summary>
        public string ActionText { get; set; } = string.Empty;

        /// <summary>
        /// 追加のメタデータ情報
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();

        /// <summary>
        /// 通知の有効期限（nullの場合は無期限）
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// 永続的な通知かどうか（false の場合はトーストのみ表示）
        /// </summary>
        public bool IsPersistent { get; set; } = true;

        /// <summary>
        /// 通知に関連するアイコン名（Material Design Icons）
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// 通知に関連するエンティティのID
        /// </summary>
        public string? EntityId { get; set; }

        /// <summary>
        /// 通知に関連するエンティティの種類
        /// </summary>
        public string? EntityType { get; set; }

        /// <summary>
        /// 通知が削除されたかどうか
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 通知が削除された日時
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// 通知が期限切れかどうかを判定します
        /// </summary>
        /// <returns>期限切れの場合true</returns>
        public bool IsExpired()
        {
            return ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.Now;
        }

        /// <summary>
        /// 通知を既読にマークします
        /// </summary>
        public void MarkAsRead()
        {
            IsRead = true;
            ReadAt = DateTime.Now;
        }

        /// <summary>
        /// 通知を未読にマークします
        /// </summary>
        public void MarkAsUnread()
        {
            IsRead = false;
            ReadAt = null;
        }

        /// <summary>
        /// 通知を削除マークします
        /// </summary>
        public void MarkAsDeleted()
        {
            IsDeleted = true;
            DeletedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// 通知の種類を定義する列挙型
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// システム通知（メンテナンス、アップデート等）
        /// </summary>
        System = 1,

        /// <summary>
        /// データ変更通知（社員・部署の作成・更新・削除）
        /// </summary>
        DataChange = 2,

        /// <summary>
        /// ユーザー操作通知
        /// </summary>
        UserAction = 3,

        /// <summary>
        /// アラート通知
        /// </summary>
        Alert = 4,

        /// <summary>
        /// 警告通知
        /// </summary>
        Warning = 5,

        /// <summary>
        /// エラー通知
        /// </summary>
        Error = 6,

        /// <summary>
        /// 成功通知
        /// </summary>
        Success = 7,

        /// <summary>
        /// 監査ログ関連通知
        /// </summary>
        AuditLog = 8,

        /// <summary>
        /// 権限変更通知
        /// </summary>
        Permission = 9,

        /// <summary>
        /// 緊急通知
        /// </summary>
        Critical = 10
    }

    /// <summary>
    /// 通知の優先度を定義する列挙型
    /// </summary>
    public enum NotificationPriority
    {
        /// <summary>
        /// 低優先度
        /// </summary>
        Low = 1,

        /// <summary>
        /// 通常優先度
        /// </summary>
        Normal = 2,

        /// <summary>
        /// 高優先度
        /// </summary>
        High = 3,

        /// <summary>
        /// 緊急優先度
        /// </summary>
        Critical = 4
    }
}
using System;
using System.Collections.Generic;

namespace EmployeeManagement.Domain.Models
{
    /// <summary>
    /// ユーザーの通知設定を表すドメインモデル
    /// </summary>
    public class NotificationSettings
    {
        /// <summary>
        /// 設定対象のユーザーID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 通知機能全体の有効/無効
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 通知種類別の有効/無効設定
        /// </summary>
        public Dictionary<NotificationType, bool> TypeSettings { get; set; } = new();

        /// <summary>
        /// 音声通知の有効/無効
        /// </summary>
        public bool PlaySound { get; set; } = true;

        /// <summary>
        /// デスクトップ通知の有効/無効
        /// </summary>
        public bool ShowDesktopNotification { get; set; } = true;

        /// <summary>
        /// トースト通知の自動消去秒数（0の場合は手動消去のみ）
        /// </summary>
        public int AutoDismissSeconds { get; set; } = 5;

        /// <summary>
        /// 表示する最小優先度レベル
        /// </summary>
        public NotificationPriority MinimumPriority { get; set; } = NotificationPriority.Low;

        /// <summary>
        /// 通知表示の最大件数
        /// </summary>
        public int MaxDisplayCount { get; set; } = 10;

        /// <summary>
        /// 夜間モード（指定時間は通知を抑制）
        /// </summary>
        public bool QuietHoursEnabled { get; set; } = false;

        /// <summary>
        /// 夜間モード開始時刻
        /// </summary>
        public TimeSpan QuietHoursStart { get; set; } = new TimeSpan(22, 0, 0);

        /// <summary>
        /// 夜間モード終了時刻
        /// </summary>
        public TimeSpan QuietHoursEnd { get; set; } = new TimeSpan(8, 0, 0);

        /// <summary>
        /// 設定の最終更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 設定の作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// デフォルト設定を初期化します
        /// </summary>
        public NotificationSettings()
        {
            InitializeDefaultSettings();
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// 指定された通知種類が有効かどうかを判定します
        /// </summary>
        /// <param name="type">通知種類</param>
        /// <returns>有効な場合true</returns>
        public bool IsTypeEnabled(NotificationType type)
        {
            if (!IsEnabled) return false;
            
            return TypeSettings.TryGetValue(type, out var enabled) ? enabled : true;
        }

        /// <summary>
        /// 指定された優先度が表示対象かどうかを判定します
        /// </summary>
        /// <param name="priority">優先度</param>
        /// <returns>表示対象の場合true</returns>
        public bool ShouldShowPriority(NotificationPriority priority)
        {
            return priority >= MinimumPriority;
        }

        /// <summary>
        /// 現在時刻が夜間モード対象時間かどうかを判定します
        /// </summary>
        /// <returns>夜間モード対象時間の場合true</returns>
        public bool IsQuietHours()
        {
            if (!QuietHoursEnabled) return false;

            var now = DateTime.Now.TimeOfDay;
            
            // 日をまたぐ場合の処理
            if (QuietHoursStart > QuietHoursEnd)
            {
                return now >= QuietHoursStart || now <= QuietHoursEnd;
            }
            else
            {
                return now >= QuietHoursStart && now <= QuietHoursEnd;
            }
        }

        /// <summary>
        /// 通知種類の設定を更新します
        /// </summary>
        /// <param name="type">通知種類</param>
        /// <param name="enabled">有効/無効</param>
        public void SetTypeEnabled(NotificationType type, bool enabled)
        {
            TypeSettings[type] = enabled;
            UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// デフォルト設定を初期化します
        /// </summary>
        private void InitializeDefaultSettings()
        {
            // 全ての通知種類をデフォルトで有効にする
            foreach (NotificationType type in Enum.GetValues<NotificationType>())
            {
                TypeSettings[type] = true;
            }

            // エラーと警告は常に表示
            TypeSettings[NotificationType.Error] = true;
            TypeSettings[NotificationType.Warning] = true;
            TypeSettings[NotificationType.Critical] = true;
        }

        /// <summary>
        /// 設定をリセットしてデフォルトに戻します
        /// </summary>
        public void ResetToDefault()
        {
            IsEnabled = true;
            PlaySound = true;
            ShowDesktopNotification = true;
            AutoDismissSeconds = 5;
            MinimumPriority = NotificationPriority.Low;
            MaxDisplayCount = 10;
            QuietHoursEnabled = false;
            QuietHoursStart = new TimeSpan(22, 0, 0);
            QuietHoursEnd = new TimeSpan(8, 0, 0);
            
            TypeSettings.Clear();
            InitializeDefaultSettings();
            UpdatedAt = DateTime.Now;
        }
    }
}
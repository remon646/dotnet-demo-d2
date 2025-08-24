using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Domain.Interfaces
{
    /// <summary>
    /// 通知設定のデータアクセスを提供するリポジトリインターフェース
    /// </summary>
    public interface INotificationSettingsRepository
    {
        /// <summary>
        /// ユーザーの通知設定を取得します
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>通知設定（存在しない場合はデフォルト設定）</returns>
        Task<NotificationSettings> GetByUserIdAsync(string userId);

        /// <summary>
        /// ユーザーの通知設定を保存します
        /// </summary>
        /// <param name="settings">通知設定</param>
        /// <returns>保存成功の場合true</returns>
        Task<bool> SaveAsync(NotificationSettings settings);

        /// <summary>
        /// 全ユーザーの通知設定を取得します（管理者用）
        /// </summary>
        /// <returns>全ユーザーの通知設定</returns>
        Task<List<NotificationSettings>> GetAllAsync();

        /// <summary>
        /// ユーザーの通知設定を削除します
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>削除成功の場合true</returns>
        Task<bool> DeleteAsync(string userId);

        /// <summary>
        /// デフォルト通知設定を取得します
        /// </summary>
        /// <returns>デフォルト通知設定</returns>
        Task<NotificationSettings> GetDefaultSettingsAsync();

        /// <summary>
        /// ユーザーの通知設定をデフォルトにリセットします
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>リセット成功の場合true</returns>
        Task<bool> ResetToDefaultAsync(string userId);
    }
}
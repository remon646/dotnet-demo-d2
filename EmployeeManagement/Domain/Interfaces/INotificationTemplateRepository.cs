using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeManagement.Domain.Models;

namespace EmployeeManagement.Domain.Interfaces
{
    /// <summary>
    /// 通知テンプレートのデータアクセスを提供するリポジトリインターフェース
    /// </summary>
    public interface INotificationTemplateRepository
    {
        /// <summary>
        /// テンプレートを作成します
        /// </summary>
        /// <param name="template">作成するテンプレート</param>
        /// <returns>作成されたテンプレートのID</returns>
        Task<int> CreateAsync(NotificationTemplate template);

        /// <summary>
        /// テンプレートを更新します
        /// </summary>
        /// <param name="template">更新するテンプレート</param>
        /// <returns>更新成功の場合true</returns>
        Task<bool> UpdateAsync(NotificationTemplate template);

        /// <summary>
        /// テンプレートをIDで取得します
        /// </summary>
        /// <param name="id">テンプレートID</param>
        /// <returns>テンプレート（存在しない場合はnull）</returns>
        Task<NotificationTemplate?> GetByIdAsync(int id);

        /// <summary>
        /// テンプレートを名前で取得します
        /// </summary>
        /// <param name="name">テンプレート名</param>
        /// <returns>テンプレート（存在しない場合はnull）</returns>
        Task<NotificationTemplate?> GetByNameAsync(string name);

        /// <summary>
        /// 全てのテンプレートを取得します
        /// </summary>
        /// <param name="includeInactive">非アクティブなテンプレートを含むかどうか</param>
        /// <param name="category">カテゴリフィルタ（指定なしの場合は全カテゴリ）</param>
        /// <returns>テンプレートのリスト</returns>
        Task<List<NotificationTemplate>> GetAllAsync(bool includeInactive = false, string? category = null);

        /// <summary>
        /// アクティブなテンプレートを種類別で取得します
        /// </summary>
        /// <param name="type">通知種類</param>
        /// <returns>テンプレートのリスト</returns>
        Task<List<NotificationTemplate>> GetActiveByTypeAsync(NotificationType type);

        /// <summary>
        /// システム定義テンプレートを取得します
        /// </summary>
        /// <returns>システムテンプレートのリスト</returns>
        Task<List<NotificationTemplate>> GetSystemTemplatesAsync();

        /// <summary>
        /// テンプレートを削除します
        /// </summary>
        /// <param name="id">テンプレートID</param>
        /// <returns>削除成功の場合true</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// テンプレート名の重複チェックを行います
        /// </summary>
        /// <param name="name">テンプレート名</param>
        /// <param name="excludeId">チェックから除外するID（更新時に使用）</param>
        /// <returns>重複している場合true</returns>
        Task<bool> ExistsAsync(string name, int? excludeId = null);

        /// <summary>
        /// テンプレートのクローンを作成します
        /// </summary>
        /// <param name="sourceId">コピー元のテンプレートID</param>
        /// <param name="newName">新しいテンプレート名</param>
        /// <returns>作成されたテンプレートのID</returns>
        Task<int> CloneAsync(int sourceId, string newName);
    }
}
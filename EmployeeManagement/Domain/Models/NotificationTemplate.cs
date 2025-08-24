using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EmployeeManagement.Domain.Models
{
    /// <summary>
    /// 通知テンプレートを表すドメインモデル
    /// 動的な通知生成のための雛形を提供します
    /// </summary>
    public class NotificationTemplate
    {
        /// <summary>
        /// テンプレートの一意識別子
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// テンプレート名（一意）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// テンプレートの説明
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// タイトルテンプレート（プレースホルダーを含む）
        /// </summary>
        public string TitleTemplate { get; set; } = string.Empty;

        /// <summary>
        /// メッセージテンプレート（プレースホルダーを含む）
        /// </summary>
        public string MessageTemplate { get; set; } = string.Empty;

        /// <summary>
        /// 通知の種類
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// 通知の優先度
        /// </summary>
        public NotificationPriority Priority { get; set; }

        /// <summary>
        /// テンプレートのアクティブ状態
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 関連するアイコン名
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// アクションURLテンプレート
        /// </summary>
        public string ActionUrlTemplate { get; set; } = string.Empty;

        /// <summary>
        /// アクションボタンテキスト
        /// </summary>
        public string ActionText { get; set; } = string.Empty;

        /// <summary>
        /// 通知の有効期限（日数）
        /// </summary>
        public int? ExpirationDays { get; set; }

        /// <summary>
        /// テンプレートのパラメーター定義
        /// </summary>
        public List<NotificationTemplateParameter> Parameters { get; set; } = new();

        /// <summary>
        /// テンプレートの作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// テンプレートの最終更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// テンプレートの作成者
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// テンプレートの最終更新者
        /// </summary>
        public string UpdatedBy { get; set; } = string.Empty;

        /// <summary>
        /// テンプレートのカテゴリ
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// システム定義テンプレートかどうか
        /// </summary>
        public bool IsSystemTemplate { get; set; }

        /// <summary>
        /// パラメーターを使用してタイトルを生成します
        /// </summary>
        /// <param name="parameters">パラメーター値</param>
        /// <returns>生成されたタイトル</returns>
        public string GenerateTitle(Dictionary<string, string> parameters)
        {
            return ReplaceParameters(TitleTemplate, parameters);
        }

        /// <summary>
        /// パラメーターを使用してメッセージを生成します
        /// </summary>
        /// <param name="parameters">パラメーター値</param>
        /// <returns>生成されたメッセージ</returns>
        public string GenerateMessage(Dictionary<string, string> parameters)
        {
            return ReplaceParameters(MessageTemplate, parameters);
        }

        /// <summary>
        /// パラメーターを使用してアクションURLを生成します
        /// </summary>
        /// <param name="parameters">パラメーター値</param>
        /// <returns>生成されたアクションURL</returns>
        public string GenerateActionUrl(Dictionary<string, string> parameters)
        {
            return ReplaceParameters(ActionUrlTemplate, parameters);
        }

        /// <summary>
        /// テンプレートからNotificationオブジェクトを生成します
        /// </summary>
        /// <param name="parameters">パラメーター値</param>
        /// <param name="userId">対象ユーザーID</param>
        /// <param name="createdBy">作成者</param>
        /// <returns>生成された通知</returns>
        public Notification GenerateNotification(Dictionary<string, string> parameters, string? userId = null, string createdBy = "System")
        {
            ValidateParameters(parameters);

            var notification = new Notification
            {
                Title = GenerateTitle(parameters),
                Message = GenerateMessage(parameters),
                Type = Type,
                Priority = Priority,
                UserId = userId,
                CreatedBy = createdBy,
                CreatedAt = DateTime.Now,
                ActionUrl = GenerateActionUrl(parameters),
                ActionText = ActionText,
                Icon = Icon
            };

            // 有効期限を設定
            if (ExpirationDays.HasValue)
            {
                notification.ExpiresAt = DateTime.Now.AddDays(ExpirationDays.Value);
            }

            return notification;
        }

        /// <summary>
        /// 必須パラメーターが提供されているかを検証します
        /// </summary>
        /// <param name="parameters">パラメーター値</param>
        /// <exception cref="ArgumentException">必須パラメーターが不足している場合</exception>
        public void ValidateParameters(Dictionary<string, string> parameters)
        {
            var missingParameters = Parameters
                .Where(p => p.IsRequired)
                .Where(p => !parameters.ContainsKey(p.Name) || string.IsNullOrEmpty(parameters[p.Name]))
                .Select(p => p.Name)
                .ToList();

            if (missingParameters.Any())
            {
                throw new ArgumentException($"必須パラメーターが不足しています: {string.Join(", ", missingParameters)}");
            }
        }

        /// <summary>
        /// テンプレート内のプレースホルダーをパラメーター値で置換します
        /// </summary>
        /// <param name="template">テンプレート文字列</param>
        /// <param name="parameters">パラメーター値</param>
        /// <returns>置換後の文字列</returns>
        private string ReplaceParameters(string template, Dictionary<string, string> parameters)
        {
            if (string.IsNullOrEmpty(template))
                return string.Empty;

            var result = template;

            // {ParameterName} 形式のプレースホルダーを置換
            foreach (var param in parameters)
            {
                var placeholder = $"{{{param.Key}}}";
                result = result.Replace(placeholder, param.Value ?? string.Empty);
            }

            // デフォルト値を持つ未置換のプレースホルダーを処理
            foreach (var templateParam in Parameters.Where(p => !string.IsNullOrEmpty(p.DefaultValue)))
            {
                var placeholder = $"{{{templateParam.Name}}}";
                if (result.Contains(placeholder))
                {
                    result = result.Replace(placeholder, templateParam.DefaultValue);
                }
            }

            return result;
        }

        /// <summary>
        /// テンプレートで使用されているパラメーター名を抽出します
        /// </summary>
        /// <returns>使用されているパラメーター名のリスト</returns>
        public List<string> ExtractUsedParameters()
        {
            var parameters = new HashSet<string>();
            var templates = new[] { TitleTemplate, MessageTemplate, ActionUrlTemplate };

            var regex = new Regex(@"\{([^}]+)\}", RegexOptions.Compiled);

            foreach (var template in templates.Where(t => !string.IsNullOrEmpty(t)))
            {
                var matches = regex.Matches(template);
                foreach (Match match in matches)
                {
                    parameters.Add(match.Groups[1].Value);
                }
            }

            return parameters.ToList();
        }

        /// <summary>
        /// パラメーターを追加します
        /// </summary>
        /// <param name="name">パラメーター名</param>
        /// <param name="description">説明</param>
        /// <param name="isRequired">必須かどうか</param>
        /// <param name="defaultValue">デフォルト値</param>
        public void AddParameter(string name, string description, bool isRequired = false, string defaultValue = "")
        {
            if (Parameters.Any(p => p.Name == name))
            {
                throw new ArgumentException($"パラメーター '{name}' は既に存在します。");
            }

            Parameters.Add(new NotificationTemplateParameter
            {
                Name = name,
                Description = description,
                IsRequired = isRequired,
                DefaultValue = defaultValue
            });

            UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// テンプレートのクローンを作成します
        /// </summary>
        /// <returns>クローンされたテンプレート</returns>
        public NotificationTemplate Clone()
        {
            return new NotificationTemplate
            {
                Name = $"{Name}_Copy",
                Description = Description,
                TitleTemplate = TitleTemplate,
                MessageTemplate = MessageTemplate,
                Type = Type,
                Priority = Priority,
                IsActive = IsActive,
                Icon = Icon,
                ActionUrlTemplate = ActionUrlTemplate,
                ActionText = ActionText,
                ExpirationDays = ExpirationDays,
                Parameters = Parameters.Select(p => new NotificationTemplateParameter
                {
                    Name = p.Name,
                    Description = p.Description,
                    IsRequired = p.IsRequired,
                    DefaultValue = p.DefaultValue
                }).ToList(),
                Category = Category,
                IsSystemTemplate = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }
    }

    /// <summary>
    /// 通知テンプレートのパラメーターを表すクラス
    /// </summary>
    public class NotificationTemplateParameter
    {
        /// <summary>
        /// パラメーター名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// パラメーターの説明
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 必須パラメーターかどうか
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// デフォルト値
        /// </summary>
        public string DefaultValue { get; set; } = string.Empty;

        /// <summary>
        /// パラメーターの型（将来の拡張用）
        /// </summary>
        public string Type { get; set; } = "string";

        /// <summary>
        /// パラメーターのバリデーションルール（将来の拡張用）
        /// </summary>
        public string ValidationRule { get; set; } = string.Empty;
    }
}
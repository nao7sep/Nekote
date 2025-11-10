namespace Nekote.Core.AI.Domain.Chat
{
    /// <summary>
    /// チャット補完のオプション設定を表します。
    /// </summary>
    public sealed class ChatCompletionOptions
    {
        /// <summary>
        /// サンプリング温度 (0.0 ～ 2.0) を取得します。
        /// </summary>
        public float? Temperature { get; init; }

        /// <summary>
        /// 生成する最大トークン数を取得します。
        /// </summary>
        public int? MaxTokens { get; init; }

        /// <summary>
        /// Top-p サンプリング値を取得します。
        /// </summary>
        public float? TopP { get; init; }

        /// <summary>
        /// 使用するモデル名を取得します (オプション、未設定なら設定のデフォルトを使用)。
        /// </summary>
        public string? ModelName { get; init; }

        /// <summary>
        /// プロバイダー固有の追加パラメータを取得します。
        /// キーはパラメータ名、値はパラメータ値です。
        /// 例: OpenAI の "response_format", Gemini の "safety_settings" など。
        /// </summary>
        public Dictionary<string, object>? ProviderSpecificParameters { get; init; }
    }
}

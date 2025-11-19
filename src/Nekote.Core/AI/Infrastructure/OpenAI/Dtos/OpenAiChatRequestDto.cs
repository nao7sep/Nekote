using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// OpenAI Chat API へのリクエストボディ DTO。
    /// </summary>
    public class OpenAiChatRequestDto
    {
        /// <summary>
        /// チャットメッセージのリスト。
        /// </summary>
        [JsonPropertyName("messages")]
        public List<OpenAiChatMessageDto>? Messages { get; set; }

        /// <summary>
        /// 使用するモデルの識別子。
        /// </summary>
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        /// <summary>
        /// オーディオ出力のパラメータ。
        /// </summary>
        [JsonPropertyName("audio")]
        public OpenAiChatAudioParametersDto? Audio { get; set; }

        /// <summary>
        /// 頻度ペナルティ (-2.0 ~ 2.0)。
        /// </summary>
        [JsonPropertyName("frequency_penalty")]
        public double? FrequencyPenalty { get; set; }

        // function_call, functions, parallel_tool_calls, tool_choice, tools は、
        // このライブラリが現在ツール関連の操作をサポートしていないため省略されている。
        // API からのレスポンスを解析する DTO ではデータの損失を防ぐためにこれらのフィールドをサポートするが、
        // リクエスト DTO ではツール関連のデータを送信しないため含まれていない。

        /// <summary>
        /// トークン出現確率の調整マップ。
        /// </summary>
        [JsonPropertyName("logit_bias")]
        public Dictionary<string, int>? LogitBias { get; set; }

        /// <summary>
        /// ログ確率を返すかどうか。
        /// </summary>
        [JsonPropertyName("logprobs")]
        public bool? Logprobs { get; set; }

        /// <summary>
        /// 生成可能な最大トークン数 (推論トークンを含む)。
        /// </summary>
        [JsonPropertyName("max_completion_tokens")]
        public int? MaxCompletionTokens { get; set; }

        /// <summary>
        /// 生成可能な最大トークン数 (非推奨)。
        /// </summary>
        /// <remarks>
        /// このフィールドは非推奨となり、max_completion_tokens に置き換えられた。
        /// </remarks>
        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; set; }

        /// <summary>
        /// メタデータのキーバリューペア (最大16個)。
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// 出力モダリティ (例: ["text", "audio"])。
        /// </summary>
        [JsonPropertyName("modalities")]
        public List<string>? Modalities { get; set; }

        /// <summary>
        /// 生成する候補の数。
        /// </summary>
        [JsonPropertyName("n")]
        public int? N { get; set; }

        /// <summary>
        /// 予測出力の構成。
        /// </summary>
        [JsonPropertyName("prediction")]
        public OpenAiChatPredictionDto? Prediction { get; set; }

        /// <summary>
        /// 存在ペナルティ (-2.0 ~ 2.0)。
        /// </summary>
        [JsonPropertyName("presence_penalty")]
        public double? PresencePenalty { get; set; }

        /// <summary>
        /// プロンプトキャッシュキー。
        /// </summary>
        [JsonPropertyName("prompt_cache_key")]
        public string? PromptCacheKey { get; set; }

        /// <summary>
        /// プロンプトキャッシュの保持期間 (例: "24h")。
        /// </summary>
        [JsonPropertyName("prompt_cache_retention")]
        public string? PromptCacheRetention { get; set; }

        /// <summary>
        /// 推論努力のレベル (例: "low", "medium", "high")。
        /// </summary>
        [JsonPropertyName("reasoning_effort")]
        public string? ReasoningEffort { get; set; }

        /// <summary>
        /// レスポンスフォーマットの指定。
        /// </summary>
        [JsonPropertyName("response_format")]
        public OpenAiChatResponseFormatDto? ResponseFormat { get; set; }

        /// <summary>
        /// 安全性識別子。
        /// </summary>
        [JsonPropertyName("safety_identifier")]
        public string? SafetyIdentifier { get; set; }

        /// <summary>
        /// 決定論的サンプリングのシード値 (非推奨)。
        /// </summary>
        [JsonPropertyName("seed")]
        public int? Seed { get; set; }

        /// <summary>
        /// サービス層 (例: "auto", "default", "flex", "priority")。
        /// </summary>
        [JsonPropertyName("service_tier")]
        public string? ServiceTier { get; set; }

        /// <summary>
        /// 生成停止シーケンス (最大4個)。
        /// リクエスト送信時は単一の文字列または文字列の配列を使用する。
        /// </summary>
        [JsonPropertyName("stop")]
        [JsonConverter(typeof(OpenAiChatStopConverter))]
        public OpenAiChatStopBaseDto? Stop { get; set; }

        /// <summary>
        /// 出力を保存するかどうか (デフォルト: false)。
        /// </summary>
        [JsonPropertyName("store")]
        public bool? Store { get; set; }

        /// <summary>
        /// ストリーミングレスポンスを有効にするかどうか。
        /// </summary>
        [JsonPropertyName("stream")]
        public bool? Stream { get; set; }

        /// <summary>
        /// ストリーミングレスポンスのオプション。
        /// </summary>
        [JsonPropertyName("stream_options")]
        public OpenAiChatStreamOptionsDto? StreamOptions { get; set; }

        /// <summary>
        /// サンプリング温度 (0 ~ 2)。
        /// </summary>
        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }

        /// <summary>
        /// 返すトップログ確率の数 (0 ~ 20)。
        /// </summary>
        [JsonPropertyName("top_logprobs")]
        public int? TopLogprobs { get; set; }

        /// <summary>
        /// Nucleus サンプリングの確率質量 (0 ~ 1)。
        /// </summary>
        [JsonPropertyName("top_p")]
        public double? TopP { get; set; }

        /// <summary>
        /// エンドユーザーの識別子 (非推奨)。
        /// </summary>
        /// <remarks>
        /// このフィールドは非推奨となり、safety_identifier と prompt_cache_key に置き換えられた。
        /// </remarks>
        [JsonPropertyName("user")]
        public string? User { get; set; }

        /// <summary>
        /// レスポンスの冗長性レベル (例: "low", "medium", "high")。
        /// </summary>
        [JsonPropertyName("verbosity")]
        public string? Verbosity { get; set; }

        /// <summary>
        /// Web 検索オプション。
        /// </summary>
        [JsonPropertyName("web_search_options")]
        public OpenAiChatWebSearchOptionsDto? WebSearchOptions { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}

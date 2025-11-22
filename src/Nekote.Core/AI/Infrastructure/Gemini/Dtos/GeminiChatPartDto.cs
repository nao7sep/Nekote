using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Chat API のパート。
    /// </summary>
    public class GeminiChatPartDto
    {
        /// <summary>
        /// モデルによって考えられた部分かどうかを示す。
        /// </summary>
        [JsonPropertyName("thought")]
        public bool? Thought { get; set; }

        /// <summary>
        /// 後続のリクエストで再利用できるように、思考の不透明な署名 (Base64 エンコード)。
        /// </summary>
        [JsonPropertyName("thoughtSignature")]
        public string? ThoughtSignature { get; set; }

        /// <summary>
        /// Part に関連付けられたカスタム メタデータ。
        /// </summary>
        [JsonPropertyName("partMetadata")]
        public JsonElement? PartMetadata { get; set; }

        /// <summary>
        /// 入力メディアのメディア解像度。
        /// </summary>
        [JsonPropertyName("mediaResolution")]
        public GeminiChatMediaResolutionDto? MediaResolution { get; set; }

        /// <summary>
        /// テキスト内容。
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// インライン メディア バイト。
        /// </summary>
        [JsonPropertyName("inlineData")]
        public GeminiChatBlobDto? InlineData { get; set; }

        /// <summary>
        /// モデルから返される、予測された関数呼び出し。
        /// </summary>
        [JsonPropertyName("functionCall")]
        public GeminiChatFunctionCallDto? FunctionCall { get; set; }

        /// <summary>
        /// 関数呼び出しの結果の出力。
        /// </summary>
        [JsonPropertyName("functionResponse")]
        public GeminiChatFunctionResponseDto? FunctionResponse { get; set; }

        /// <summary>
        /// URI ベースのデータ。
        /// </summary>
        [JsonPropertyName("fileData")]
        public GeminiChatFileDataDto? FileData { get; set; }

        /// <summary>
        /// 実行されることを目的とし、モデルによって生成されたコード。
        /// </summary>
        [JsonPropertyName("executableCode")]
        public GeminiChatExecutableCodeDto? ExecutableCode { get; set; }

        /// <summary>
        /// 実行可能なコードの実行結果。
        /// </summary>
        [JsonPropertyName("codeExecutionResult")]
        public GeminiChatCodeExecutionResultDto? CodeExecutionResult { get; set; }

        /// <summary>
        /// 動画のメタデータ。
        /// </summary>
        [JsonPropertyName("videoMetadata")]
        public GeminiChatVideoMetadataDto? VideoMetadata { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}

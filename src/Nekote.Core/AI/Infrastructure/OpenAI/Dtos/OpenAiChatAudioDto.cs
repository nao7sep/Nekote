using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// メッセージのオーディオ情報 DTO。
    /// レスポンス受信時はすべてのフィールドが含まれる。
    /// リクエスト送信時は id のみを使用して以前のオーディオを参照する。
    /// </summary>
    public class OpenAiChatAudioDto
    {
        /// <summary>
        /// オーディオの一意識別子。
        /// レスポンス受信時はこの ID をキャッシュし、リクエスト送信時に再利用する。
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// モデルが生成した Base64 エンコードされたオーディオバイト (レスポンスのみ)。
        /// </summary>
        [JsonPropertyName("data")]
        public string? Data { get; set; }

        /// <summary>
        /// このオーディオがサーバー上でアクセス不可になる Unix タイムスタンプ (秒) (レスポンスのみ)。
        /// </summary>
        [JsonPropertyName("expires_at")]
        public long? ExpiresAt { get; set; }

        /// <summary>
        /// モデルが生成したオーディオのトランスクリプト (レスポンスのみ)。
        /// </summary>
        [JsonPropertyName("transcript")]
        public string? Transcript { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}

namespace Nekote.Core.AI.Infrastructure.Google
{
    /// <summary>
    /// Google AI サービスの設定を定義します。
    /// </summary>
    public sealed class GoogleConfiguration
    {
        /// <summary>
        /// デフォルトの API キーを取得します。
        /// </summary>
        public string? DefaultApiKey { get; init; }

        /// <summary>
        /// チャット専用の API キーを取得します。
        /// </summary>
        public string? ChatApiKey { get; init; }

        /// <summary>
        /// エンベディング専用の API キーを取得します。
        /// </summary>
        public string? EmbeddingApiKey { get; init; }

        /// <summary>
        /// デフォルトのベース URL を取得します。
        /// </summary>
        public string? BaseUrl { get; init; }

        /// <summary>
        /// チャット専用のエンドポイントを取得します。
        /// </summary>
        public string? ChatEndpoint { get; init; }

        /// <summary>
        /// エンベディング専用のエンドポイントを取得します。
        /// </summary>
        public string? EmbeddingEndpoint { get; init; }

        /// <summary>
        /// デフォルトのモデル名を取得します。
        /// </summary>
        public string? DefaultModelName { get; init; }

        /// <summary>
        /// チャット専用のモデル名を取得します。
        /// </summary>
        public string? ChatModelName { get; init; }

        /// <summary>
        /// エンベディング専用のモデル名を取得します。
        /// </summary>
        public string? EmbeddingModelName { get; init; }
    }
}

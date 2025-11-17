using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// OpenAI の "content" プロパティをデシリアライズするカスタム コンバーター。
    /// JSON の型 (string, array, null) に応じて、
    /// OpenAiChatMessageContentBaseDto の適切な派生クラスをインスタンス化する。
    /// </summary>
    internal class OpenAiChatMessageContentConverter : JsonConverter<OpenAiChatMessageContentBaseDto>
    {
        /// <summary>
        /// JSON から OpenAiChatMessageContentBaseDto を読み取る。
        /// </summary>
        public override OpenAiChatMessageContentBaseDto Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                // Case 1: "content": "Hello world"
                case JsonTokenType.String:
                    return new OpenAiChatMessageContentStringDto { Text = reader.GetString() };

                // Case 2: "content": [ { "type": "text", ... } ]
                case JsonTokenType.StartArray:
                    var parts = JsonSerializer.Deserialize<List<OpenAiChatMessageContentPartDto>>(ref reader, options);
                    return new OpenAiChatMessageContentArrayDto { Parts = parts ?? new() };

                // Case 3: "content": null (例: ツール呼び出し応答時)
                case JsonTokenType.Null:
                    return new OpenAiChatMessageContentStringDto { Text = "" };

                default:
                    throw new JsonException(
                        $"Cannot deserialize 'content'. Expected string, array, or null, but got {reader.TokenType}.");
            }
        }

        /// <summary>
        /// OpenAiChatMessageContentBaseDto を JSON に書き込む。
        /// </summary>
        public override void Write(
            Utf8JsonWriter writer,
            OpenAiChatMessageContentBaseDto value,
            JsonSerializerOptions options)
        {
            switch (value)
            {
                case OpenAiChatMessageContentStringDto stringContent:
                    writer.WriteStringValue(stringContent.Text);
                    break;

                case OpenAiChatMessageContentArrayDto arrayContent:
                    JsonSerializer.Serialize(writer, arrayContent.Parts, options);
                    break;

                case null:
                    writer.WriteNullValue();
                    break;
            }
        }
    }
}

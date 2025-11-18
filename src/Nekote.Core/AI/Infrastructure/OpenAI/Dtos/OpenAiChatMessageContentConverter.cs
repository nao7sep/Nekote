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
                // "content": "Hello world"
                case JsonTokenType.String:
                    return new OpenAiChatMessageContentStringDto { Text = reader.GetString() };

                // "content": [ { "type": "text", ... } ]
                case JsonTokenType.StartArray:
                    var parts = JsonSerializer.Deserialize<List<OpenAiChatMessageContentPartDto>>(ref reader, options)
                        ?? throw new JsonException("Cannot deserialize 'content' array. Deserialization returned null.");
                    return new OpenAiChatMessageContentArrayDto { Parts = parts };

                default:
                    throw new JsonException(
                        $"Cannot deserialize 'content'. Expected string or array, but got {reader.TokenType}.");
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

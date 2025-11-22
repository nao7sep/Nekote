using System.Text.Json;
using System.Text.Json.Serialization;
using Nekote.Core.AI.Infrastructure.OpenAI.Dtos;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Converters
{
    /// <summary>
    /// OpenAI の "content" プロパティをデシリアライズするカスタム コンバーター。
    /// JSON の型 (string, array, null) に応じて、
    /// <see cref="OpenAiChatMessageContentBaseDto"/> の適切な派生クラスをインスタンス化する。
    /// </summary>
    public class OpenAiChatMessageContentConverter : JsonConverter<OpenAiChatMessageContentBaseDto>
    {
        /// <summary>
        /// JSON から <see cref="OpenAiChatMessageContentBaseDto"/> を読み取る。
        /// </summary>
        public override OpenAiChatMessageContentBaseDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                // "content": "Hello world"
                case JsonTokenType.String:
                    return new OpenAiChatMessageContentStringDto { Text = reader.GetString() };

                // "content": [ { "type": "text", ... } ]
                case JsonTokenType.StartArray:
                    return new OpenAiChatMessageContentArrayDto { Parts = JsonSerializer.Deserialize<List<OpenAiChatMessageContentPartBaseDto>>(ref reader, options) };

                // "content": null
                case JsonTokenType.Null:
                    return null;

                default:
                    throw new JsonException(
                        $"Cannot deserialize 'content'. Expected string, array, or null, but got {reader.TokenType}.");
            }
        }

        /// <summary>
        /// <see cref="OpenAiChatMessageContentBaseDto"/> を JSON に書き込む。
        /// </summary>
        public override void Write(Utf8JsonWriter writer, OpenAiChatMessageContentBaseDto? value, JsonSerializerOptions options)
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

                default:
                    throw new JsonException(
                        $"Cannot serialize 'content'. Unexpected type: {value.GetType().Name}.");
            }
        }
    }
}

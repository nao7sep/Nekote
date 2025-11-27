using System.Text.Json;
using System.Text.Json.Serialization;
using Nekote.Core.AI.Infrastructure.OpenAI.Dtos;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Converters
{
    /// <summary>
    /// OpenAI の "prediction/content" プロパティをシリアライズ/デシリアライズするカスタム コンバーター。
    /// JSON の型（string, array）に応じて、
    /// <see cref="OpenAiChatPredictionContentBaseDto"/> の適切な派生クラスをインスタンス化する。
    /// </summary>
    public class OpenAiChatPredictionContentConverter : JsonConverter<OpenAiChatPredictionContentBaseDto>
    {
        /// <summary>
        /// JSON から <see cref="OpenAiChatPredictionContentBaseDto"/> を読み取る。
        /// </summary>
        public override OpenAiChatPredictionContentBaseDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                // "content": "Hello world"
                case JsonTokenType.String:
                    return new OpenAiChatPredictionContentStringDto { Text = reader.GetString() };

                // "content": [ { "type": "text", ... } ]
                case JsonTokenType.StartArray:
                    return new OpenAiChatPredictionContentArrayDto { Parts = JsonSerializer.Deserialize<List<OpenAiChatPredictionContentPartDto>>(ref reader, options) };

                // "content": null
                case JsonTokenType.Null:
                    return null;

                default:
                    throw new JsonException(
                        $"Cannot deserialize prediction 'content'. Expected string, array, or null, but got {reader.TokenType}.");
            }
        }

        /// <summary>
        /// <see cref="OpenAiChatPredictionContentBaseDto"/> を JSON に書き込む。
        /// </summary>
        public override void Write(Utf8JsonWriter writer, OpenAiChatPredictionContentBaseDto? value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case OpenAiChatPredictionContentStringDto stringContent:
                    writer.WriteStringValue(stringContent.Text);
                    break;

                case OpenAiChatPredictionContentArrayDto arrayContent:
                    JsonSerializer.Serialize(writer, arrayContent.Parts, options);
                    break;

                case null:
                    writer.WriteNullValue();
                    break;

                default:
                    throw new JsonException(
                        $"Cannot serialize prediction 'content'. Unexpected type: {value.GetType().Name}.");
            }
        }
    }
}

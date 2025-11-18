using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// OpenAI の "prediction" 内の "content" プロパティをシリアライズ/デシリアライズするカスタム コンバーター。
    /// JSON の型 (string, array) に応じて、
    /// OpenAiChatPredictionContentBaseDto の適切な派生クラスをインスタンス化する。
    /// </summary>
    internal class OpenAiChatPredictionContentConverter : JsonConverter<OpenAiChatPredictionContentBaseDto>
    {
        /// <summary>
        /// JSON から OpenAiChatPredictionContentBaseDto を読み取る。
        /// </summary>
        public override OpenAiChatPredictionContentBaseDto Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                // "content": "Hello world"
                case JsonTokenType.String:
                    return new OpenAiChatPredictionContentStringDto { Text = reader.GetString() };

                // "content": [ { "type": "text", ... } ]
                case JsonTokenType.StartArray:
                    var parts = JsonSerializer.Deserialize<List<OpenAiChatPredictionContentPartDto>>(ref reader, options)
                        ?? throw new JsonException("Cannot deserialize prediction 'content' array. Deserialization returned null.");
                    return new OpenAiChatPredictionContentArrayDto { Parts = parts };

                default:
                    throw new JsonException(
                        $"Cannot deserialize prediction 'content'. Expected string or array, but got {reader.TokenType}.");
            }
        }

        /// <summary>
        /// OpenAiChatPredictionContentBaseDto を JSON に書き込む。
        /// </summary>
        public override void Write(
            Utf8JsonWriter writer,
            OpenAiChatPredictionContentBaseDto value,
            JsonSerializerOptions options)
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
            }
        }
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;
using Nekote.Core.AI.Infrastructure.OpenAI.Dtos;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Converters
{
    /// <summary>
    /// OpenAI の "stop" プロパティをシリアライズ/デシリアライズするカスタム コンバーター。
    /// JSON の型 (string, array, null) に応じて、
    /// OpenAiChatStopBaseDto の適切な派生クラスをインスタンス化する。
    /// </summary>
    public class OpenAiChatStopConverter : JsonConverter<OpenAiChatStopBaseDto>
    {
        /// <summary>
        /// JSON から OpenAiChatStopBaseDto を読み取る。
        /// </summary>
        public override OpenAiChatStopBaseDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                // "stop": "string"
                case JsonTokenType.String:
                    return new OpenAiChatStopStringDto { Sequence = reader.GetString() };

                // "stop": ["string1", "string2"]
                case JsonTokenType.StartArray:
                    return new OpenAiChatStopArrayDto { Sequences = JsonSerializer.Deserialize<List<string>>(ref reader, options) };

                // "stop": null
                case JsonTokenType.Null:
                    return null;

                default:
                    throw new JsonException(
                        $"Cannot deserialize 'stop'. Expected string, array, or null, but got {reader.TokenType}.");
            }
        }

        /// <summary>
        /// OpenAiChatStopBaseDto を JSON に書き込む。
        /// </summary>
        public override void Write(Utf8JsonWriter writer, OpenAiChatStopBaseDto? value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case OpenAiChatStopStringDto stringStop:
                    writer.WriteStringValue(stringStop.Sequence);
                    break;

                case OpenAiChatStopArrayDto arrayStop:
                    JsonSerializer.Serialize(writer, arrayStop.Sequences, options);
                    break;

                case null:
                    writer.WriteNullValue();
                    break;

                default:
                    throw new JsonException(
                        $"Cannot serialize 'stop'. Unexpected type: {value.GetType().Name}.");
            }
        }
    }
}

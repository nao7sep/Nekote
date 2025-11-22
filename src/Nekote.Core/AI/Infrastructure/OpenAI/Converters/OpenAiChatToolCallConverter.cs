using System.Text.Json;
using System.Text.Json.Serialization;
using Nekote.Core.AI.Infrastructure.OpenAI.Dtos;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Converters
{
    /// <summary>
    /// OpenAI の "tool_calls" 配列要素をデシリアライズするカスタム コンバーター。
    /// JSON の "type" フィールドに応じて、
    /// <see cref="OpenAiChatToolCallBaseDto"/> の適切な派生クラスをインスタンス化する。
    /// </summary>
    public class OpenAiChatToolCallConverter : JsonConverter<OpenAiChatToolCallBaseDto>
    {
        /// <summary>
        /// JSON から <see cref="OpenAiChatToolCallBaseDto"/> を読み取る。
        /// </summary>
        public override OpenAiChatToolCallBaseDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                if (!root.TryGetProperty("type", out var typeProperty))
                {
                    throw new JsonException("Cannot deserialize 'tool_calls' element. Missing 'type' property.");
                }

                var typeValue = typeProperty.GetString();
                var json = root.GetRawText();

                switch (typeValue)
                {
                    case "function":
                        return JsonSerializer.Deserialize<OpenAiChatToolCallFunctionToolDto>(json, options);

                    case "custom":
                        return JsonSerializer.Deserialize<OpenAiChatToolCallCustomToolDto>(json, options);

                    default:
                        throw new JsonException($"Cannot deserialize 'tool_calls' element. Unknown type: {typeValue}");
                }
            }
        }

        /// <summary>
        /// <see cref="OpenAiChatToolCallBaseDto"/> を JSON に書き込む。
        /// </summary>
        public override void Write(Utf8JsonWriter writer, OpenAiChatToolCallBaseDto? value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case OpenAiChatToolCallFunctionToolDto functionToolCall:
                    JsonSerializer.Serialize(writer, functionToolCall, options);
                    break;

                case OpenAiChatToolCallCustomToolDto customToolCall:
                    JsonSerializer.Serialize(writer, customToolCall, options);
                    break;

                case null:
                    writer.WriteNullValue();
                    break;

                default:
                    throw new JsonException(
                        $"Cannot serialize 'tool_calls' element. Unexpected type: {value.GetType().Name}.");
            }
        }
    }
}

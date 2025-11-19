using System.Text.Json;
using System.Text.Json.Serialization;
using Nekote.Core.AI.Infrastructure.OpenAI.Dtos;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Converters
{
    /// <summary>
    /// OpenAI の "tool_calls" 配列要素をデシリアライズするカスタム コンバーター。
    /// JSON の "type" フィールドに応じて、
    /// OpenAiChatToolCallBaseDto の適切な派生クラスをインスタンス化する。
    /// </summary>
    public class OpenAiChatToolCallConverter : JsonConverter<OpenAiChatToolCallBaseDto>
    {
        /// <summary>
        /// JSON から OpenAiChatToolCallBaseDto を読み取る。
        /// </summary>
        public override OpenAiChatToolCallBaseDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException(
                    $"Cannot deserialize 'tool_calls' element. Expected object or null, but got {reader.TokenType}.");
            }

            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = document.RootElement;

                if (root.TryGetProperty("type", out JsonElement typeElement))
                {
                    string? type = typeElement.GetString();

                    switch (type)
                    {
                        case "function":
                            return JsonSerializer.Deserialize<OpenAiChatToolCallFunctionToolDto>(root.GetRawText(), options);

                        case "custom":
                            return JsonSerializer.Deserialize<OpenAiChatToolCallCustomToolDto>(root.GetRawText(), options);

                        default:
                            throw new JsonException(
                                $"Cannot deserialize 'tool_calls' element. Unknown type: '{type}'.");
                    }
                }

                throw new JsonException(
                    "Cannot deserialize 'tool_calls' element. Missing 'type' property.");
            }
        }

        /// <summary>
        /// OpenAiChatToolCallBaseDto を JSON に書き込む。
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

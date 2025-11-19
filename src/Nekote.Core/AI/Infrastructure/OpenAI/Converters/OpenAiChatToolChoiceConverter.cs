using System.Text.Json;
using System.Text.Json.Serialization;
using Nekote.Core.AI.Infrastructure.OpenAI.Dtos;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Converters
{
    /// <summary>
    /// OpenAI の "tool_choice" プロパティをデシリアライズするカスタム コンバーター。
    /// JSON の型 (string, object) に応じて、
    /// <see cref="OpenAiChatToolChoiceBaseDto"/> の適切な派生クラスをインスタンス化する。
    /// </summary>
    public class OpenAiChatToolChoiceConverter : JsonConverter<OpenAiChatToolChoiceBaseDto>
    {
        /// <summary>
        /// JSON から <see cref="OpenAiChatToolChoiceBaseDto"/> を読み取る。
        /// </summary>
        public override OpenAiChatToolChoiceBaseDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                // "tool_choice": "none" or "auto" or "required"
                case JsonTokenType.String:
                    return new OpenAiChatToolChoiceStringDto { Value = reader.GetString() };

                // "tool_choice": { "type": "...", ... }
                case JsonTokenType.StartObject:
                    using (var doc = JsonDocument.ParseValue(ref reader))
                    {
                        var root = doc.RootElement;
                        if (!root.TryGetProperty("type", out var typeProperty))
                        {
                            throw new JsonException("Cannot deserialize 'tool_choice' object. Missing 'type' property.");
                        }

                        var typeValue = typeProperty.GetString();
                        var json = root.GetRawText();

                        return typeValue switch
                        {
                            "function" => JsonSerializer.Deserialize<OpenAiChatToolChoiceFunctionDto>(json, options)
                                ?? throw new JsonException("Cannot deserialize 'tool_choice' as function tool. Deserialization returned null."),
                            "allowed_tools" => JsonSerializer.Deserialize<OpenAiChatToolChoiceAllowedDto>(json, options)
                                ?? throw new JsonException("Cannot deserialize 'tool_choice' as allowed tools. Deserialization returned null."),
                            "custom" => JsonSerializer.Deserialize<OpenAiChatToolChoiceCustomDto>(json, options)
                                ?? throw new JsonException("Cannot deserialize 'tool_choice' as custom tool. Deserialization returned null."),
                            _ => throw new JsonException($"Cannot deserialize 'tool_choice' object. Unknown type: {typeValue}")
                        };
                    }

                // "tool_choice": null
                case JsonTokenType.Null:
                    return null;

                default:
                    throw new JsonException(
                        $"Cannot deserialize 'tool_choice'. Expected string, object, or null, but got {reader.TokenType}.");
            }
        }

        /// <summary>
        /// <see cref="OpenAiChatToolChoiceBaseDto"/> を JSON に書き込む。
        /// </summary>
        public override void Write(Utf8JsonWriter writer, OpenAiChatToolChoiceBaseDto value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case OpenAiChatToolChoiceStringDto stringValue:
                    writer.WriteStringValue(stringValue.Value);
                    break;

                case OpenAiChatToolChoiceFunctionDto functionToolValue:
                    JsonSerializer.Serialize(writer, functionToolValue, options);
                    break;

                case OpenAiChatToolChoiceAllowedDto allowedToolsValue:
                    JsonSerializer.Serialize(writer, allowedToolsValue, options);
                    break;

                case OpenAiChatToolChoiceCustomDto customValue:
                    JsonSerializer.Serialize(writer, customValue, options);
                    break;

                case null:
                    writer.WriteNullValue();
                    break;

                default:
                    throw new JsonException(
                        $"Cannot serialize 'tool_choice'. Unexpected type: {value.GetType().Name}.");
            }
        }
    }
}

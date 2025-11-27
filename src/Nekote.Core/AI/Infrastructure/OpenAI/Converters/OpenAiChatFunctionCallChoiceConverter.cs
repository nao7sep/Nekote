using System.Text.Json;
using System.Text.Json.Serialization;
using Nekote.Core.AI.Infrastructure.OpenAI.Dtos;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Converters
{
    /// <summary>
    /// 関数呼び出し選択のための JSON コンバーター（非推奨: tool_choice に置き換えられた）。
    /// 文字列またはオブジェクトを適切な型に変換する。
    /// </summary>
    [Obsolete("This class is deprecated. Use ToolChoice instead.")]
    public class OpenAiChatFunctionCallChoiceConverter : JsonConverter<OpenAiChatFunctionCallChoiceBaseDto>
    {
        /// <summary>
        /// JSON から <see cref="OpenAiChatFunctionCallChoiceBaseDto"/> を読み取る。
        /// </summary>
        public override OpenAiChatFunctionCallChoiceBaseDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return new OpenAiChatFunctionCallChoiceStringDto { Value = reader.GetString() };

                case JsonTokenType.StartObject:
                    return JsonSerializer.Deserialize<OpenAiChatFunctionCallChoiceObjectDto>(ref reader, options);

                case JsonTokenType.Null:
                    return null;

                default:
                    throw new JsonException(
                        $"Cannot deserialize 'function_call' choice. Expected string, object, or null, but got {reader.TokenType}.");
            }
        }

        /// <summary>
        /// <see cref="OpenAiChatFunctionCallChoiceBaseDto"/> を JSON に書き込む。
        /// </summary>
        public override void Write(Utf8JsonWriter writer, OpenAiChatFunctionCallChoiceBaseDto? value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case OpenAiChatFunctionCallChoiceStringDto stringValue:
                    writer.WriteStringValue(stringValue.Value);
                    break;

                case OpenAiChatFunctionCallChoiceObjectDto objectValue:
                    JsonSerializer.Serialize(writer, objectValue, options);
                    break;

                case null:
                    writer.WriteNullValue();
                    break;

                default:
                    throw new JsonException(
                        $"Cannot serialize 'function_call' choice. Unexpected type: {value.GetType().Name}.");
            }
        }
    }
}

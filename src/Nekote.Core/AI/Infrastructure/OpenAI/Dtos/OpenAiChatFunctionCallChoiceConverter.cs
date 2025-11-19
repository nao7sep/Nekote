using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 関数呼び出し選択のための JSON コンバーター (非推奨: tool_choice に置き換えられた)。
    /// 文字列またはオブジェクトを適切な型に変換する。
    /// </summary>
    public class OpenAiChatFunctionCallChoiceConverter : JsonConverter<OpenAiChatFunctionCallChoiceBaseDto>
    {
        public override OpenAiChatFunctionCallChoiceBaseDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return new OpenAiChatFunctionCallChoiceStringDto
                {
                    Value = reader.GetString()
                };
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                return JsonSerializer.Deserialize<OpenAiChatFunctionCallChoiceObjectDto>(ref reader, options);
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, OpenAiChatFunctionCallChoiceBaseDto value, JsonSerializerOptions options)
        {
            if (value is OpenAiChatFunctionCallChoiceStringDto stringValue)
            {
                writer.WriteStringValue(stringValue.Value);
            }
            else if (value is OpenAiChatFunctionCallChoiceObjectDto objectValue)
            {
                JsonSerializer.Serialize(writer, objectValue, options);
            }
        }
    }
}

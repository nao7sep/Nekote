using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ツール選択のための JSON コンバーター。
    /// 文字列またはオブジェクトを適切な型に変換する。
    /// </summary>
    public class OpenAiChatToolChoiceConverter : JsonConverter<OpenAiChatToolChoiceBaseDto>
    {
        public override OpenAiChatToolChoiceBaseDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return new OpenAiChatToolChoiceStringDto
                {
                    Value = reader.GetString()
                };
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                return JsonSerializer.Deserialize<OpenAiChatToolChoiceObjectDto>(ref reader, options);
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, OpenAiChatToolChoiceBaseDto value, JsonSerializerOptions options)
        {
            if (value is OpenAiChatToolChoiceStringDto stringValue)
            {
                writer.WriteStringValue(stringValue.Value);
            }
            else if (value is OpenAiChatToolChoiceObjectDto objectValue)
            {
                JsonSerializer.Serialize(writer, objectValue, options);
            }
        }
    }
}

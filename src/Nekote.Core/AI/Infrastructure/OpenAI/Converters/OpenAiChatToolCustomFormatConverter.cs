using System.Text.Json;
using System.Text.Json.Serialization;
using Nekote.Core.AI.Infrastructure.OpenAI.Dtos;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Converters
{
    /// <summary>
    /// OpenAI の "format" プロパティをデシリアライズするカスタム コンバーター。
    /// JSON の "type" フィールドに応じて、
    /// OpenAiChatToolCustomFormatBaseDto の適切な派生クラスをインスタンス化する。
    /// </summary>
    public class OpenAiChatToolCustomFormatConverter : JsonConverter<OpenAiChatToolCustomFormatBaseDto>
    {
        /// <summary>
        /// JSON から OpenAiChatToolCustomFormatBaseDto を読み取る。
        /// </summary>
        public override OpenAiChatToolCustomFormatBaseDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                if (!root.TryGetProperty("type", out var typeProperty))
                {
                    throw new JsonException("Cannot deserialize 'format'. Missing 'type' property.");
                }

                var typeValue = typeProperty.GetString();
                var json = root.GetRawText();

                switch (typeValue)
                {
                    case "text":
                        return JsonSerializer.Deserialize<OpenAiChatToolCustomFormatTextDto>(json, options);

                    case "grammar":
                        return JsonSerializer.Deserialize<OpenAiChatToolCustomFormatGrammarDto>(json, options);

                    default:
                        throw new JsonException($"Cannot deserialize 'format'. Unknown type: {typeValue}");
                }
            }
        }

        /// <summary>
        /// OpenAiChatToolCustomFormatBaseDto を JSON に書き込む。
        /// </summary>
        public override void Write(Utf8JsonWriter writer, OpenAiChatToolCustomFormatBaseDto? value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case OpenAiChatToolCustomFormatTextDto textFormat:
                    JsonSerializer.Serialize(writer, textFormat, options);
                    break;

                case OpenAiChatToolCustomFormatGrammarDto grammarFormat:
                    JsonSerializer.Serialize(writer, grammarFormat, options);
                    break;

                case null:
                    writer.WriteNullValue();
                    break;

                default:
                    throw new JsonException(
                        $"Cannot serialize 'format'. Unexpected type: {value.GetType().Name}.");
            }
        }
    }
}

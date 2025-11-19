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
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException(
                    $"Cannot deserialize 'format'. Expected object or null, but got {reader.TokenType}.");
            }

            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = document.RootElement;

                if (root.TryGetProperty("type", out JsonElement typeElement))
                {
                    string? type = typeElement.GetString();

                    switch (type)
                    {
                        case "text":
                            return JsonSerializer.Deserialize<OpenAiChatToolCustomFormatTextDto>(root.GetRawText(), options);

                        case "grammar":
                            return JsonSerializer.Deserialize<OpenAiChatToolCustomFormatGrammarDto>(root.GetRawText(), options);

                        default:
                            throw new JsonException(
                                $"Cannot deserialize 'format'. Unknown type: '{type}'.");
                    }
                }

                throw new JsonException(
                    "Cannot deserialize 'format'. Missing 'type' property.");
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

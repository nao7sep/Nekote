using System.Text.Json;
using System.Text.Json.Serialization;
using Nekote.Core.AI.Infrastructure.OpenAI.Dtos;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Converters
{
    /// <summary>
    /// <see cref="OpenAiChatResponseFormatBaseDto"/> のポリモーフィック JSON コンバーター。
    /// "type" プロパティの値に基づいて適切な派生型にデシリアライズする。
    /// </summary>
    public class OpenAiChatResponseFormatConverter : JsonConverter<OpenAiChatResponseFormatBaseDto>
    {
        /// <summary>
        /// JSON から <see cref="OpenAiChatResponseFormatBaseDto"/> を読み取る。
        /// </summary>
        public override OpenAiChatResponseFormatBaseDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                if (!root.TryGetProperty("type", out var typeProperty))
                {
                    throw new JsonException("Missing required 'type' property in response format object.");
                }

                var typeValue = typeProperty.GetString();
                var json = root.GetRawText();

                switch (typeValue)
                {
                    case "text":
                        return JsonSerializer.Deserialize<OpenAiChatResponseFormatTextDto>(json, options);

                    case "json_schema":
                        return JsonSerializer.Deserialize<OpenAiChatResponseFormatJsonSchemaDto>(json, options);

                    case "json_object":
                        return JsonSerializer.Deserialize<OpenAiChatResponseFormatJsonObjectDto>(json, options);

                    default:
                        throw new JsonException($"Unknown response format type: {typeValue}");
                }
            }
        }

        /// <summary>
        /// <see cref="OpenAiChatResponseFormatBaseDto"/> を JSON に書き込む。
        /// </summary>
        public override void Write(Utf8JsonWriter writer, OpenAiChatResponseFormatBaseDto? value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case OpenAiChatResponseFormatTextDto textFormat:
                    JsonSerializer.Serialize(writer, textFormat, options);
                    break;

                case OpenAiChatResponseFormatJsonSchemaDto jsonSchemaFormat:
                    JsonSerializer.Serialize(writer, jsonSchemaFormat, options);
                    break;

                case OpenAiChatResponseFormatJsonObjectDto jsonObjectFormat:
                    JsonSerializer.Serialize(writer, jsonObjectFormat, options);
                    break;

                case null:
                    writer.WriteNullValue();
                    break;

                default:
                    throw new JsonException(
                        $"Cannot serialize 'response_format'. Unexpected type: {value.GetType().Name}.");
            }
        }
    }
}

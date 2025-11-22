using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Converters
{
    /// <summary>
    /// OpenAiChatResponseFormatBaseDto のポリモーフィック JSON コンバーター。
    /// </summary>
    /// <remarks>
    /// "type" プロパティの値に基づいて適切な派生型にデシリアライズする。
    /// </remarks>
    public class OpenAiChatResponseFormatConverter : JsonConverter<Dtos.OpenAiChatResponseFormatBaseDto>
    {
        public override Dtos.OpenAiChatResponseFormatBaseDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                        return JsonSerializer.Deserialize<Dtos.OpenAiChatResponseFormatTextDto>(json, options);

                    case "json_schema":
                        return JsonSerializer.Deserialize<Dtos.OpenAiChatResponseFormatJsonSchemaDto>(json, options);

                    case "json_object":
                        return JsonSerializer.Deserialize<Dtos.OpenAiChatResponseFormatJsonObjectDto>(json, options);

                    default:
                        throw new JsonException($"Unknown response format type: {typeValue}");
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, Dtos.OpenAiChatResponseFormatBaseDto value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case Dtos.OpenAiChatResponseFormatTextDto textFormat:
                    JsonSerializer.Serialize(writer, textFormat, options);
                    break;

                case Dtos.OpenAiChatResponseFormatJsonSchemaDto jsonSchemaFormat:
                    JsonSerializer.Serialize(writer, jsonSchemaFormat, options);
                    break;

                case Dtos.OpenAiChatResponseFormatJsonObjectDto jsonObjectFormat:
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

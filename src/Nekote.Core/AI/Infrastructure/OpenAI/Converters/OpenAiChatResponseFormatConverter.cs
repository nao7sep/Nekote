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
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            if (!root.TryGetProperty("type", out var typeProperty))
            {
                throw new JsonException("Missing required 'type' property in response format object.");
            }

            var type = typeProperty.GetString();

            return type switch
            {
                "text" => JsonSerializer.Deserialize<Dtos.OpenAiChatResponseFormatTextDto>(root.GetRawText(), options),
                "json_schema" => JsonSerializer.Deserialize<Dtos.OpenAiChatResponseFormatJsonSchemaDto>(root.GetRawText(), options),
                "json_object" => JsonSerializer.Deserialize<Dtos.OpenAiChatResponseFormatJsonObjectDto>(root.GetRawText(), options),
                _ => throw new JsonException($"Unknown response format type: {type}")
            };
        }

        public override void Write(Utf8JsonWriter writer, Dtos.OpenAiChatResponseFormatBaseDto value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;
using Nekote.Core.AI.Infrastructure.OpenAI.Dtos;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Converters
{
    /// <summary>
    /// OpenAI の "tools" 配列要素をデシリアライズするカスタム コンバーター。
    /// JSON の "type" プロパティに応じて、
    /// <see cref="OpenAiChatToolBaseDto"/> の適切な派生クラスをインスタンス化する。
    /// </summary>
    public class OpenAiChatToolConverter : JsonConverter<OpenAiChatToolBaseDto>
    {
        /// <summary>
        /// JSON から <see cref="OpenAiChatToolBaseDto"/> を読み取る。
        /// </summary>
        public override OpenAiChatToolBaseDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                if (!root.TryGetProperty("type", out var typeProperty))
                {
                    throw new JsonException("Cannot deserialize 'tools' element. Missing 'type' property.");
                }

                var typeValue = typeProperty.GetString();
                var json = root.GetRawText();

                switch (typeValue)
                {
                    case "function":
                        return JsonSerializer.Deserialize<OpenAiChatToolFunctionDto>(json, options);

                    case "custom":
                        return JsonSerializer.Deserialize<OpenAiChatToolCustomDto>(json, options);

                    default:
                        throw new JsonException($"Cannot deserialize tool. Unknown type: {typeValue}");
                }
            }
        }

        /// <summary>
        /// <see cref="OpenAiChatToolBaseDto"/> を JSON に書き込む。
        /// </summary>
        public override void Write(Utf8JsonWriter writer, OpenAiChatToolBaseDto? value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case OpenAiChatToolFunctionDto functionTool:
                    JsonSerializer.Serialize(writer, functionTool, options);
                    break;

                case OpenAiChatToolCustomDto customTool:
                    JsonSerializer.Serialize(writer, customTool, options);
                    break;

                case null:
                    writer.WriteNullValue();
                    break;

                default:
                    throw new JsonException(
                        $"Cannot serialize 'tools' element. Unexpected type: {value.GetType().Name}.");
            }
        }
    }
}

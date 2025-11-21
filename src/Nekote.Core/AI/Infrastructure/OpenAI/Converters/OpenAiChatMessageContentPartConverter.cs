using System.Text.Json;
using System.Text.Json.Serialization;
using Nekote.Core.AI.Infrastructure.OpenAI.Dtos;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Converters
{
    /// <summary>
    /// OpenAI の "content" 配列内の "part" 要素をデシリアライズするカスタム コンバーター。
    /// JSON の "type" プロパティに応じて、
    /// <see cref="OpenAiChatMessageContentPartBaseDto"/> の適切な派生クラスをインスタンス化する。
    /// </summary>
    public class OpenAiChatMessageContentPartConverter : JsonConverter<OpenAiChatMessageContentPartBaseDto>
    {
        /// <summary>
        /// JSON から <see cref="OpenAiChatMessageContentPartBaseDto"/> を読み取る。
        /// </summary>
        public override OpenAiChatMessageContentPartBaseDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                if (!root.TryGetProperty("type", out var typeProperty))
                {
                    throw new JsonException("Cannot deserialize content part. Missing 'type' property.");
                }

                var typeValue = typeProperty.GetString();
                var json = root.GetRawText();

                switch (typeValue)
                {
                    case "text":
                        return JsonSerializer.Deserialize<OpenAiChatMessageContentPartTextDto>(json, options);

                    case "image_url":
                        return JsonSerializer.Deserialize<OpenAiChatMessageContentPartImageUrlDto>(json, options);

                    case "input_audio":
                        return JsonSerializer.Deserialize<OpenAiChatMessageContentPartInputAudioDto>(json, options);

                    case "file":
                        return JsonSerializer.Deserialize<OpenAiChatMessageContentPartFileDto>(json, options);

                    case "refusal":
                        return JsonSerializer.Deserialize<OpenAiChatMessageContentPartRefusalDto>(json, options);

                    default:
                        throw new JsonException($"Cannot deserialize content part. Unknown type: {typeValue}");
                }
            }
        }

        /// <summary>
        /// <see cref="OpenAiChatMessageContentPartBaseDto"/> を JSON に書き込む。
        /// </summary>
        public override void Write(Utf8JsonWriter writer, OpenAiChatMessageContentPartBaseDto? value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case OpenAiChatMessageContentPartTextDto textPart:
                    JsonSerializer.Serialize(writer, textPart, options);
                    break;

                case OpenAiChatMessageContentPartImageUrlDto imageUrlPart:
                    JsonSerializer.Serialize(writer, imageUrlPart, options);
                    break;

                case OpenAiChatMessageContentPartInputAudioDto inputAudioPart:
                    JsonSerializer.Serialize(writer, inputAudioPart, options);
                    break;

                case OpenAiChatMessageContentPartFileDto filePart:
                    JsonSerializer.Serialize(writer, filePart, options);
                    break;

                case OpenAiChatMessageContentPartRefusalDto refusalPart:
                    JsonSerializer.Serialize(writer, refusalPart, options);
                    break;

                case null:
                    writer.WriteNullValue();
                    break;

                default:
                    throw new JsonException(
                        $"Cannot serialize content part. Unexpected type: {value.GetType().Name}.");
            }
        }
    }
}

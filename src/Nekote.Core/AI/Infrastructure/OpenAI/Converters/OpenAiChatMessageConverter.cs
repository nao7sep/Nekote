using System.Text.Json;
using System.Text.Json.Serialization;
using Nekote.Core.AI.Infrastructure.OpenAI.Dtos;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Converters
{
    /// <summary>
    /// OpenAI の "messages" 配列要素をデシリアライズするカスタム コンバーター。
    /// JSON の "role" プロパティに応じて、
    /// <see cref="OpenAiChatMessageBaseDto"/> の適切な派生クラスをインスタンス化する。
    /// </summary>
    public class OpenAiChatMessageConverter : JsonConverter<OpenAiChatMessageBaseDto>
    {
        /// <summary>
        /// JSON から <see cref="OpenAiChatMessageBaseDto"/> を読み取る。
        /// </summary>
        public override OpenAiChatMessageBaseDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                if (!root.TryGetProperty("role", out var roleProperty))
                {
                    throw new JsonException("Cannot deserialize 'messages' element. Missing 'role' property.");
                }

                var roleValue = roleProperty.GetString();
                var json = root.GetRawText();

                switch (roleValue)
                {
                    case "system":
                        return JsonSerializer.Deserialize<OpenAiChatMessageSystemDto>(json, options);

                    case "developer":
                        return JsonSerializer.Deserialize<OpenAiChatMessageDeveloperDto>(json, options);

                    case "user":
                        return JsonSerializer.Deserialize<OpenAiChatMessageUserDto>(json, options);

                    case "assistant":
                        return JsonSerializer.Deserialize<OpenAiChatMessageAssistantDto>(json, options);

                    case "tool":
                        return JsonSerializer.Deserialize<OpenAiChatMessageToolDto>(json, options);

                    case "function":
#pragma warning disable CS0618 // 廃止された型の使用に関する警告を抑制します。
                        return JsonSerializer.Deserialize<OpenAiChatMessageFunctionDto>(json, options);
#pragma warning restore CS0618

                    default:
                        throw new JsonException($"Cannot deserialize message. Unknown role: {roleValue}");
                }
            }
        }

        /// <summary>
        /// <see cref="OpenAiChatMessageBaseDto"/> を JSON に書き込む。
        /// </summary>
        public override void Write(Utf8JsonWriter writer, OpenAiChatMessageBaseDto? value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case OpenAiChatMessageSystemDto systemMessage:
                    JsonSerializer.Serialize(writer, systemMessage, options);
                    break;

                case OpenAiChatMessageDeveloperDto developerMessage:
                    JsonSerializer.Serialize(writer, developerMessage, options);
                    break;

                case OpenAiChatMessageUserDto userMessage:
                    JsonSerializer.Serialize(writer, userMessage, options);
                    break;

                case OpenAiChatMessageAssistantDto assistantMessage:
                    JsonSerializer.Serialize(writer, assistantMessage, options);
                    break;

                case OpenAiChatMessageToolDto toolMessage:
                    JsonSerializer.Serialize(writer, toolMessage, options);
                    break;

#pragma warning disable CS0618 // 廃止された型の使用に関する警告を抑制します。
                case OpenAiChatMessageFunctionDto functionMessage:
#pragma warning restore CS0618
                    JsonSerializer.Serialize(writer, functionMessage, options);
                    break;

                case null:
                    writer.WriteNullValue();
                    break;

                default:
                    throw new JsonException(
                        $"Cannot serialize 'messages' element. Unexpected type: {value.GetType().Name}.");
            }
        }
    }
}

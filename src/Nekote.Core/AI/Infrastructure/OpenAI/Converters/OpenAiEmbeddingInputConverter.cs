using System.Text.Json;
using System.Text.Json.Serialization;
using Nekote.Core.AI.Infrastructure.OpenAI.Dtos;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Converters
{
    /// <summary>
    /// OpenAI の "input" プロパティをシリアライズ/デシリアライズするカスタム コンバーター。
    /// JSON の型 (string, array) に応じて、
    /// <see cref="OpenAiEmbeddingInputBaseDto"/> の適切な派生クラスをインスタンス化する。
    /// </summary>
    public class OpenAiEmbeddingInputConverter : JsonConverter<OpenAiEmbeddingInputBaseDto>
    {
        /// <summary>
        /// JSON から <see cref="OpenAiEmbeddingInputBaseDto"/> を読み取る。
        /// </summary>
        public override OpenAiEmbeddingInputBaseDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                // "input": "Hello world"
                case JsonTokenType.String:
                    return new OpenAiEmbeddingInputStringDto { Text = reader.GetString() };

                // "input": ["text1", "text2"] or [[token1, token2], [token3]]
                case JsonTokenType.StartArray:
                    using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
                    {
                        JsonElement root = doc.RootElement;
                        return ParseArrayInput(root);
                    }

                // "input": null
                case JsonTokenType.Null:
                    return null;

                default:
                    throw new JsonException(
                        $"Cannot deserialize 'input'. Expected string, array, or null, but got {reader.TokenType}.");
            }
        }

        /// <summary>
        /// 配列形式の input を解析する。
        /// </summary>
        /// <remarks>
        /// このメソッドは JsonElement から DTO インスタンスを直接構築するため、
        /// 常に null でない値を返し、戻り値の型は null 非許容型となる。
        /// </remarks>
        private static OpenAiEmbeddingInputBaseDto ParseArrayInput(JsonElement root)
        {
            if (root.GetArrayLength() == 0)
            {
                // 空配列は文字列配列として扱われる
                return new OpenAiEmbeddingInputStringArrayDto { Texts = new List<string>() };
            }

            JsonElement firstElement = root[0];

            // 最初の要素が文字列 → string[]
            if (firstElement.ValueKind == JsonValueKind.String)
            {
                var texts = new List<string>();
                foreach (JsonElement element in root.EnumerateArray())
                {
                    // 配列内の個別要素が null の場合は例外をスローする。
                    // 配列自体が null であれば防御的プログラミングで許容するが、
                    // 配列内の要素が null であることは異常であり、データが破損している可能性が高い。
                    // List<string?> にする必要はなく、このような状況は例外として扱う。
                    string text = element.GetString() ?? throw new JsonException(
                        $"Cannot deserialize 'input' string array. Expected all elements to be strings, but got null or non-string value.");
                    texts.Add(text);
                }
                return new OpenAiEmbeddingInputStringArrayDto { Texts = texts };
            }
            // 最初の要素が配列 → int[][]
            else if (firstElement.ValueKind == JsonValueKind.Array)
            {
                var tokenArrays = new List<int[]>();
                foreach (JsonElement arrayElement in root.EnumerateArray())
                {
                    var tokens = new List<int>();
                    foreach (JsonElement token in arrayElement.EnumerateArray())
                    {
                        tokens.Add(token.GetInt32());
                    }
                    tokenArrays.Add(tokens.ToArray());
                }
                return new OpenAiEmbeddingInputTokenArrayDto { TokenArrays = tokenArrays };
            }
            else
            {
                throw new JsonException(
                    $"Cannot deserialize 'input' array. Expected array of strings or array of token arrays, but got array of {firstElement.ValueKind}.");
            }
        }

        /// <summary>
        /// <see cref="OpenAiEmbeddingInputBaseDto"/> を JSON に書き込む。
        /// </summary>
        public override void Write(Utf8JsonWriter writer, OpenAiEmbeddingInputBaseDto? value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case OpenAiEmbeddingInputStringDto stringInput:
                    writer.WriteStringValue(stringInput.Text);
                    break;

                case OpenAiEmbeddingInputStringArrayDto stringArrayInput:
                    writer.WriteStartArray();
                    if (stringArrayInput.Texts != null)
                    {
                        foreach (string text in stringArrayInput.Texts)
                        {
                            writer.WriteStringValue(text);
                        }
                    }
                    writer.WriteEndArray();
                    break;

                case OpenAiEmbeddingInputTokenArrayDto tokenArrayInput:
                    writer.WriteStartArray();
                    if (tokenArrayInput.TokenArrays != null)
                    {
                        foreach (int[] tokens in tokenArrayInput.TokenArrays)
                        {
                            writer.WriteStartArray();
                            foreach (int token in tokens)
                            {
                                writer.WriteNumberValue(token);
                            }
                            writer.WriteEndArray();
                        }
                    }
                    writer.WriteEndArray();
                    break;

                case null:
                    writer.WriteNullValue();
                    break;

                default:
                    throw new JsonException(
                        $"Cannot serialize 'input'. Unexpected type: {value.GetType().Name}.");
            }
        }
    }
}

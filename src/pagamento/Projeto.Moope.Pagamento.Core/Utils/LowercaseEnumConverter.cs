using System.Text.Json;
using System.Text.Json.Serialization;

namespace Projeto.Moope.Pagamento.Core.Utils
{
    public class LowercaseEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string stringValue = reader.GetString() ?? string.Empty;
                if (Enum.TryParse<T>(stringValue, true, out T result))
                {
                    return result;
                }
            }

            throw new JsonException($"Não foi possível converter '{reader.GetString()}' para {typeof(T).Name}");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString().ToLowerInvariant());
        }
    }
}

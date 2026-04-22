using System.Text.Json;
using System.Text.Json.Serialization;

namespace Projeto.Moope.Pagamento.Core.Utils
{
    public sealed class FlexibleStringListConverter : JsonConverter<List<string>>
    {
        public override List<string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            var list = new List<string>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                switch (reader.TokenType)
                {
                    case JsonTokenType.String:
                        list.Add(reader.GetString()!);
                        break;
                    case JsonTokenType.Number:
                        list.Add(reader.GetInt64().ToString());
                        break;
                    case JsonTokenType.Null:
                        list.Add(null);
                        break;
                    default:
                        throw new JsonException($"Elemento inválido no array de phones/emails: {reader.TokenType}");
                }
            }

            return list;
        }

        public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var s in value)
                writer.WriteStringValue(s);
            writer.WriteEndArray();
        }
    }
}

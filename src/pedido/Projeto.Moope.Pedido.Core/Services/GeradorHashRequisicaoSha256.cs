using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Projeto.Moope.Pedido.Core.Interfaces.Services;

namespace Projeto.Moope.Pedido.Core.Services
{
    public class GeradorHashRequisicaoSha256 : IGeradorHashRequisicao
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public string GerarHash(object requisicao)
        {
            if (requisicao == null) throw new ArgumentNullException(nameof(requisicao));

            var json = JsonSerializer.Serialize(requisicao, JsonOptions);
            using var doc = JsonDocument.Parse(json);
            var canonico = CanonicalizarJson(doc.RootElement);

            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonico));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private static string CanonicalizarJson(JsonElement elemento)
        {
            return elemento.ValueKind switch
            {
                JsonValueKind.Object => CanonicalizarObjeto(elemento),
                JsonValueKind.Array => CanonicalizarArray(elemento),
                _ => elemento.GetRawText()
            };
        }

        private static string CanonicalizarObjeto(JsonElement objeto)
        {
            var props = objeto.EnumerateObject()
                .OrderBy(p => p.Name, StringComparer.Ordinal);

            var sb = new StringBuilder();
            sb.Append('{');

            var primeiro = true;
            foreach (var prop in props)
            {
                if (!primeiro) sb.Append(',');
                primeiro = false;

                sb.Append(JsonSerializer.Serialize(prop.Name, JsonOptions));
                sb.Append(':');
                sb.Append(CanonicalizarJson(prop.Value));
            }

            sb.Append('}');
            return sb.ToString();
        }

        private static string CanonicalizarArray(JsonElement array)
        {
            var sb = new StringBuilder();
            sb.Append('[');

            var primeiro = true;
            foreach (var item in array.EnumerateArray())
            {
                if (!primeiro) sb.Append(',');
                primeiro = false;

                sb.Append(CanonicalizarJson(item));
            }

            sb.Append(']');
            return sb.ToString();
        }
    }
}


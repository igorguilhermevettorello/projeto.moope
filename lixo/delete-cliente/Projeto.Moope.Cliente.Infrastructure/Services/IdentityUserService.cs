using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Projeto.Moope.Cliente.Core.Interfaces.Services;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Cliente.Infrastructure.Services
{
    public class IdentityUserService : IIdentityUserService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _authBaseUrl;

        public IdentityUserService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _authBaseUrl = configuration.GetValue<string>("Jwt:Authority")?.TrimEnd('/')
                ?? throw new InvalidOperationException("A configura??o 'Jwt:Authority' ? obrigat?ria para integra??o com BC Auth.");
        }

        public async Task<ResultUser<Guid>> CriarUsuarioAsync(string email, string senha, string? telefone = null, TipoUsuario tipoUsuario = TipoUsuario.Cliente)
        {
            var payload = new
            {
                nome = email,
                email,
                cpfCnpj = string.Empty,
                telefone = telefone ?? string.Empty,
                tipoPessoa = TipoPessoa.FISICA,
                tipoUsuario = tipoUsuario,
                senha,
                confirmacao = senha,
                nomeFantasia = string.Empty,
                inscricaoEstadual = string.Empty
            };

            var response = await SendAsync(HttpMethod.Post, "/api/usuario", payload);
            if (!response.IsSuccessStatusCode)
            {
                return new ResultUser<Guid>
                {
                    Status = false,
                    Mensagem = await BuildErrorMessageAsync(response, "Falha ao criar usu?rio no BC Auth")
                };
            }

            var usuarioId = await ReadGuidAsync(response);
            if (usuarioId == Guid.Empty)
            {
                return new ResultUser<Guid>
                {
                    Status = false,
                    Mensagem = "BC Auth n?o retornou um ID de usu?rio v?lido."
                };
            }

            return new ResultUser<Guid>
            {
                Status = true,
                Dados = usuarioId
            };
        }

        public async Task<Result> RemoverAoFalharAsync(Guid usuarioId)
        {
            var response = await SendAsync(HttpMethod.Delete, $"/api/usuario/{usuarioId}");
            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new Result { Status = true };

            return new Result
            {
                Status = false,
                Mensagem = await BuildErrorMessageAsync(response, "Falha ao remover usu?rio no BC Auth")
            };
        }

        public async Task<Result<Guid>> AlterarUsuarioAsync(Guid userId, string email, string? telefone = null)
        {
            var payload = new
            {
                id = userId,
                nome = email,
                email,
                cpfCnpj = string.Empty,
                telefone = telefone ?? string.Empty,
                tipoPessoa = TipoPessoa.FISICA,
                nomeFantasia = string.Empty,
                inscricaoEstadual = string.Empty,
                vendedorId = (Guid?)null
            };

            var response = await SendAsync(HttpMethod.Put, $"/api/usuario/{userId}", payload);
            if (!response.IsSuccessStatusCode)
            {
                return new Result<Guid>
                {
                    Status = false,
                    Mensagem = await BuildErrorMessageAsync(response, "Falha ao atualizar usu?rio no BC Auth")
                };
            }

            return new Result<Guid> { Status = true, Dados = userId };
        }

        public async Task<Guid?> BuscarPorEmailAsync(string email)
        {
            var response = await SendAsync(HttpMethod.Get, "/api/usuario");
            if (!response.IsSuccessStatusCode)
                return null;

            var usuarios = await response.Content.ReadFromJsonAsync<List<UsuarioListagemDto>>(JsonOptions) ?? new List<UsuarioListagemDto>();
            return usuarios.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase))?.Id;
        }

        public async Task<Result> AlterarSenhaAsync(Guid usuarioId, string senhaAtual, string novaSenha)
        {
            var payload = new
            {
                usuarioId,
                senhaAtual,
                novaSenha,
                confirmacaoNovaSenha = novaSenha
            };

            var response = await SendAsync(HttpMethod.Post, "/api/usuario/alterar-senha", payload);
            if (response.IsSuccessStatusCode)
                return new Result { Status = true, Mensagem = "Senha alterada com sucesso" };

            return new Result
            {
                Status = false,
                Mensagem = await BuildErrorMessageAsync(response, "Falha ao alterar senha no BC Auth")
            };
        }

        private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? payload = null)
        {
            var request = new HttpRequestMessage(method, $"{_authBaseUrl}{path}");
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrWhiteSpace(token) && AuthenticationHeaderValue.TryParse(token, out var authHeader))
                request.Headers.Authorization = authHeader;

            if (payload != null)
                request.Content = JsonContent.Create(payload);

            var client = _httpClientFactory.CreateClient(nameof(IdentityUserService));
            return await client.SendAsync(request);
        }

        private static async Task<Guid> ReadGuidAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (Guid.TryParse(content.Trim('"'), out var asGuid))
                return asGuid;

            try
            {
                var result = JsonSerializer.Deserialize<Result<Guid>>(content, JsonOptions);
                return result?.Dados ?? Guid.Empty;
            }
            catch
            {
                return Guid.Empty;
            }
        }

        private static async Task<string> BuildErrorMessageAsync(HttpResponseMessage response, string fallback)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
                return $"{fallback}. StatusCode: {(int)response.StatusCode}.";

            return $"{fallback}. StatusCode: {(int)response.StatusCode}. Detalhes: {content}";
        }

        private sealed class UsuarioListagemDto
        {
            public Guid Id { get; set; }
            public string Email { get; set; } = string.Empty;
        }
    }
}

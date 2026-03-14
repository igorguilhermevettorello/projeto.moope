using System.Net.Mail;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Email.Core.Interfaces.Services;
using Projeto.Moope.Email.Infrastructure.Configurations;

namespace Projeto.Moope.Email.Infrastructure.Services
{
    /// <summary>
    /// Serviço de envio de email via requisição HTTP a endpoint externo.
    /// Implementa IEmailGateway na camada Infrastructure para respeitar SOLID (DIP).
    /// </summary>
    public class EmailGateway : IEmailGateway
    {
        private readonly ILogger<EmailGateway> _logger;
        private readonly EmailSettings _emailSettings;
        private readonly HttpClient _httpClient;
        private readonly INotificador _notificador;

        public EmailGateway(
            ILogger<EmailGateway> logger,
            IOptions<EmailSettings> emailSettings,
            HttpClient httpClient,
            INotificador notificador)
        {
            _logger = logger;
            _emailSettings = emailSettings.Value;
            _httpClient = httpClient;
            _notificador = notificador;
        }

        public async Task<ResultadoEnvioEmail> EnviarAsync(EnvioEmailRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                {
                    _notificador.Handle(new Projeto.Moope.Core.Notifications.Notificacao
                    {
                        Campo = "Email",
                        Mensagem = "Request de email não pode ser nulo"
                    });
                    return new ResultadoEnvioEmail(false, MensagemErro: "Request de email não pode ser nulo");
                }

                if (!ValidarEmail(request))
                {
                    return new ResultadoEnvioEmail(false, MensagemErro: "Dados do email inválidos");
                }

                if (string.IsNullOrEmpty(_emailSettings.ApiKey))
                {
                    _logger.LogError("API_KEY não configurada para envio de email");
                    _notificador.Handle(new Projeto.Moope.Core.Notifications.Notificacao
                    {
                        Campo = "Email",
                        Mensagem = "API_KEY não configurada para envio de email"
                    });
                    return new ResultadoEnvioEmail(false, MensagemErro: "API_KEY não configurada para envio de email");
                }

                var remetente = !string.IsNullOrEmpty(request.Remetente)
                    ? request.Remetente
                    : _emailSettings.RemetenteEmail;

                var emailApiRequest = new EmailApiRequestDto
                {
                    Subject = request.Assunto,
                    Body = request.Corpo,
                    From = remetente,
                    To = request.Destinatario,
                    Cc = !string.IsNullOrEmpty(request.Copia) ? request.Copia : null,
                    Bcc = !string.IsNullOrEmpty(request.CopiaOculta) ? request.CopiaOculta : null
                };

                var jsonContent = JsonSerializer.Serialize(emailApiRequest, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, ObterUrlEnvio())
                {
                    Content = content
                };
                requestMessage.Headers.Add("x-api-key", _emailSettings.ApiKey);

                _httpClient.Timeout = TimeSpan.FromSeconds(_emailSettings.TimeoutSeconds);

                var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogInformation("Email enviado com sucesso via API para {Destinatario}", request.Destinatario);

                    try
                    {
                        var successResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        var status = successResponse.TryGetProperty("status", out var statusProperty) ? statusProperty.GetString() : "success";
                        var location = successResponse.TryGetProperty("location", out var locationProperty) ? locationProperty.GetString() : null;

                        var mensagemSucesso = $"Email enviado com sucesso. Status: {status}" +
                            (!string.IsNullOrEmpty(location) ? $", Location: {location}" : "");

                        return new ResultadoEnvioEmail(true, MensagemSucesso: mensagemSucesso);
                    }
                    catch (JsonException)
                    {
                        return new ResultadoEnvioEmail(true, MensagemSucesso: "Email enviado com sucesso");
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Falha no envio de email via API. Status: {StatusCode}, Erro: {ErrorContent}",
                    response.StatusCode, errorContent);

                try
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonElement>(errorContent);
                    var error = errorResponse.TryGetProperty("error", out var errorProperty) ? errorProperty.GetString() : "Erro desconhecido";
                    var detalhes = errorResponse.TryGetProperty("detalhes", out var detalhesProperty) ? detalhesProperty.GetString() : null;

                    var mensagemErro = $"Falha no envio via API: {error}";
                    if (!string.IsNullOrEmpty(detalhes))
                        mensagemErro += $". Detalhes: {detalhes}";

                    _notificador.Handle(new Projeto.Moope.Core.Notifications.Notificacao { Campo = "Email", Mensagem = mensagemErro });
                    return new ResultadoEnvioEmail(false, MensagemErro: mensagemErro);
                }
                catch (JsonException)
                {
                    var mensagemErro = $"Falha no envio via API: {response.StatusCode} - {errorContent}";
                    _notificador.Handle(new Projeto.Moope.Core.Notifications.Notificacao { Campo = "Email", Mensagem = mensagemErro });
                    return new ResultadoEnvioEmail(false, MensagemErro: mensagemErro);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexão HTTP ao enviar email para {Destinatario}", request?.Destinatario);
                var mensagemErro = $"Erro de conexão: {ex.Message}";
                _notificador.Handle(new Projeto.Moope.Core.Notifications.Notificacao { Campo = "Email", Mensagem = mensagemErro });
                return new ResultadoEnvioEmail(false, MensagemErro: mensagemErro);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout ao enviar email para {Destinatario}", request?.Destinatario);
                var mensagemErro = "Timeout no envio de email";
                _notificador.Handle(new Projeto.Moope.Core.Notifications.Notificacao { Campo = "Email", Mensagem = mensagemErro });
                return new ResultadoEnvioEmail(false, MensagemErro: mensagemErro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado no envio de email para {Destinatario}", request?.Destinatario);
                var mensagemErro = $"Erro inesperado: {ex.Message}";
                _notificador.Handle(new Projeto.Moope.Core.Notifications.Notificacao { Campo = "Email", Mensagem = mensagemErro });
                return new ResultadoEnvioEmail(false, MensagemErro: mensagemErro);
            }
        }

        private string ObterUrlEnvio()
        {
            var baseUrl = _emailSettings.ApiUrl.TrimEnd('/');
            var endpoint = _emailSettings.EndpointEnvio.TrimStart('/');
            return $"{baseUrl}/{endpoint}";
        }

        private bool ValidarEmail(EnvioEmailRequest request)
        {
            if (string.IsNullOrEmpty(request.Destinatario))
            {
                _notificador.Handle(new Projeto.Moope.Core.Notifications.Notificacao { Campo = "Destinatario", Mensagem = "Destinatário é obrigatório" });
                return false;
            }

            if (string.IsNullOrEmpty(request.Assunto))
            {
                _notificador.Handle(new Projeto.Moope.Core.Notifications.Notificacao { Campo = "Assunto", Mensagem = "Assunto é obrigatório" });
                return false;
            }

            if (string.IsNullOrEmpty(request.Corpo))
            {
                _notificador.Handle(new Projeto.Moope.Core.Notifications.Notificacao { Campo = "Corpo", Mensagem = "Corpo do email é obrigatório" });
                return false;
            }

            if (!IsValidEmail(request.Destinatario))
            {
                _notificador.Handle(new Projeto.Moope.Core.Notifications.Notificacao { Campo = "Destinatario", Mensagem = "Email do destinatário não é válido" });
                return false;
            }

            if (!string.IsNullOrEmpty(request.Remetente) && !IsValidEmail(request.Remetente))
            {
                _notificador.Handle(new Projeto.Moope.Core.Notifications.Notificacao { Campo = "Remetente", Mensagem = "Email do remetente não é válido" });
                return false;
            }

            return true;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// DTO interno para serialização da requisição à API externa.
        /// </summary>
        private class EmailApiRequestDto
        {
            public string Subject { get; set; } = string.Empty;
            public string Body { get; set; } = string.Empty;
            public string From { get; set; } = string.Empty;
            public string To { get; set; } = string.Empty;
            public string? Cc { get; set; }
            public string? Bcc { get; set; }
        }
    }
}

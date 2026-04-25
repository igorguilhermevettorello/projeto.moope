using Projeto.Moope.Pagamento.Core.Enums;

namespace Projeto.Moope.Pagamento.Core.DTOs.Idempotencia
{
    public class ResultadoInicioIdempotenciaDto
    {
        public bool DeveProcessar { get; init; }

        public bool JaConcluido { get; init; }

        public bool EmProcessamento { get; init; }

        public StatusIdempotencia Status { get; init; }

        public Guid IdempotenciaId { get; init; }

        public int? ResponseStatusCode { get; init; }

        public string? ResponseBody { get; init; }

        public string? ResourceId { get; init; }

        public string? ResourceType { get; init; }
    }
}


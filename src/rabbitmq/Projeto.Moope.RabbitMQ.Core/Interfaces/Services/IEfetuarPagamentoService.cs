using Projeto.Moope.Core.DTOs;
using Projeto.Moope.RabbitMQ.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto.Moope.RabbitMQ.Core.Interfaces.Services
{
    public interface IEfetuarPagamentoService
    {
        Task<ResultDto> EfetuarPagamento(PagamentoDto request, string? authorizationHeader, CancellationToken cancellationToken);
    }
}

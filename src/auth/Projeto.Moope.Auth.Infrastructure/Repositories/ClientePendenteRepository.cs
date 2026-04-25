using Microsoft.EntityFrameworkCore;
using Projeto.Moope.Auth.Core.DTOs.Usuario;
using Projeto.Moope.Auth.Core.Interfaces.Repositories;
using Projeto.Moope.Auth.Infrastructure.Data;
using Projeto.Moope.Auth.Infrastructure.ReadModels;
using Projeto.Moope.Core.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Projeto.Moope.Auth.Infrastructure.Repositories
{
    public class ClientePendenteRepository : IClientePendenteRepository
    {
        private readonly AppIdentityDbContext _context;

        public ClientePendenteRepository(AppIdentityDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<ClientePendenteDto>> ListarClientesPendentesAsync(CancellationToken cancellationToken = default)
        {
            var cliente = TipoUsuario.Cliente.ToString().ToUpper();
            const string sql = @"SELECT anu.Id,
                                        anu.Email
                                   FROM AspNetUsers anu
                                  INNER JOIN AspNetUserRoles anur 
                                          ON anur.UserId = anu.Id
                                  INNER JOIN AspNetRoles anr 
                                          ON anr.Id = anur.RoleId
                                  WHERE anr.NormalizedName = {0}
                                    AND NOT EXISTS (SELECT 1
                                                      FROM Cliente c
                                                     WHERE c.Id = anu.Id);";

            var rows = await _context.Set<ClientePendenteReadModel>()
                .FromSqlRaw(sql, cliente)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (rows.Count == 0)
            {
                return Array.Empty<ClientePendenteDto>();
            }

            return rows
                .Select(r => new ClientePendenteDto { Id = r.Id, Email = r.Email ?? string.Empty })
                .ToList();
        }
    }
}


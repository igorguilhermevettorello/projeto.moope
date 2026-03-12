using Projeto.Moope.Core.Interfaces.Data;
using Projeto.Moope.Core.Models;

namespace Projeto.Moope.Auth.Core.Models
{
    public class RefreshToken : Entity, IAggregateRoot
    {
        public Guid UsuarioId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByToken { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokedAt.HasValue;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto.Moope.Gateways.Core.DTOs.Cartao
{
    public class CartaoGalaxPayCreateDto
    {
        public int CustomerId { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Holder { get; set; } = string.Empty;
        public string ExpiresAt { get; set; } = string.Empty;
        public string Cvv { get; set; } = string.Empty;
    }
}

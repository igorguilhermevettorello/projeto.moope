using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto.Moope.Email.Core.Enums
{
    public enum Prioridade
    {
        /// <summary>
        /// Prioridade baixa
        /// </summary>
        Baixa = 1,

        /// <summary>
        /// Prioridade normal (padrão)
        /// </summary>
        Normal = 2,

        /// <summary>
        /// Prioridade alta
        /// </summary>
        Alta = 3,

        /// <summary>
        /// Prioridade urgente
        /// </summary>
        Urgente = 4
    }
}

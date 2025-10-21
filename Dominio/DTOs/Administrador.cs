using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace minimal_api.Dominio.DTOs
{
    public record AdministradorDTO
    {
        public string Email { get; set; } = default!;
        public string Senha { get; set; } = default!;

        public string Perfil { get; set; } = default!;
    }
}
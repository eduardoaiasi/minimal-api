using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.Entidades;

namespace minimal_api.Dominio.Interfaces
{
    public interface IVeiculoServico
    {
        List<Veiculo> Todos(int? page = 1, string? nome = null, string? modelo = null, int? ano = null);

        Veiculo? BuscarPorId(int id);

        void Incluir(Veiculo veiculo);

        void Remover(Veiculo veiculo);

        void Atualizar(Veiculo veiculo);
    }
}
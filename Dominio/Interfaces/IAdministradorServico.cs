using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalApi.Dominio.Entidades;
using MinimalApi.DTOs;

namespace MinimalApi.Infraestrutura.Interfaces
{
    public interface IAdministradorServico
    {
        Administrador? Login(LoginDTO loginDTO);

        Administrador? BuscarPorId(int id);

        void Incluir(Administrador administrador);
        void Atualizar(Administrador administrador);
        void Remover(Administrador administrador);
        List<Administrador>Todos(int? page = 1 ,string? Email = null, int? Senha = null, string? Perfil = null);
    }
}
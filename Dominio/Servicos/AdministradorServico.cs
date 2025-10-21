using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Entidades;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Interfaces;
using MnimalApi.Infraestrutura.DB;

namespace MinimalApi.Dominio.Servicos
{
    public class AdministradorServico : IAdministradorServico
    {
        private readonly DbContexto _contexto;
        public AdministradorServico(DbContexto contexto)
        {
            _contexto = contexto;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            var adm = _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
            return (adm);
        }

        public void Incluir(Administrador administrador)
        {
            _contexto.Administradores.Add(administrador);
            _contexto.SaveChanges();
        }

        public void Atualizar(Administrador administrador)
        {
            _contexto.Administradores.Update(administrador);
            _contexto.SaveChanges(); 
            
        }

        public void Remover(Administrador administrador)
        {
            _contexto.Administradores.Remove(administrador);
            _contexto.SaveChanges();
        }

        public List<Administrador> Todos(int? page = 1, string? email = null, int? senha = null, string? perfil = null)
        {
            var query = _contexto.Administradores.AsQueryable();
            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(a => EF.Functions.Like(a.Email.ToLower(), $"{email}"));
            }
            int itensPorPagina = 10;

            if (page != null)
            {
                query = query.Skip(((int)page - 1) * itensPorPagina).Take(itensPorPagina);
            }
            
            return query.ToList();
        }

        public Administrador? BuscarPorId(int id)
        {
            return _contexto.Administradores.Find(id);
        }
    }
}
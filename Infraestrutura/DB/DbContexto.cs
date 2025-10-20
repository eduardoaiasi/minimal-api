using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.Entidades;
using MinimalApi.Dominio.Entidades;

namespace MnimalApi.Infraestrutura.DB;

public class DbContexto : DbContext
{
   protected override void OnModelCreating(ModelBuilder modelBuilder) // Configura o modelo de dados
{
    base.OnModelCreating(modelBuilder); // Chama o método base para garantir a configuração padrão

        modelBuilder.Entity<Administrador>().HasData( // Seed de exemplo
            new Administrador
            {
                Id = 1, // ⚠️ Precisa ter ID definido!
                Email = "administrador@teste.com",
                Senha = "123456",
                Perfil = "Administrador"
            }
        );

        modelBuilder.Entity<Veiculo>().HasData( // Seed de exemplo
            new Veiculo
            {
                Id = 1,
                Nome = "Carro Exemplo",
                Marca = "Marca Exemplo",
                Ano = 2020
            }
        );
}

    public DbContexto(DbContextOptions<DbContexto> options) // Construtor que recebe as opções de configuração
        : base(options)
    {
    }

    public DbSet<Administrador> Administradores { get; set; } = default!; // Representa a tabela de administradores no banco de dados
    public DbSet<Veiculo> Veiculos { get; set; } = default!; // Representa a tabela de veículos no banco de dados
}

using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Entidades;

namespace MnimalApi.Infraestrutura.DB;

public class DbContexto : DbContext
{
   protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Administrador>().HasData(
        new Administrador
        {
            Id = 1, // ⚠️ Precisa ter ID definido!
            Email = "administrador@teste.com",
            Senha = "123456",
            Perfil = "Administrador"
        }
    );
}

    public DbContexto(DbContextOptions<DbContexto> options)
        : base(options)
    {
    }

    public DbSet<Administrador> Administradores { get; set; } = default!;
}

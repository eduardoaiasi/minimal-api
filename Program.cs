using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.Servicos;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Servicos;
using MinimalApi.Infraestrutura.Interfaces;
using MnimalApi.Infraestrutura.DB; // importante para reconhecer o DbContexto

#region builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();
builder.Services.AddScoped<IAdministradorServico, AdministradorServico>(); 

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContexto>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("mysql");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

var app = builder.Build();
#endregion builder

#region Home
app.MapGet("/", () =>
{
    var html = """
    <html>
        <head><title>Bem-vindo</title></head>
        <body style="font-family: Arial; margin: 40px;">
            <h1>Bem-vindo à Minimal API 🚀</h1>
            <p>Acesse a documentação clicando abaixo:</p>
            <a href="/swagger" style="font-size: 18px;">Abrir Swagger</a>
        </body>
    </html>
    """;

    return Results.Content(html, "text/html");
});
#endregion Home

#region Veiculo
ErrosValidacao validaDTO(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosValidacao
    {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(veiculoDTO.Nome))
    {
        validacao.Mensagens.Add("O nome não pode ser vazio");
    }
    if (string.IsNullOrEmpty(veiculoDTO.Marca))
    {
        validacao.Mensagens.Add("A marca não pode ser vazia");
    }
    if (veiculoDTO.Ano < 1900)
    {
        validacao.Mensagens.Add("O veiculo é muito antigo");
    }
    return validacao;
}

app.MapPost("/veiculos", ([FromBody]minimal_api.Dominio.DTOs.VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }
    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculoServico.Incluir(veiculo);
    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).WithTags("Veiculo");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);
    return Results.Ok(veiculos);
}).WithTags("Veiculo");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);

    if (veiculo == null) return Results.NotFound();

    return Results.Ok(veiculo);

}).WithTags("Veiculo");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);
    if (veiculo == null) return Results.NotFound();

    var validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);

    return Results.Ok(veiculo);
}).WithTags("Veiculo");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);
    if (veiculo == null) return Results.NotFound();

    veiculoServico.Remover(veiculo);

    return Results.NoContent();
}).WithTags("Veiculo");
#endregion Veiculo

#region Administrador

ErrosValidacao validaAdmDTO(AdministradorDTO administradorDTO)
{
    var validacao = new ErrosValidacao
    {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(administradorDTO.Email))
    {
        validacao.Mensagens.Add("O email não pode ser vazio!");
    }
    if (string.IsNullOrEmpty(administradorDTO.Senha))
    {
        validacao.Mensagens.Add("A senha não pode ser vazia");
    }   
    return validacao;
}

app.MapPost("/administrador/login", ([FromBody] MinimalApi.DTOs.LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    if (administradorServico.Login(loginDTO) != null)
        return Results.Ok("Login successful");
    else
        return Results.Unauthorized();
}).WithTags("Administrador");

app.MapPost("/administrador/cadastro", ([FromBody] minimal_api.Dominio.DTOs.AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
    var validacao = validaAdmDTO(administradorDTO);
    if(validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var administrador = new Administrador
    {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil
    };

    administradorServico.Incluir(administrador);

    // 🔹 Corrigido o caminho (tinha um erro de digitação em "administardor")
    return Results.Created($"/administrador/{administrador.Id}", administrador);
})
.WithTags("Administrador");

app.MapPut("/administrador/Atualizar/{id}", ([FromRoute] int id, AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
    var validacao = validaAdmDTO(administradorDTO);

    if(validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var administrador = administradorServico.BuscarPorId(id);
    if (administrador == null) return Results.NotFound();

    administrador.Email = administradorDTO.Email;
    administrador.Senha = administradorDTO.Senha;
    administrador.Perfil = administradorDTO.Perfil;
   
    administradorServico.Atualizar(administrador);
    
    return Results.Ok(administrador);
}).WithTags("Administrador");

app.MapGet("/administrador/pegar/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscarPorId(id);
    if (administrador == null) return Results.BadRequest();

    return Results.Ok(administrador);
}).WithTags("Administrador");

app.MapGet("/administrador/listar", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{
    var administradores = administradorServico.Todos(pagina);
    return Results.Ok(administradores);
}).WithTags("Administrador");

app.MapDelete("/administrador/delete/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscarPorId(id);
    if (administrador == null) return Results.BadRequest();

    administradorServico.Remover(administrador);

    return Results.Ok(administrador);
}).WithTags("Administrador");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
#endregion
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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

// ✅ Leitura correta da chave JWT
var key = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(key))
    key = "MinhaChaveSuperSecretaParaJwtComMaisDe32Caracteres123"; // fallback dev

// ✅ Configuração de autenticação JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ✅ Injeções de dependência
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();
builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();

// ✅ Swagger configurado com autenticação JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT aqui:"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ✅ Banco de dados
builder.Services.AddDbContext<DbContexto>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("MySql");
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
}).AllowAnonymous();
#endregion Home

#region Veiculo
ErrosValidacao ValidaDTO(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosValidacao { Mensagens = new List<string>() };

    if (string.IsNullOrEmpty(veiculoDTO.Nome))
        validacao.Mensagens.Add("O nome não pode ser vazio");

    if (string.IsNullOrEmpty(veiculoDTO.Marca))
        validacao.Mensagens.Add("A marca não pode ser vazia");

    if (veiculoDTO.Ano < 1900)
        validacao.Mensagens.Add("O veículo é muito antigo");

    return validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var validacao = ValidaDTO(veiculoDTO);
    if (validacao.Mensagens.Any())
        return Results.BadRequest(validacao);

    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };

    veiculoServico.Incluir(veiculo);
    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
.WithTags("Veículo");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);
    return Results.Ok(veiculos);
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Veículo");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);
    return veiculo is null ? Results.NotFound() : Results.Ok(veiculo);
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Veículo");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);
    if (veiculo == null) return Results.NotFound();

    var validacao = ValidaDTO(veiculoDTO);
    if (validacao.Mensagens.Any())
        return Results.BadRequest(validacao);

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);
    return Results.Ok(veiculo);
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Veículo");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);
    if (veiculo == null) return Results.NotFound();

    veiculoServico.Remover(veiculo);
    return Results.NoContent();
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Veículo");
#endregion Veiculo

#region Administrador
string GerarTokenJwt(Administrador administrador)
{
    if (string.IsNullOrEmpty(key)) return string.Empty;

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Email, administrador.Email),
        new Claim(ClaimTypes.Role, administrador.Perfil)
    };

    var token = new JwtSecurityToken(
        expires: DateTime.UtcNow.AddHours(2),
        claims: claims,
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

ErrosValidacao ValidaAdmDTO(AdministradorDTO administradorDTO)
{
    var validacao = new ErrosValidacao { Mensagens = new List<string>() };

    if (string.IsNullOrEmpty(administradorDTO.Email))
        validacao.Mensagens.Add("O email não pode ser vazio!");

    if (string.IsNullOrEmpty(administradorDTO.Senha))
        validacao.Mensagens.Add("A senha não pode ser vazia!");

    if (string.IsNullOrEmpty(administradorDTO.Perfil))
        validacao.Mensagens.Add("O perfil não pode ser vazio!");

    return validacao;
}

app.MapPost("/administrador/login", ([FromBody] MinimalApi.DTOs.LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    var adm = administradorServico.Login(loginDTO);
    if (adm == null)
        return Results.Unauthorized();

    var token = GerarTokenJwt(adm);
    return Results.Ok(new Administradorlogado
    {
        Email = adm.Email,
        Perfil = adm.Perfil,
        Token = token
    });
})
.AllowAnonymous()
.WithTags("Administrador");

app.MapPost("/administrador/cadastro", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
    var validacao = ValidaAdmDTO(administradorDTO);
    if (validacao.Mensagens.Any())
        return Results.BadRequest(validacao);

    var administrador = new Administrador
    {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil
    };

    administradorServico.Incluir(administrador);
    return Results.Created($"/administrador/{administrador.Id}", new { administrador.Email, administrador.Perfil });
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Administrador");

app.MapPut("/administrador/atualizar/{id}", ([FromRoute] int id, AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
    var validacao = ValidaAdmDTO(administradorDTO);
    if (validacao.Mensagens.Any())
        return Results.BadRequest(validacao);

    var administrador = administradorServico.BuscarPorId(id);
    if (administrador == null) return Results.NotFound();

    administrador.Email = administradorDTO.Email;
    administrador.Senha = administradorDTO.Senha;
    administrador.Perfil = administradorDTO.Perfil;

    administradorServico.Atualizar(administrador);
    return Results.Ok(new { administrador.Email, administrador.Perfil });
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Administrador");

app.MapGet("/administrador/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscarPorId(id);
    return administrador == null ? Results.NotFound() : Results.Ok(administrador);
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Administrador");

app.MapGet("/administrador/listar", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{
    var administradores = administradorServico.Todos(pagina);
    return Results.Ok(administradores);
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Administrador");

app.MapDelete("/administrador/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscarPorId(id);
    if (administrador == null) return Results.NotFound();

    administradorServico.Remover(administrador);
    return Results.NoContent();
})
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Administrador");
#endregion

#region App
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();

#endregion
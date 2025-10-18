var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Rota GET simples
app.MapGet("/", () => "Hello World!");

// Rota POST que recebe um JSON e faz uma verificação
app.MapPost("/login", (MinimalApi.DTOs.LoginDTO loginDTO) =>
{
    if (loginDTO.Email == "adm@teste.com" && loginDTO.Senha == "123456")
    {
        return Results.Ok("Login successful");
    }
    else
    {
        return Results.Unauthorized();
    }
});

app.Run();



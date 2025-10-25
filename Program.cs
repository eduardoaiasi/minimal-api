using MinimalApi;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

// ⚙️ Chama Configure com os parâmetros esperados
startup.Configure(app, builder.Environment, builder.Configuration);

app.Run();
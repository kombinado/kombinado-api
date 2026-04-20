using Kombinado.Api.Data;
using Kombinado.Api.Services.Auth;
using Kombinado.Api.Services.Token;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.Load(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"));

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Entity Framework Core configuration
string? connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
builder.Services.AddDbContext<KombinadoDbContext>(o => o.UseNpgsql(connectionString));

// Dependency Injection for services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Create a root endpoint
app.Map("/", () => "Welcome to Kombinado API!");

// Map controllers
app.MapControllers();

app.Run();
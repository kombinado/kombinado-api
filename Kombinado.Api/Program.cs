using Kombinado.Api.Data;
using Kombinado.Api.Services.Auth;
using Kombinado.Api.Services.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using Kombinado.Api.Extensions;
using Kombinado.Api.Handlers;
using Kombinado.Api.Models;

// Load environment variables from the .env file
DotNetEnv.Env.Load(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"));

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Entity Framework Core configuration
string? connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
builder.Services.AddDbContext<KombinadoDbContext>(o => o.UseNpgsql(connectionString));

// Dependency Injection for services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Global exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// JWT Authentication configuration
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCustomPolicies();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

WebApplication app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Create a root endpoint
app.Map("/", () => "Welcome to Kombinado API!");
app.MapControllers();

app.Run();
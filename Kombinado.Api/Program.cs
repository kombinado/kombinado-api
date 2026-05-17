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

// JWT Authentication configuration
string? secretKey = builder.Configuration["JWT_SECRET"] 
    ?? throw new InvalidOperationException("JWT_SECRET não configurado.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT_ISSUER"],
        ValidAudience = builder.Configuration["JWT_AUDIENCE"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
    
    options.Events = new JwtBearerEvents
    {
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            
            var response = ApiResponse<string>.FailureResponse("Acesso negado. Apenas motoristas podem acessar esta rota.", 403);
            var json = JsonSerializer.Serialize(response);
            
            return context.Response.WriteAsync(json);
        }
    };
});
builder.Services.AddCustomPolicies();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

WebApplication app = builder.Build();
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

// Map controllers
app.MapControllers();

app.Run();
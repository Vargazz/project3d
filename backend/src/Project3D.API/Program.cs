using System.Text;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Project3D.Application.Extensions;
using Project3D.Infrastructure.Data;
using Project3D.Infrastructure.Data.Seed;
using Project3D.Infrastructure.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------
// Serilog
// -------------------------------------------------------
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

// -------------------------------------------------------
// Infrastructure (EF, Identity, repos, services)
// -------------------------------------------------------
builder.Services.AddInfrastructure(builder.Configuration);

// -------------------------------------------------------
// Application (Mapster, FluentValidation)
// -------------------------------------------------------
var mapConfig = TypeAdapterConfig.GlobalSettings;
mapConfig.Scan(typeof(Project3D.Application.Mappings.MappingConfig).Assembly);
builder.Services.AddSingleton(mapConfig);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// Registrar validators do assembly Application
builder.Services.AddValidatorsFromAssembly(typeof(Project3D.Application.Validators.RegisterRequestValidator).Assembly);

// Application Services (vazio — registros feitos no Infrastructure)
builder.Services.AddApplicationServices();

// -------------------------------------------------------
// JWT Authentication
// -------------------------------------------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey não configurada.");
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CustomerOrAdmin", policy => policy.RequireRole("Customer", "Admin"));
});

// -------------------------------------------------------
// CORS
// -------------------------------------------------------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// -------------------------------------------------------
// Controllers e OpenAPI
// -------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddDirectoryBrowser();

var app = builder.Build();

// -------------------------------------------------------
// Migrations e Seed automáticos em desenvolvimento
// -------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await db.Database.MigrateAsync();

        // Seed: roda sempre que não houver categorias (banco recém-criado)
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao aplicar migrations ou seed.");
        throw;
    }
}

// -------------------------------------------------------
// Pipeline
// -------------------------------------------------------
app.UseSerilogRequestLogging();

// Middleware global de tratamento de exceções — converte exceptions em respostas HTTP adequadas
app.UseMiddleware<Project3D.API.Middleware.GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads"
});

app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

using System.Text;
using Collectivites.Api.Data;
using Collectivites.Api.Models.Converters;
using Collectivites.Api.Models.Entities;
using Collectivites.Api.Models.Enums;
using Collectivites.Api.Models.Options;
using Collectivites.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new DateTimeUtcJsonConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL + PostGIS via Npgsql et NetTopologySuite (chapitre 6)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.UseNetTopologySuite()));

// ---- Authentification JWT (UC-01) ----
// Valeurs par défaut de développement dans JwtOptions ; surchargeables via la section "Jwt"
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

// Couche Services (architecture en couches : Controllers / Services / Data)
builder.Services.AddScoped<ICollectiviteService, CollectiviteService>();
builder.Services.AddScoped<ICollectiviteImportService, CollectiviteImportService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUtilisateurService, UtilisateurService>();
builder.Services.AddScoped<IProjetDotationService, ProjetDotationService>();
builder.Services.AddScoped<IIndicateurService, IndicateurService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed : utilisateur administrateur par défaut (UC-01, dev uniquement)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        if (!await db.Utilisateurs.AnyAsync(u => u.Identifiant == "admin"))
        {
            db.Utilisateurs.Add(new Utilisateur
            {
                Id = Guid.NewGuid(),
                Nom = "Administrateur",
                Identifiant = "admin",
                Role = Role.Administrateur,
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234")
            });
            await db.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Base de données indisponible au démarrage : migration et seed ignorés.");
    }
}

app.Run();

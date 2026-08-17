using Collectivites.Api.Data;
using Collectivites.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL + PostGIS via Npgsql et NetTopologySuite (chapitre 6)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.UseNetTopologySuite()));

// Couche Services (architecture en couches : Controllers / Services / Data)
builder.Services.AddScoped<ICollectiviteService, CollectiviteService>();
builder.Services.AddScoped<ICollectiviteImportService, CollectiviteImportService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

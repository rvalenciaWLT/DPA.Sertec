using System.Text.Json.Serialization;
using DPA.Sertec.Data.Core.Interfaces;
using DPA.Sertec.Data.Infrastructure.Data;
using DPA.Sertec.Data.Infrastructure.Repositories;
using DPA.Sertec.Data.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Enfoque Database First: el DbContext lo genero Scaffold-DbContext y se registra
// aqui por inyeccion de dependencias. La cadena de conexion vive en appsettings.json,
// no dentro del codigo generado (por eso se uso -NoOnConfiguring en el scaffolding).
builder.Services.AddDbContext<SertecDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SertecDB")));

// Pregunta 5: inyeccion de dependencias del patron Repository.
// Scoped = una instancia por peticion HTTP, el mismo ciclo de vida del DbContext.
// Se registra contra la INTERFAZ: el controlador nunca ve las clases concretas.
builder.Services.AddScoped<IOrdenReparacionRepository, OrdenReparacionRepository>();
builder.Services.AddScoped<IOrdenReparacionService, OrdenReparacionService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Las entidades scaffoldeadas tienen navegaciones bidireccionales
        // (TipoReparacion -> OrdenReparacions -> TipoReparacion). Sin esto
        // System.Text.Json aborta con "object cycle detected".
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

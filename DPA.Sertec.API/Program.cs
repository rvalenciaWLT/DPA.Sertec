using System.Text.Json.Serialization;
using DPA.Sertec.Data.Core.Interfaces;
using DPA.Sertec.Data.Infrastructure.Data;
using DPA.Sertec.Data.Infrastructure.Repositories;
using DPA.Sertec.Data.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SertecDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SertecDB")));

builder.Services.AddScoped<IOrdenReparacionRepository, OrdenReparacionRepository>();
builder.Services.AddScoped<IOrdenReparacionService, OrdenReparacionService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Las entidades scaffoldeadas tienen navegaciones bidireccionales.
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

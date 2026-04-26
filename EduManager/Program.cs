using System.Reflection;
using EduManager;
using EduManager.Data;
using Microsoft.EntityFrameworkCore;
using EduManager.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Swagger generátor hozzáadása
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "EduManager API", Version = "v1" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});
builder.Services.AddAutoMapper(config => { }, Assembly.GetExecutingAssembly());
builder.Services.AddScoped<IUnitOfWork, SimpleUnitOfWork>();
builder.Services.AddHostedService<NotificationBackgroundService>();

// builder.Services.AddDbContext<EduDbContext>(options => options.UseSqlServer("Server=localhost; Database=CourseManagerDB_WZFXSG; User Id=sa; Password=Password123!; TrustServerCertificate=True;"));
builder.Services.AddDbContext<EduDbContext>(options => options.UseSqlServer("Server=localhost; Database=CourseManagerDB_WZFXSG; User Id=sa; Password=RentACar_2026!; TrustServerCertificate=True;"));

builder.Services.AddOpenApi();



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<EduDbContext>();
        // Inicializáló meghívása
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Hiba történt az adatbázis inicializálásakor: " + ex.Message);
        if (ex.InnerException != null)
        {
            Console.WriteLine("Belső hiba (SQL): " + ex.InnerException.Message);
        }
    }
}

app.Run();

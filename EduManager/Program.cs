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
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(config => { }, Assembly.GetExecutingAssembly());
builder.Services.AddScoped<IUnitOfWork, SimpleUnitOfWork>();
builder.Services.AddHostedService<NotificationBackgroundService>();

// builder.Services.AddDbContext<EduDbContext>(options => options.UseSqlServer("Server=localhost; Database=CourseManagerDB_WZFXSG; User Id=sa; Password=Password123!; TrustServerCertificate=True;"));
builder.Services.AddDbContext<EduDbContext>(options => options.UseSqlServer("Server=localhost; Database=CourseManagerDB_WZFXSG; User Id=sa; Password=RentACar_2026!; TrustServerCertificate=True;"));

builder.Services.AddOpenApi();



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Ez generálja a JSON dokumentációt
    app.UseSwagger();
    
    // Ez hozza létre a vizuális felületet a böngészőben
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty; // Így a főoldalon (localhost:5000) rögtön a Swagger fogad
    });
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

/*app.UseHttpsRedirection();

app.UseAuthorization();*/

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<EduDbContext>();
        // Itt hívjuk meg a saját inicializálónkat
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        // Itt logolhatod, ha valami hiba történt az adatbázis elérésekor
        Console.WriteLine("Hiba történt az adatbázis inicializálásakor: " + ex.Message);
        if (ex.InnerException != null)
        {
            Console.WriteLine("Belső hiba (SQL): " + ex.InnerException.Message);
        }
    }
}

/*// Felhasználók kilistázása ellenőrzéshez
app.MapGet("/test-users", (EduDbContext context) => 
{
    return context.Users.ToList();
});

// Tantárgyak kilistázása ellenőrzéshez
app.MapGet("/test-subjects", (EduDbContext context) => 
{
    return context.Subjects.ToList();
});*/

app.Run();

using Microsoft.EntityFrameworkCore;
using EduManager.Entities;

namespace EduManager.Data;

public class EduDbContext : DbContext
{
    public EduDbContext(DbContextOptions<EduDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Notification> Notifications { get; set; } 

    /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Kapcsolati karakterlánc beállítása (Linux Docker SQL Server-hez igazítva)
        optionsBuilder.UseSqlServer("Server=localhost; Database=CourseManagerDB_WZFXSG; User Id=sa; Password=Password123!; TrustServerCertificate=True;");
    }*/
}
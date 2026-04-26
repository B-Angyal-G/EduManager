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
    public DbSet<CourseSchedule> CourseSchedules { get; set; }
    public DbSet<NotificationLog> Notifications { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Hallgatók és kurzusok kapcsolata (Saját kapcsolótábla)
        modelBuilder.Entity<Course>()
            .HasMany(c => c.Students)
            .WithMany(u => u.Courses) // Feltételezve, hogy a User-ben van egy ICollection<Course> Courses
            .UsingEntity(j => j.ToTable("CourseStudents"));

        // Oktatók és kurzusok kapcsolata (Saját kapcsolótábla)
        modelBuilder.Entity<Course>()
            .HasMany(c => c.Teachers)
            .WithMany() // Ha a User-ben nincs külön 'TeachingCourses' lista, hagyjuk üresen
            .UsingEntity(j => j.ToTable("CourseTeachers"));
    }
    
    /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Kapcsolati karakterlánc beállítása (Linux Docker SQL Server-hez igazítva)
        optionsBuilder.UseSqlServer("Server=localhost; Database=CourseManagerDB_WZFXSG; User Id=sa; Password=Password123!; TrustServerCertificate=True;");
    }*/
}
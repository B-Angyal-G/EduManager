using EduManager.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduManager.Data;

public class DbInitializer
{
    public static void Initialize(EduDbContext context)
    {
        // 1. Lefuttatja a függőben lévő migrációkat (opcionális, de hasznos)
        context.Database.Migrate();

        // 2. Ellenőrzi, vannak-e már felhasználók. Ha igen, nem csinál semmit.
        if (context.Users.Any())
        {
            return;
        }

        // 3. Tesztadatok létrehozása (a specifikáció szerinti típusokkal)
        var users = new User[]
        {
            new User
            {
                Username = "admin_pista", Email = "admin@student.uni-pannon.hu", Password = "password123", Role = UserRole.Admin,
                IsActive = true
            },
            new User
            {
                Username = "toth_janos", Email = "toth.j@student.uni-pannon.hu", Password = "password123", Role = UserRole.Teacher,
                IsActive = true
            },
            new User
            {
                Username = "kiss_pista", Email = "k.pisti@student.uni-pannon.hu", Password = "password123",
                Role = UserRole.Student, StudyMode = StudyMode.FullTime, IsActive = true
            }
        };

        context.Users.AddRange(users);

        context.Subjects.AddRange(
        new Subject { Code = "PROG1", Name = "Programozás I.", Credits = 5, IsActive = true },
        new Subject { Code = "PROG2", Name = "Haladó Programozás II.", Credits = 5, IsActive = true },
        new Subject { Code = "ADATDB1", Name = "Adatbáziskezelés I", Credits = 3, IsActive = true }
            );

        // 5. Mentés az adatbázisba
        context.SaveChanges();
    }
}
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
        var teachers = new List<User>
        {
            new() { Username = "toth_janos", Email = "toth.j@tanar.hu", Password = "password123", Role = UserRole.Teacher, IsActive = true },
            new() { Username = "nagy_eva", Email = "nagy.e@tanar.hu", Password = "password123", Role = UserRole.Teacher, IsActive = true },
            new() { Username = "szabo_gabor", Email = "szabo.g@tanar.hu", Password = "password123", Role = UserRole.Teacher, IsActive = true },
            new() { Username = "horvath_zoltan", Email = "horvath.z@tanar.hu", Password = "password123", Role = UserRole.Teacher, IsActive = true },
            new() { Username = "molnar_maria", Email = "molnar.m@tanar.hu", Password = "password123", Role = UserRole.Teacher, IsActive = true }
        };

        var students = new List<User>
        {
            new() { Username = "kiss_pista", Email = "k.pisti@student.hu", Password = "password123", Role = UserRole.Student, StudyMode = StudyMode.FullTime, IsActive = true },
            new() { Username = "kovacs_anna", Email = "k.anna@student.hu", Password = "password123", Role = UserRole.Student, StudyMode = StudyMode.FullTime, IsActive = true },
            new() { Username = "varga_luca", Email = "v.luca@student.hu", Password = "password123", Role = UserRole.Student, StudyMode = StudyMode.FullTime, IsActive = true },
            new() { Username = "szabo_mate", Email = "sz.mate@student.hu", Password = "password123", Role = UserRole.Student, StudyMode = StudyMode.FullTime, IsActive = true },
            new() { Username = "toth_balazs", Email = "t.balazs@student.hu", Password = "password123", Role = UserRole.Student, StudyMode = StudyMode.FullTime, IsActive = true },
            new() { Username = "farkas_flora", Email = "f.flora@student.hu", Password = "password123", Role = UserRole.Student, StudyMode = StudyMode.FullTime, IsActive = true },
            new() { Username = "papp_luca", Email = "p.luca@student.hu", Password = "password123", Role = UserRole.Student, StudyMode = StudyMode.FullTime, IsActive = true },
            new() { Username = "takacs_gergo", Email = "t.gergo@student.hu", Password = "password123", Role = UserRole.Student, StudyMode = StudyMode.FullTime, IsActive = true },
            new() { Username = "meszaros_akos", Email = "m.akos@student.hu", Password = "password123", Role = UserRole.Student, StudyMode = StudyMode.FullTime, IsActive = true },
            new() { Username = "biro_kitti", Email = "b.kitti@student.hu", Password = "password123", Role = UserRole.Student, StudyMode = StudyMode.FullTime, IsActive = true },
            new() { Username = "molnar_eszter", Email = "m.eszter@student.hu", Password = "password123", Role = UserRole.Student, StudyMode = StudyMode.PartTime, IsActive = true },
            new() { Username = "horvath_zsofi", Email = "h.zsofi@student.hu", Password = "password123", Role = UserRole.Student, StudyMode = StudyMode.PartTime, IsActive = true },
            new() { Username = "balogh_adam", Email = "b.adam@student.hu", Password = "password123", Role = UserRole.Student, StudyMode = StudyMode.PartTime, IsActive = true },
            new() { Username = "juhasz_emese", Email = "j.emese@student.hu", Password = "password123", Role = UserRole.Student, StudyMode = StudyMode.PartTime, IsActive = true },
            new() { Username = "simon_david", Email = "s.david@student.hu", Password = "password123", Role = UserRole.Student, StudyMode = StudyMode.PartTime, IsActive = true },
            new() { Username = "nemeth_bela", Email = "n.bela@student.hu", Password = "password123", Role = UserRole.Student, StudyMode = StudyMode.PartTime, IsActive = true }
        };

        var admins = new List<User>
        {
            new() { Username = "admin_armand", Email = "admin_armand@edu.hu", Password = "password123", Role = UserRole.Admin, IsActive = true },
            new() { Username = "admin_reka", Email = "admin.reka@edu.hu", Password = "password123", Role = UserRole.Admin, IsActive = true },
            new() { Username = "admin_viktor", Email = "admin.viktor@edu.hu", Password = "password123", Role = UserRole.Admin, IsActive = true }
        };

        context.Users.AddRange(teachers);
        context.Users.AddRange(students);
        context.Users.AddRange(admins);
        context.SaveChanges(); // Elmentjük, hogy legyen ID-juk a továbbiakhoz

        // 2. TANTÁRGYAK
        var prog1 = new Subject { Code = "PROG1", Name = "Programozás I.", Credits = 5, IsActive = true };
        var adatb1 = new Subject { Code = "ADATDB1", Name = "Adatbáziskezelés I", Credits = 3, IsActive = true };
        var web2 = new Subject { Code = "WEB2", Name = "Webfejlesztés II.", Credits = 4, IsActive = true };
        var algo1 = new Subject { Code = "ALGO1", Name = "Adatstruktúrák és algoritmusok", Credits = 4, IsActive = true };

        context.Subjects.AddRange(prog1, adatb1, web2, algo1);
        context.SaveChanges();

        // 3. KURZUSOK (Tesztesetekhez optimalizálva)
        var semester = "2025/26/2";

        var courses = new List<Course>
        {
            // === PROG1 (Programozás I.) ===
            new() { 
                CourseCode = "PROG1-ELM-01", SubjectId = prog1.Id, Type = CourseType.Theory, 
                Form = CourseForm.FullTime, Semester = semester, MaxStudents = 100,
                Teachers = new List<User> { teachers[0] } 
            },
            new() { 
                CourseCode = "PROG1-LAB-01", SubjectId = prog1.Id, Type = CourseType.Lab, 
                Form = CourseForm.FullTime, Semester = semester, MaxStudents = 2, // Tesztelni a betelt kurzust
                Teachers = new List<User> { teachers[0] },
                Students = new List<User> { students[0] } 
            },

            // === ADATB1 (Adatbáziskezelés I.) ===
            new() { 
                CourseCode = "ADATB1-COMB-ELM", SubjectId = adatb1.Id, Type = CourseType.Theory, 
                Form = CourseForm.Combined, Semester = semester, MaxStudents = 50,
                Teachers = new List<User> { teachers[1] } 
            },
            new() { 
                CourseCode = "ADATB1-GYAK-FT", SubjectId = adatb1.Id, Type = CourseType.Practice, 
                Form = CourseForm.FullTime, Semester = semester, MaxStudents = 20,
                Teachers = new List<User> { teachers[2] } 
            },
            new() { 
                CourseCode = "ADATB1-GYAK-PT", SubjectId = adatb1.Id, Type = CourseType.Practice, 
                Form = CourseForm.PartTime, Semester = semester, MaxStudents = 15,
                Teachers = new List<User> { teachers[2] } 
            },

            // === WEB2 (Webfejlesztés II.) ===
            new() { 
                CourseCode = "WEB2-ELM-01", SubjectId = web2.Id, Type = CourseType.Theory, 
                Form = CourseForm.FullTime, Semester = semester, MaxStudents = 80,
                Teachers = new List<User> { teachers[1] } 
            },
            new() { 
                CourseCode = "WEB2-LAB-01", SubjectId = web2.Id, Type = CourseType.Lab, 
                Form = CourseForm.FullTime, Semester = semester, MaxStudents = 15,
                Teachers = new List<User> { teachers[1] } 
            },

            // === ALGO1 (Adatstruktúrák és algoritmusok) ===
            new() { 
                CourseCode = "ALGO1-ELM-01", SubjectId = algo1.Id, Type = CourseType.Theory,
                Form = CourseForm.FullTime, Semester = semester, MaxStudents = 100,
                Teachers = new List<User> { teachers[2] } 
            },
            new() { 
                CourseCode = "ALGO1-GYAK-01", SubjectId = algo1.Id, Type = CourseType.Practice, 
                Form = CourseForm.FullTime, Semester = semester, MaxStudents = 20,
                Teachers = new List<User> { teachers[2] } 
            }
        };

        context.Courses.AddRange(courses);
        context.SaveChanges();

        // 4. ÓRARENDI IDŐPONTOK (A Background Service teszteléséhez)
        // Egy óra, ami 31 perc múlva kezdődik
        var now = DateTime.Now;
        context.CourseSchedules.Add(new CourseSchedule
        {
            CourseId = courses[0].Id,
            StartTime = now.AddMinutes(31),
            EndTime = now.AddMinutes(120)
        });

        // Egy távolabbi időpont
        context.CourseSchedules.Add(new CourseSchedule
        {
            CourseId = courses[1].Id,
            StartTime = now.AddDays(1).Date.AddHours(8), // Holnap reggel 8
            EndTime = now.AddDays(1).Date.AddHours(10)
        });

        context.SaveChanges();
    }
}
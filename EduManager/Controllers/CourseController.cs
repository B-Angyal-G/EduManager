using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduManager.Data;
using EduManager.DTO.CourseDTO;
using EduManager.Entities;

namespace EduManager.Controllers
{
    // [Route("api/[controller]")]
    [Route("api/courses")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly EduDbContext _context;

        public CourseController(EduDbContext context)
        {
            _context = context;
        }

        // GET: api/Course
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseGetDTO>>> GetCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Teachers)
                .Select(c => new CourseGetDTO
                {
                    Id = c.Id,
                    CourseCode = c.CourseCode,
                    SubjectName = c.Subject != null ? c.Subject.Name : "Ismeretlen tárgy",
                    Semester = c.Semester,
                    MaxStudents = c.MaxStudents,
                    Type = c.Type.ToString(),
                    Form = c.Form.ToString(),
                    HoursDescription = $"{c.Hours} {(c.HourUnit == HourType.Weekly ? "heti" : "féléves")}",
                    TeacherNames = c.Teachers.Select(t => t.Username).ToList()
                })
                .ToListAsync();

            return Ok(courses);
        }

        
        // GET: api/Course/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseGetDTO>> GetCourse(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Teachers)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound();
            }

            var dto = new CourseGetDTO
            {
                Id = course.Id,
                CourseCode = course.CourseCode,
                SubjectName = course.Subject?.Name ?? "Ismeretlen tárgy",
                Semester = course.Semester,
                MaxStudents = course.MaxStudents,
                Type = course.Type.ToString(),
                Form = course.Form.ToString(),
                HoursDescription = $"{course.Hours} {(course.HourUnit == HourType.Weekly ? "heti" : "féléves")}",
                TeacherNames = course.Teachers.Select(t => t.Username).ToList()
            };

            return Ok(dto);
        }

        
        // PUT: api/Course/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(int courseId, CourseUpdateDTO dto)
        {
            if (courseId != dto.Id)
            {
                return BadRequest("Az URL-ben szereplő és a testben küldött ID nem egyezik.");
            }

            // Megkeressük a meglévő kurzust (oktatók nélkül is elég, mert azokhoz nem nyúlunk)
            var existingCourse = await _context.Courses.FindAsync(courseId);

            if (existingCourse == null)
            {
                return NotFound();
            }

            // Kurzuskód egyediség ellenőrzése, ha megváltozott
            if (existingCourse.CourseCode != dto.CourseCode && 
                await _context.Courses.AnyAsync(c => c.CourseCode == dto.CourseCode))
            {
                return BadRequest("Ez a kurzuskód már foglalt egy másik kurzusnál.");
            }

            // Adatok frissítése
            existingCourse.CourseCode = dto.CourseCode;
            existingCourse.Semester = dto.Semester;
            existingCourse.MaxStudents = dto.MaxStudents;
            existingCourse.Type = dto.Type;
            existingCourse.Form = dto.Form;
            existingCourse.Hours = dto.Hours;
            existingCourse.HourUnit = dto.HourUnit;
            

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(courseId)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        
        // POST: api/Course
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CourseGetDTO>> PostCourse(CourseCreateDTO dto)
        {
            // 1. Tantárgy kikeresése és ellenőrzése
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Code == dto.SubjectCode);

            if (subject == null)
            {
                return BadRequest($"Nem található tantárgy ezzel a kóddal: {dto.SubjectCode}");
            }

            // Csak aktív tárgyhoz hirdethető kurzus
            if (!subject.IsActive)
            {
                return BadRequest("Inaktív tantárgyhoz nem hirdethető új kurzus.");
            }

            // 2. Kurzuskód egyediség ellenőrzése
            if (await _context.Courses.AnyAsync(c => c.CourseCode == dto.CourseCode))
            {
                return BadRequest("Ez a kurzuskód már foglalt.");
            }

            // 3. Oktatók ellenőrzése
            // Csak azokat a felhasználókat keressük ki, akiknek az ID-ja benne van a listában és oktatók
            var teachers = await _context.Users
                .Where(u => dto.TeacherIds.Contains(u.Id) && u.Role == UserRole.Teacher)
                .ToListAsync();

            if (teachers.Count != dto.TeacherIds.Count)
            {
                return BadRequest("Egy vagy több megadott oktató ID érvénytelen, vagy a felhasználó nem oktató.");
            }

            // 4. Kurzus entitás létrehozása és adatok leképezése
            var course = new Course
            {
                CourseCode = dto.CourseCode,
                SubjectId = subject.Id,
                Semester = dto.Semester,
                MaxStudents = dto.MaxStudents,
                Type = dto.Type,
                Form = dto.Form,
                Hours = dto.Hours,
                HourUnit = dto.HourUnit,
                Teachers = teachers
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // 5. Válasz DTO összeállítása
            var responseDto = new CourseGetDTO
            {
                Id = course.Id,
                CourseCode = course.CourseCode,
                SubjectName = subject.Name,
                Semester = course.Semester,
                MaxStudents = course.MaxStudents,
                Type = course.Type.ToString(),
                Form = course.Form.ToString(),
                HoursDescription = $"{course.Hours} {(course.HourUnit == HourType.Weekly ? "heti" : "féléves")}",
                TeacherNames = teachers.Select(t => t.Username).ToList()
            };

            return CreatedAtAction(nameof(GetCourse), new { courseId = course.Id }, responseDto);
        }

        
        // DELETE: api/Course/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int courseId)
        {
            // 1. Megkeressük a kurzust, és betöltjük a hallgatóit is
            var course = await _context.Courses
                .Include(c => c.Students) 
                .FirstOrDefaultAsync(c => c.Id == courseId);

            // 2. Ha nem létezik a kurzus
            if (course == null)
            {
                return NotFound();
            }

            // 3. Csak akkor törölhető, ha nincs rajta hallgató
            if (course.Students != null && course.Students.Any())
            {
                return BadRequest("A kurzus nem törölhető, mert már vannak rá jelentkezett hallgatók. Előbb távolítsd el a hallgatókat!");
            }

            // 4. Tényleges törlés
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            // 5. Sikeres törlés
            return NoContent();
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}

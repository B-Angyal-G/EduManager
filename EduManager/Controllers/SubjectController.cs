using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduManager.Data;
using EduManager.DTO.SubjectDTO;
using EduManager.Entities;

namespace EduManager.Controllers
{
    // [Route("api/[controller]")]
    [Route("api/subjects")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly EduDbContext _context;

        public SubjectController(EduDbContext context)
        {
            _context = context;
        }

        
        // GET: api/Subject
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubjectGetDTO>>> GetSubjects()
        {
            return await _context.Subjects
                .Where(s => s.IsActive)
                .Select(s => new SubjectGetDTO
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name,
                    Credits = s.Credits,
                    IsActive = s.IsActive
                })
                .ToListAsync();
        }

        
        // GET: api/Subject/5
        [HttpGet("{subjectId}")]
        public async Task<ActionResult<SubjectGetDTO>> GetSubject(int subjectId)
        {
            var subject = await _context.Subjects.FindAsync(subjectId);

            if (subject == null)
            {
                return NotFound();
            }
            
            return new SubjectGetDTO {
                Id = subject.Id,
                Code = subject.Code,
                Name = subject.Name,
                Credits = subject.Credits,
                IsActive = subject.IsActive
            };
        }

        
        // PUT: api/Subject/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSubject(int subjectId, SubjectUpdateDTO dto)
        {
            // 1. Keressük meg a létező tárgyat
            var existingSubject = await _context.Subjects.FindAsync(subjectId);
            
            if (existingSubject == null)
            {
                return NotFound();
            }

            // 2. Kód egyediség ellenőrzése
            // Csak akkor nézzük, ha a kód megváltozott!
            if (existingSubject.Code != dto.Code && await _context.Subjects.AnyAsync(s => s.Code == dto.Code))
            {
                return BadRequest("Ez a tantárgy kód már használatban van egy másik tárgynál.");
            }

            // 3. Módosítások átvezetése
            existingSubject.Code = dto.Code;
            existingSubject.Name = dto.Name;
            existingSubject.Credits = dto.Credits;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Subjects.Any(s => s.Id == subjectId)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        
        // POST: api/Subject
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SubjectGetDTO>> PostSubject(SubjectCreateDTO dto)
        {
            // Ellenőrzés: ne legyen két ugyanolyan kódú tárgy (pl. két PROG1)
            if (await _context.Subjects.AnyAsync(s => s.Code == dto.Code))
            {
                return BadRequest("Ezzel a kóddal már létezik tantárgy.");
            }
            
            var subject = new Subject
            {
                Code = dto.Code,
                Name = dto.Name,
                Credits = dto.Credits,
                IsActive = true
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubject), new { subjectId = subject.Id }, new SubjectGetDTO 
            { 
                Id = subject.Id,
                Code = subject.Code,
                Name = subject.Name,
                Credits = subject.Credits,
                IsActive = true 
            });
        }

        
        // POST: api/subjects/5/deactivate
        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> DeactivateSubject(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }

            if (!subject.IsActive)
            {
                return BadRequest("A tantárgy már alapból inaktív.");
            }

            subject.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Subject {id} deactivated successfully." });
        }
        
        
        // POST: api/subjects/5/reactivate
        [HttpPost("{id}/reactivate")]
        public async Task<IActionResult> ReactivateSubject(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }

            if (subject.IsActive)
            {
                return BadRequest("A tantárgy már alapból aktív.");
            }

            subject.IsActive = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Subject {id} reactivated successfully." });
        }
        
        
        
        
        // <=== JELENTKEZÉSEK ===>
        // POST /api/subjects/{subjectId}/register
        [HttpPost("{subjectId}/register")]
        public async Task<IActionResult> RegisterToSubject(int subjectId, SubjectRegisterDTO dto)
        {
            // 1. Hallgató ellenőrzése
            var student = await _context.Users.FindAsync(dto.StudentId);
            if (student == null || student.Role != UserRole.Student || !student.IsActive)
            {
                return BadRequest("Érvénytelen vagy inaktív hallgató.");
            }

            // 2. Kurzusok betöltése és alapvető ellenőrzések
            var coursesToRegister = await _context.Courses
                .Include(c => c.Students) // Kell a létszámellenőrzéshez
                .Where(c => dto.CourseIds.Contains(c.Id))
                .ToListAsync();

            if (coursesToRegister.Count != dto.CourseIds.Count)
            {
                return BadRequest("Egy vagy több kurzus ID érvénytelen.");
            }

            // --- VALIDÁCIÓK A SPECIFIKÁCIÓ SZERINT ---

            // A: Minden kurzus ehhez a tárgyhoz tartozik-e?
            if (coursesToRegister.Any(c => c.SubjectId != subjectId))
            {
                return BadRequest("Minden választott kurzusnak a megadott tárgyhoz kell tartoznia.");
            }

            // B: Minden kurzus ugyanahhoz a félévhez tartozik-e?
            var semesters = coursesToRegister.Select(c => c.Semester).Distinct();
            if (semesters.Count() > 1)
            {
                return BadRequest("Az összes felvett kurzusnak ugyanahhoz a félévhez kell tartoznia.");
            }
            string currentSemester = semesters.First();

            // C: Tagozat ellenőrzése (Nappalis -> Nappali/Combined, Levelezős -> Levelező/Combined)
            foreach (var course in coursesToRegister)
            {
                bool isCompatible = false;
                if (course.Form == CourseForm.Combined) isCompatible = true;
                else if (student.StudyMode == StudyMode.FullTime && course.Form == CourseForm.FullTime) isCompatible = true;
                else if (student.StudyMode == StudyMode.PartTime && course.Form == CourseForm.PartTime) isCompatible = true;

                if (!isCompatible)
                {
                    return BadRequest($"A(z) {course.CourseCode} kurzus tagozata nem felel meg a hallgató képzési rendjének.");
                }
            }

            // D: Létszámkeret ellenőrzése
            if (coursesToRegister.Any(c => c.Students.Count >= c.MaxStudents))
            {
                return BadRequest("Egy vagy több kurzus betelt.");
            }

            // E: "Minden elérhető típusból pontosan egyet" szabály
            // Megnézzük, milyen típusok (elmélet/gyak/labor) érhetőek el ebből a tárgyból ebben a félévben a hallgatónak
            var availableTypes = await _context.Courses
                .Where(c => c.SubjectId == subjectId && c.Semester == currentSemester && 
                           (c.Form == CourseForm.Combined || 
                            (student.StudyMode == StudyMode.FullTime && c.Form == CourseForm.FullTime) ||
                            (student.StudyMode == StudyMode.PartTime && c.Form == CourseForm.PartTime)))
                .Select(c => c.Type)
                .Distinct()
                .ToListAsync();

            var pickedTypes = coursesToRegister.Select(c => c.Type).ToList();

            if (availableTypes.Count != pickedTypes.Count || !availableTypes.All(t => pickedTypes.Contains(t)))
            {
                return BadRequest("A tantárgy teljesítéséhez szükséges összes kurzustípust fel kell venni pontosan egyszer.");
            }

            // 3. MENTÉS
            foreach (var course in coursesToRegister)
            {
                course.Students.Add(student);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Sikeres tárgyfelvétel." });
        }
        

        private bool SubjectExists(int id)
        {
            return _context.Subjects.Any(e => e.Id == id);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduManager.Data;
using EduManager.DTO.SubjectDTO;
using EduManager.DTO.UserDto;
using EduManager.Entities;
using EduManager.Repository;

namespace EduManager.Controllers
{
    // [Route("api/[controller]")]
    [Route("api/subjects")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SubjectController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        
        // GET: api/Subject
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubjectGetDTO>>> GetSubjects()
        {
            // Csak az aktív tantárgyakat listázzuk
            var subjects = await _unitOfWork.SubjectRepository.GetAllAsync(s => s.IsActive);
            return Ok(_mapper.Map<List<SubjectGetDTO>>(subjects));
        }

        
        // GET: api/Subject/5
        [HttpGet("{subjectId}")]
        public async Task<ActionResult<SubjectGetDTO>> GetSubject(int id)
        {
            var subject = await _unitOfWork.SubjectRepository.FindByIdAsync(id);

            if (subject == null || !subject.IsActive) return NotFound();

            return Ok(_mapper.Map<SubjectGetDTO>(subject));
        }

        
        // PUT: api/Subject/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSubject(int id, SubjectUpdateDTO dto)
        {
            if (id != dto.Id) return BadRequest("Az ID-k nem egyeznek.");

            var subject = await _unitOfWork.SubjectRepository.FindByIdAsync(id);
            if (subject == null) return NotFound();

            // Kód egyediség ellenőrzése, ha megváltozott a kód
            if (subject.Code != dto.Code)
            {
                var codeExists = await _unitOfWork.SubjectRepository.GetAllAsync(s => s.Code == dto.Code);
                if (codeExists.Any()) return BadRequest("Az új tantárgykód már foglalt.");
            }

            // AutoMapper: rátöltjük a DTO-t a létező entitásra
            _mapper.Map(dto, subject);

            _unitOfWork.SubjectRepository.Update(subject);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }

        
        // POST: api/Subject
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SubjectGetDTO>> PostSubject(SubjectCreateDTO dto)
        {
            // Ellenőrizzük, létezik-e már ilyen kódú tárgy (akár inaktív is!)
            var existing = await _unitOfWork.SubjectRepository.GetAllAsync(s => s.Code == dto.Code);
            if (existing.Any())
            {
                return BadRequest("Ez a tantárgykód már foglalt.");
            }

            var subject = _mapper.Map<Subject>(dto);
            subject.IsActive = true;

            _unitOfWork.SubjectRepository.Add(subject);
            await _unitOfWork.SaveAsync();

            var resultDto = _mapper.Map<SubjectGetDTO>(subject);
            return CreatedAtAction(nameof(GetSubject), new { id = subject.Id }, resultDto);
        }

        
        // POST: api/subjects/5/deactivate
        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> DeactivateSubject(int id)
        {
            var subject = await _unitOfWork.SubjectRepository.FindByIdAsync(id);
            if (subject == null) return NotFound();

            subject.IsActive = false;
            _unitOfWork.SubjectRepository.Update(subject);
            await _unitOfWork.SaveAsync();

            return Ok(new { message = "Tantárgy inaktiválva." });
        }
        
        
        // POST: api/subjects/5/reactivate
        [HttpPost("{id}/reactivate")]
        public async Task<IActionResult> ReactivateSubject(int id)
        {
            var subject = await _unitOfWork.SubjectRepository.FindByIdAsync(id);
            if (subject == null) return NotFound();

            subject.IsActive = true;
            _unitOfWork.SubjectRepository.Update(subject);
            await _unitOfWork.SaveAsync();

            return Ok(new { message = "Tantárgy aktiválva." });
        }
        
        
        
        
        // <=== JELENTKEZÉSEK ===>
        [HttpPost("{subjectId}/register")]
        public async Task<IActionResult> RegisterToSubject(int subjectId, SubjectRegisterDTO dto)
        {
            // 1. Hallgató ellenőrzése
            var student = await _unitOfWork.UserRepository.FindByIdAsync(dto.StudentId);
            if (student == null || student.Role != UserRole.Student || !student.IsActive)
                return BadRequest("Érvénytelen vagy inaktív hallgató.");

            // 2. Kurzusok betöltése (Include Students kell a létszámhoz)
            var includes = new[] { "Students" };
            var coursesToRegister = await _unitOfWork.CourseRepository.GetAllAsync(
                c => dto.CourseIds.Contains(c.Id), includes);

            if (coursesToRegister.Count != dto.CourseIds.Count)
                return BadRequest("Egy vagy több kurzus ID érvénytelen.");

            // --- SZABÁLYOK ELLENŐRZÉSE ---

            // A: Minden kurzus ehhez a tárgyhoz tartozik?
            if (coursesToRegister.Any(c => c.SubjectId != subjectId))
                return BadRequest("Minden kurzusnak a megadott tárgyhoz kell tartoznia.");

            // B: Minden kurzus ugyanahhoz a félévhez tartozik?
            var semester = coursesToRegister.First().Semester;
            if (coursesToRegister.Any(c => c.Semester != semester))
                return BadRequest("Minden kurzusnak ugyanahhoz a félévhez kell tartoznia.");

            // C: Tagozat ellenőrzése
            foreach (var course in coursesToRegister)
            {
                bool compatible = course.Form == CourseForm.Combined || 
                                 (student.StudyMode == StudyMode.FullTime && course.Form == CourseForm.FullTime) ||
                                 (student.StudyMode == StudyMode.PartTime && course.Form == CourseForm.PartTime);
                if (!compatible)
                    return BadRequest($"A(z) {course.CourseCode} kurzus tagozata nem megfelelő.");
            }

            // D: Létszámkeret ellenőrzése
            if (coursesToRegister.Any(c => c.Students.Count >= c.MaxStudents))
                return BadRequest("Egy vagy több kurzus betelt.");

            // E: Kurzustípusok ellenőrzése (Minden elérhető típusból pontosan egy)
            // Megnézzük, milyen típusok (Theory/Practice/Lab) hirdettek meg ebben a félévben a tagozatnak
            var availableCourses = await _unitOfWork.CourseRepository.GetAllAsync(c => 
                c.SubjectId == subjectId && c.Semester == semester &&
                (c.Form == CourseForm.Combined || 
                 (student.StudyMode == StudyMode.FullTime && c.Form == CourseForm.FullTime) ||
                 (student.StudyMode == StudyMode.PartTime && c.Form == CourseForm.PartTime)));

            var requiredTypes = availableCourses.Select(c => c.Type).Distinct().ToList();
            var pickedTypes = coursesToRegister.Select(c => c.Type).ToList();

            if (requiredTypes.Count != pickedTypes.Count || pickedTypes.Distinct().Count() != pickedTypes.Count)
                return BadRequest("Minden szükséges kurzustípusból (elmélet, gyakorlat, stb.) pontosan egyet kell felvenni.");

            // 3. Mentés (Kapcsolótábla frissítése)
            foreach (var course in coursesToRegister)
            {
                course.Students.Add(student);
                _unitOfWork.CourseRepository.Update(course);
            }

            await _unitOfWork.SaveAsync();
            return Ok(new { message = "Sikeres tárgyfelvétel!" });
        }
        
        
        [HttpPost("{subjectId}/unregister")]
        public async Task<IActionResult> UnregisterFromSubject(int subjectId, SubjectUnregisterDTO dto)
        {
            var includes = new[] { "Students" };
            // Kikérjük az összes olyan kurzust, amin a hallgató rajta van az adott tárgyból és félévből
            var courses = await _unitOfWork.CourseRepository.GetAllAsync(c => 
                    c.SubjectId == subjectId && 
                    c.Semester == dto.Semester && 
                    c.Students.Any(s => s.Id == dto.StudentId), 
                includes);

            if (!courses.Any()) return BadRequest("A hallgató nincs feliratkozva erre a tárgyra ebben a félévben.");

            var student = await _unitOfWork.UserRepository.FindByIdAsync(dto.StudentId);

            foreach (var course in courses)
            {
                course.Students.Remove(student!);
                _unitOfWork.CourseRepository.Update(course);
            }

            await _unitOfWork.SaveAsync();
            return Ok(new { message = "Sikeres lejelentkezés." });
        }
        
        
        [HttpGet("{subjectId}/students")]
        public async Task<ActionResult<IEnumerable<UserGetDTO>>> GetSubjectStudents(int subjectId, [FromQuery] string semester)
        {
            var includes = new[] { "Students" };
            var courses = await _unitOfWork.CourseRepository.GetAllAsync(c => 
                c.SubjectId == subjectId && c.Semester == semester, includes);

            // Összeszedjük az összes hallgatót az összes kurzusról (duplikációk nélkül)
            var students = courses.SelectMany(c => c.Students).DistinctBy(s => s.Id).ToList();

            return Ok(_mapper.Map<List<UserGetDTO>>(students));
        }
    }
}

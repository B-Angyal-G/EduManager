using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduManager.Data;
using EduManager.DTO.GradeDTO;
using EduManager.Entities;
using EduManager.Repository;

namespace EduManager.Controllers
{
    // [Route("api/[controller]")]
    [Route("api/grades")]
    [ApiController]
    public class GradesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GradesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: api/Grade
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GradeGetDTO>>> GetGrades(int subjectId, int studentId, string semester, bool onlyLatest = false)
        {
            var grades = await _unitOfWork.GradeRepository.GetAllAsync(
                g => g.SubjectId == subjectId && g.StudentId == studentId && g.Semester == semester,
                new[] { "Student", "Subject" });

            if (!grades.Any()) return NotFound("Nem találhatók jegyek.");

            if (onlyLatest)
            {
                var latest = grades.OrderByDescending(g => g.Time).First();
                return Ok(_mapper.Map<GradeGetDTO>(latest));
            }

            return Ok(_mapper.Map<IEnumerable<GradeGetDTO>>(grades));
        }

        /*// GET: api/Grade/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Grade>> GetGrade(int id)
        {
            var grade = await _context.Grades.FindAsync(id);

            if (grade == null)
            {
                return NotFound();
            }

            return grade;
        }*/

        /*// PUT: api/Grade/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGrade(int id, Grade grade)
        {
            if (id != grade.Id)
            {
                return BadRequest();
            }

            _context.Entry(grade).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GradeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }*/

        // POST: api/Grade
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<GradeGetDTO>> PostGrade(GradeCreateDTO dto)
        {
            // 1. Validálás: Jegy értéke
            if (dto.GradeValue < 1 || dto.GradeValue > 5) 
                return BadRequest("A jegy csak 1 és 5 közötti érték lehet.");

            // 2. Validálás: Hallgató rajta van-e a tárgyon az adott félévben
            // Megnézzük, van-e olyan kurzus a tárgyhoz az adott félévben, amin a diák rajta van
            var enrollment = await _unitOfWork.CourseRepository.GetAllAsync(
                c => c.SubjectId == dto.SubjectId && 
                     c.Semester == dto.Semester && 
                     c.Students.Any(s => s.Id == dto.StudentId),
                new[] { "Students" });

            if (!enrollment.Any())
                return BadRequest("A hallgató nem vette fel ezt a tárgyat ebben a félévben.");

            // 3. Validálás: Időpont félévhez kötése (egyszerűsített: 1. félév: szept-jan, 2. félév: febr-jún)
            bool isValidDate = dto.Semester.EndsWith("1") 
                ? (dto.Time.Month >= 9 || dto.Time.Month <= 1)
                : (dto.Time.Month >= 2 && dto.Time.Month <= 7);
            
            if (!isValidDate)
                return BadRequest("A megszerzés időpontja nem esik a megadott félév időszakába.");
            
            // Aláírás megléte
            var signatures = await _unitOfWork.SignatureRepository.GetAllAsync(
                s => s.StudentId == dto.StudentId && s.SubjectId == dto.SubjectId && s.Semester == dto.Semester);
            
            if (!signatures.Any()) 
                return BadRequest("Nincs aláírás bejegyzés. Jegy csak aláírás után adható.");
            
            // Megnézzük a legutolsót
            var latestSignature = signatures.OrderByDescending(s => s.Time).First();
            if (!latestSignature.IsSigned)
                return BadRequest("A legutolsó aláírás bejegyzés 'Megtagadva'. Jegy nem adható.");

            var grade = new Grade
            {
                SubjectId = dto.SubjectId,
                StudentId = dto.StudentId,
                Semester = dto.Semester,
                GradeValue = dto.GradeValue,
                Time = dto.Time
            };

            _unitOfWork.GradeRepository.Add(grade);
            await _unitOfWork.SaveAsync();
            var savedGrade = await _unitOfWork.GradeRepository.GetAllAsync(g => g.Id == grade.Id, new[] { "Student", "Subject" });

            return CreatedAtAction(nameof(GetGrades), new { studentId = grade.StudentId, subjectId = grade.SubjectId, semester = grade.Semester }, _mapper.Map<GradeGetDTO>(savedGrade.First()));
        }

        // DELETE: api/Grade/5
        [HttpDelete("{gradeId}")]
        public async Task<IActionResult> DeleteGrade(int gradeId)
        {
            var grade = await _unitOfWork.GradeRepository.FindByIdAsync(gradeId);
            if (grade == null) return NotFound();

            _unitOfWork.GradeRepository.Delete(grade);
            await _unitOfWork.SaveAsync();
            return NoContent();
        }

        
        [HttpPatch("{gradeId}")]
        public async Task<IActionResult> PatchGrade(int gradeId, [FromBody] int newGrade)
        {
            if (newGrade < 1 || newGrade > 5) return BadRequest("Érvénytelen jegy.");
        
            var grade = await _unitOfWork.GradeRepository.FindByIdAsync(gradeId);
            if (grade == null) return NotFound();

            grade.GradeValue = newGrade;
            await _unitOfWork.SaveAsync();
            return NoContent();
        }
        
        
        
    }
}

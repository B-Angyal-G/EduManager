using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduManager.Data;
using EduManager.DTO.CourseDTO;
using EduManager.DTO.ScheduleDTO;
using EduManager.DTO.UserDto;
using EduManager.Entities;
using EduManager.Repository;

namespace EduManager.Controllers
{
    // [Route("api/[controller]")]
    [Route("api/courses")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: api/Course
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseGetDTO>>> GetCourses()
        {
            var includes = new[] { "Subject", "Teachers" };
            var courses = await _unitOfWork.CourseRepository.GetAllAsync(null, includes);
            return Ok(_mapper.Map<List<CourseGetDTO>>(courses));
        }

        
        // GET: api/Course/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseGetDTO>> GetCourse(int id)
        {
            var includes = new[] { "Subject", "Teachers" };
        
            // Szűrés ID alapján az include-okkal együtt
            var courses = await _unitOfWork.CourseRepository.GetAllAsync(c => c.Id == id, includes);
            var course = courses.FirstOrDefault();

            if (course == null) return NotFound();

            return Ok(_mapper.Map<CourseGetDTO>(course));
        }

        
        // PUT: api/Course/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(int id, CourseUpdateDTO dto)
        {
            if (id != dto.Id) return BadRequest("Az ID-k nem egyeznek.");

            var course = await _unitOfWork.CourseRepository.FindByIdAsync(id);
            if (course == null) return NotFound();
            
           _mapper.Map(dto, course);

            _unitOfWork.CourseRepository.Update(course);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }

        
        // POST: api/Course
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CourseGetDTO>> PostCourse(CourseCreateDTO dto)
        {
            // 1. Tantárgy kikeresése kód alapján
            var subjects = await _unitOfWork.SubjectRepository.GetAllAsync(s => s.Code == dto.SubjectCode);
            var subject = subjects.FirstOrDefault();

            if (subject == null) return BadRequest("A megadott tantárgykód nem létezik.");
            if (!subject.IsActive) return BadRequest("Inaktív tantárgyhoz nem hirdethető kurzus.");

            // 2. Oktatók kikeresése ID lista alapján
            var teachers = await _unitOfWork.UserRepository.GetAllAsync(u => 
                dto.TeacherIds.Contains(u.Id) && u.Role == UserRole.Teacher);

            if (teachers.Count != dto.TeacherIds.Count)
                return BadRequest("Egy vagy több oktató érvénytelen vagy nem tanár szerepkörű.");

            // 3. Mappolás és mentés
            var course = _mapper.Map<Course>(dto);
            course.SubjectId = subject.Id;
            course.Teachers = teachers;

            _unitOfWork.CourseRepository.Add(course);
            await _unitOfWork.SaveAsync();

            // A visszatérő DTO-hoz újra leképezzük (hogy a SubjectName is benne legyen)
            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, _mapper.Map<CourseGetDTO>(course));
        }

        
        // DELETE: api/Course/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            // Ellenőrzés: vannak-e rajta hallgatók?
            var includes = new[] { "Students" };
            var courses = await _unitOfWork.CourseRepository.GetAllAsync(c => c.Id == id, includes);
            var course = courses.FirstOrDefault();

            if (course == null) return NotFound();

            if (course.Students != null && course.Students.Any())
            {
                return BadRequest("A kurzus nem törölhető, mert vannak rá jelentkezett hallgatók.");
            }

            _unitOfWork.CourseRepository.Delete(course);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }
        
        
        
        // <=== JELENTKEZÉSEK ===>
        [HttpPost("change")]
        public async Task<IActionResult> ChangeCourse(CourseChangeDTO dto)
        {
            var includes = new[] { "Students" };
            var fromCourse = (await _unitOfWork.CourseRepository.GetAllAsync(c => c.Id == dto.FromCourseId, includes)).FirstOrDefault();
            var toCourse = (await _unitOfWork.CourseRepository.GetAllAsync(c => c.Id == dto.ToCourseId, includes)).FirstOrDefault();
            var student = await _unitOfWork.UserRepository.FindByIdAsync(dto.StudentId);

            if (fromCourse == null || toCourse == null || student == null) 
                return NotFound("Valamelyik adat érvénytelen.");

            // Validálás: Ugyanaz a tárgy és típus?
            if (fromCourse.SubjectId != toCourse.SubjectId || fromCourse.Type != toCourse.Type)
                return BadRequest("Csak ugyanazon tárgyon belül, azonos típusú kurzusra lehet átjelentkezni.");

            // Validálás: Van hely a célkurzuson?
            if (toCourse.Students.Count >= toCourse.MaxStudents)
                return BadRequest("A célkurzus betelt.");

            // Validálás: Tagozat megfelel? (A tagozatellenőrzés ugyanaz, mint a regisztrációnál)
            // ... (ide jöhet a tagozat ellenőrzése, ha nagyon precíz akarsz lenni)

            fromCourse.Students.Remove(student);
            toCourse.Students.Add(student);

            _unitOfWork.CourseRepository.Update(fromCourse);
            _unitOfWork.CourseRepository.Update(toCourse);
            await _unitOfWork.SaveAsync();

            return Ok(new { message = "Sikeres átjelentkezés!" });
        }
        
        
        [HttpGet("{courseId}/students")]
        public async Task<ActionResult<IEnumerable<UserGetDTO>>> GetCourseStudents(int courseId)
        {
            var includes = new[] { "Students" };
            var courses = await _unitOfWork.CourseRepository.GetAllAsync(c => c.Id == courseId, includes);
            var course = courses.FirstOrDefault();
    
            if (course == null) return NotFound();

            return Ok(_mapper.Map<List<UserGetDTO>>(course.Students));
        }
        
        
        
        // <=== ÓRAREND ===>
        /// <summary>
        /// Órarendi időpontok megadása egy kurzushoz.
        /// </summary>
        /// <remarks>
        /// Példa heti órára (Type: "weekly"): 14 héten át generál időpontokat a megadott naptól kezdve.
        /// Példa tömbösített órára (Type: "blocked"): A listában megadott konkrét időpontokat menti el.
        /// </remarks>
        [HttpPost("{courseId}/schedule")]
        public async Task<IActionResult> SetSchedule(int courseId, ScheduleCreateDTO dto)
        {
            var course = await _unitOfWork.CourseRepository.FindByIdAsync(courseId);
            if (course == null) return NotFound();

            var schedules = new List<CourseSchedule>();

            if (dto.Type == "weekly")
            {
                if (!dto.FirstDate.HasValue || !dto.StartTime.HasValue || !dto.EndTime.HasValue || !dto.DayOfWeek.HasValue)
                    return BadRequest("Heti rend esetén minden időpont adat kötelező.");

                // 14 héten keresztül generálunk
                for (int i = 0; i < 14; i++)
                {
                    var date = dto.FirstDate.Value.AddDays(i * 7);
                    schedules.Add(new CourseSchedule
                    {
                        CourseId = courseId,
                        StartTime = date.Date.Add(dto.StartTime.Value),
                        EndTime = date.Date.Add(dto.EndTime.Value)
                    });
                }
            }
            else if (dto.Type == "blocked" && dto.Occurrences != null)
            {
                foreach (var occ in dto.Occurrences)
                {
                    schedules.Add(new CourseSchedule { CourseId = courseId, StartTime = occ.Start, EndTime = occ.End });
                }
            }

            foreach (var s in schedules) _unitOfWork.CourseScheduleRepository.Add(s);
            await _unitOfWork.SaveAsync();

            return Ok(new { message = $"{schedules.Count} időpont rögzítve." });
        }
    }
}

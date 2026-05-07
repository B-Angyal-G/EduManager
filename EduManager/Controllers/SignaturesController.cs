using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduManager.Data;
using EduManager.DTO.SignatureDTO;
using EduManager.Entities;
using EduManager.Repository;

namespace EduManager.Controllers
{
    // [Route("api/[controller]")]
    [Route("api/signatures")]
    [ApiController]
    public class SignaturesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SignaturesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: api/Signatures
        // TESZTELÉSHEZ
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SignatureGetDTO>>> GetSignatures(int subjectId, int studentId, string semester)
        {
            var signatures = await _unitOfWork.SignatureRepository.GetAllAsync(
                s => s.SubjectId == subjectId && s.StudentId == studentId && s.Semester == semester,
                new[] { "Student", "Subject" });

            if (!signatures.Any()) return NotFound("Nincs aláírás bejegyzés ehhez a hallgatóhoz.");

            return Ok(_mapper.Map<IEnumerable<SignatureGetDTO>>(signatures));
        }

        // GET: api/Signatures/5
        /*[HttpGet("{id}")]
        public async Task<ActionResult<Signature>> GetSignature(int id)
        {
            var signature = await _context.Signatures.FindAsync(id);

            if (signature == null)
            {
                return NotFound();
            }

            return signature;
        }*/

        // PUT: api/Signatures/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /*[HttpPut("{id}")]
        public async Task<IActionResult> PutSignature(int id, Signature signature)
        {
            if (id != signature.Id)
            {
                return BadRequest();
            }

            _context.Entry(signature).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SignatureExists(id))
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

        // POST: api/Signatures
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostSignature(SignatureCreateDTO dto)
        {
            // 1. Validálás: Hallgató rajta van-e a tárgyon
            var enrolled = await _unitOfWork.CourseRepository.GetAllAsync(
                c => c.SubjectId == dto.SubjectId && c.Semester == dto.Semester && c.Students.Any(s => s.Id == dto.StudentId),
                new[] { "Students" });

            if (!enrolled.Any()) return BadRequest("A hallgató nem vette fel ezt a tárgyat.");

            // 2. Időpont ellenőrzés (a GradesController-nél használt logikával)
            bool isValidDate = dto.Semester.EndsWith("1") 
                ? (dto.Time.Month >= 9 || dto.Time.Month <= 1)
                : (dto.Time.Month >= 2 && dto.Time.Month <= 7);
            if (!isValidDate) return BadRequest("Az időpont nem esik a félévbe.");

            var sig = new Signature {
                SubjectId = dto.SubjectId,
                StudentId = dto.StudentId,
                Semester = dto.Semester,
                IsSigned = dto.Value,
                Time = dto.Time
            };

            _unitOfWork.SignatureRepository.Add(sig);
            await _unitOfWork.SaveAsync();
            return Ok("Aláírás bejegyzés rögzítve.");
        }

        // DELETE: api/Signatures/5
        [HttpDelete("{signatureId}")]
        public async Task<IActionResult> DeleteSignature(int signatureId)
        {
            var sig = await _unitOfWork.SignatureRepository.FindByIdAsync(signatureId);
            if (sig == null) return NotFound();

            var hasGrade = (await _unitOfWork.GradeRepository.GetAllAsync(
                g => g.StudentId == sig.StudentId && g.SubjectId == sig.SubjectId && g.Semester == sig.Semester)).Any();

            if (hasGrade) return BadRequest("Az aláírás nem törölhető, mert a hallgatónak már van beírt jegye.");

            _unitOfWork.SignatureRepository.Delete(sig);
            await _unitOfWork.SaveAsync();
            return NoContent();
        }

        [HttpPatch("{signatureId}")]
        public async Task<IActionResult> PatchSignature(int signatureId, [FromBody] bool newValue)
        {
            var sig = await _unitOfWork.SignatureRepository.FindByIdAsync(signatureId);
            if (sig == null) return NotFound();

            // Validálás: Van-e már jegye?
            var hasGrade = (await _unitOfWork.GradeRepository.GetAllAsync(
                g => g.StudentId == sig.StudentId && g.SubjectId == sig.SubjectId && g.Semester == sig.Semester)).Any();

            if (hasGrade) return BadRequest("Az aláírás nem módosítható, mert a hallgatónak már van beírt jegye.");

            sig.IsSigned = newValue;
            await _unitOfWork.SaveAsync();
            return NoContent();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduManager.Data;
using EduManager.DTO.UserDto;
using EduManager.Entities;
using EduManager.Repository;

namespace EduManager.Controllers
{
    // [Route("api/[controller]")]
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserGetDTO>>> GetUsers()
        {
            var users = await _unitOfWork.UserRepository.GetAllAsync(u => u.IsActive);

            var dtos = _mapper.Map<List<UserGetDTO>>(users);

            return Ok(dtos);
        }


        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserGetDTO>> GetUser(int id)
        {
            var user = await _unitOfWork.UserRepository.FindByIdAsync(id);

            if (user == null || !user.IsActive) return NotFound();

            return Ok(_mapper.Map<UserGetDTO>(user));
        }


        // PUT: api/User/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserUpdateDTO dto)
        {
            if (id != dto.Id) return BadRequest("Az ID-k nem egyeznek.");

            var user = await _unitOfWork.UserRepository.FindByIdAsync(id);
            if (user == null) return NotFound();

            // E-mail ellenőrzés: ha változott, ne legyen foglalt
            if (user.Email != dto.Email)
            {
                var emailExists = await _unitOfWork.UserRepository.GetAllAsync(u => u.Email == dto.Email);
                if (emailExists.Any()) return BadRequest("Az új e-mail cím már foglalt.");
            }

            _mapper.Map(dto, user);

            if (user.Role == UserRole.Student)
            {
                if (dto.StudyMode == (int)StudyMode.None) return BadRequest("Hallgató munkarendje nem lehet None.");
            }
            else
            {
                user.StudyMode = StudyMode.None;
            }

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }


        // POST: api/User
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserGetDTO>> PostUser(UserCreateDTO dto)
        {
            // E-mail egyediség ellenőrzése
            var existing = await _unitOfWork.UserRepository.GetAllAsync(u => u.Email == dto.Email);
            if (existing.Any()) return BadRequest("Ez az e-mail cím már foglalt.");

            var user = _mapper.Map<User>(dto);

            if (user.Role == UserRole.Student)
            {
                if (user.StudyMode == StudyMode.None) return BadRequest("Hallgató esetén a munkarend kötelező.");
            }
            else
            {
                user.StudyMode = StudyMode.None;
            }

            user.IsActive = true;

            _unitOfWork.UserRepository.Add(user);
            await _unitOfWork.SaveAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, _mapper.Map<UserGetDTO>(user));
        }


        // POST: api/users/5/deactivate
        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var user = await _unitOfWork.UserRepository.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = false;
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();

            return Ok(new { message = "Felhasználó inaktiválva." });
        }

        
        // POST: api/users/5/reactivate
        [HttpPost("{id}/reactivate")]
        public async Task<IActionResult> ReactivateUser(int id)
        {
            var user = await _unitOfWork.UserRepository.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = true;
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();

            return Ok(new { message = "Felhasználó újra aktiválva." });
        }
    }
}

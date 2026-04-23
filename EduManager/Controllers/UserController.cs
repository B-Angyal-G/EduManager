using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduManager.Data;
using EduManager.DTO.UserDto;
using EduManager.Entities;

namespace EduManager.Controllers
{
    // [Route("api/[controller]")]
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly EduDbContext _context;

        public UserController(EduDbContext context)
        {
            _context = context;
        }

        
        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserGetDTO>>> GetUsers()
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new UserGetDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    StudyMode = u.StudyMode.ToString(),
                    IsActive = u.IsActive
                })
                .ToListAsync();

            // DTO előtti
            // Csak az aktívakat kérjük le
            // return await _context.Users.Where(u => u.IsActive).ToListAsync();
        }

        
        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserGetDTO>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UserGetDTO()
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                StudyMode = user.StudyMode.ToString(),
                IsActive = user.IsActive
            };

            return userDto;
        }

        
        // PUT: api/User/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserUpdateDTO dto)
        {
            // 1. Alapvető ellenőrzés: az URL-ben lévő ID egyezik-e a JSON-ben lévővel?
            if (id != dto.Id)
            {
                return BadRequest("Az ID-k nem egyeznek.");
            }

            // 2. Megkeressük az EREDETI felhasználót az adatbázisban
            var existingUser = await _context.Users.FindAsync(id);

            if (existingUser == null)
            {
                return NotFound();
            }

            // 3. E-mail ellenőrzése: Ha megváltoztatta az e-mailt, nézzük meg, nem foglalt-e?
            if (existingUser.Email != dto.Email && await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("Ez az e-mail cím már foglalt.");
            }
            
            // 4. Munkarend beállítása hallgató feltételnek megfelelően
            if (existingUser.Role == UserRole.Student)
            {
                // Ha hallgatóról van szó, kötelező a Nappali vagy Levelező
                if (dto.StudyMode == (int)StudyMode.None)
                {
                    return BadRequest("Hallgató munkarendje nem állítható 'None' értékre!");
                }
                existingUser.StudyMode = (StudyMode)dto.StudyMode;
            }
            else
            {
                // Ha oktató vagy ügyintéző, kényszerítjük a None-t, bármit is küldtek a JSON-ben
                existingUser.StudyMode = StudyMode.None;
            }
            
            // 5. Frissítjük a megengedett mezőket
            existingUser.Username = dto.Username;
            existingUser.Email = dto.Email;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        
        // POST: api/User
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(UserCreateDTO dto)
        {
            // 1. Ellenőrzés: létezik-e már ilyen e-mail? (Specifikáció!)
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("Ez az e-mail cím már regisztrálva van.");
            }
            
            StudyMode finalStudyMode;

            if (dto.Role == (int)UserRole.Student)
            {
                // Ha diák, de None-t küldtek vagy érvénytelen értéket
                if (dto.StudyMode == (int)StudyMode.None)
                {
                    return BadRequest("Hallgató esetén kötelező a nappali vagy levelező munkarend!");
                }
                finalStudyMode = (StudyMode)dto.StudyMode;
            }
            else
            {
                // Ha nem diák, kényszerítjük a None értéket, 
                // bármit is küldött a kliens a JSON-ben.
                finalStudyMode = StudyMode.None;
            }
            
            // 2. Mapping: DTO -> Entity
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = dto.Password,
                Role = (UserRole)dto.Role,
                StudyMode = finalStudyMode,
                IsActive = true
            };

            // 3. Mentés
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 4. Válasz DTO formátumban
            var responseDto = new UserGetDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                StudyMode = user.StudyMode.ToString(),
                IsActive = user.IsActive
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, responseDto);
        }

        
        // POST: api/users/5/deactivate
        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!user.IsActive)
            {
                return BadRequest("A felhasználó már alapból inaktív.");
            }

            user.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User {id} deactivated successfully." });
        }

        
        // POST: api/users/5/reactivate
        [HttpPost("{id}/reactivate")]
        public async Task<IActionResult> ReactivateUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.IsActive)
            {
                return BadRequest("A felhasználó már alapból aktív.");
            }

            user.IsActive = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User {id} reactivated successfully." });
        }


        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}

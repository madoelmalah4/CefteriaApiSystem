using CafeteriaProject.Models; 
using CafeteriaProject.Models.Data;
using CafeteriaProject.Models.DTO_s;
using CafeteriaProject.Services; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 

namespace CafeteriaProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context; 
        private readonly AuthService _authService;

        public AuthController(AppDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Username or Email already exists.");
            }

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = _authService.HashPassword(request.Password) 
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            var token = _authService.GenerateJwtToken(newUser);
            newUser.Token = token; 

            return CreatedAtAction(nameof(Register), new { isSuccess = true , token = newUser.Token }); 
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            
            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            if (!_authService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials.");
            }

            var token = _authService.GenerateJwtToken(user);

            user.Token = token;

            return Ok(new { isSuccess = true, Token = token, UserId = user.Id, Username = user.Username,});
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                // Efficient DB query (requires Token to be mapped to DB)
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Token == token);

                if (user != null)
                {
                    // Expire the token
                    user.Token = null;

                    await _context.SaveChangesAsync();
                }
            }

            return Ok(new { isSuccess = true , message = "Logged out successfully. Token expired." });
        }




    }

}
// Services/AuthService.cs (create a Services folder)
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration; // For IConfiguration
using CafeteriaProject.Models; // Your User model
using BCrypt.Net; // For BCrypt password hashing

namespace CafeteriaProject.Services
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _jwtDurationInMinutes;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
            _jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT:Key not configured.");
            _jwtIssuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT:Issuer not configured.");
            _jwtAudience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT:Audience not configured.");
            _jwtDurationInMinutes = int.Parse(_configuration["Jwt:DurationInMinutes"] ?? "60");
        }

        // --- Password Hashing and Verification ---
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        // ----------------------------------------

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Unique user ID
                new Claim(ClaimTypes.Name, user.Username),
                // Add roles here if your User model has a role property, e.g.:
                // new Claim(ClaimTypes.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtDurationInMinutes),
                Issuer = _jwtIssuer,
                Audience = _jwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
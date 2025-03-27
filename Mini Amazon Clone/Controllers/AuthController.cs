using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Mini_Amazon_Clone.Data;
using Mini_Amazon_Clone.DTO;
using Mini_Amazon_Clone.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;

namespace Mini_Amazon_Clone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult RegisterAsync([FromBody] UserDto userDto)
        {
            if (_context.Users.Any(u => u.Email == userDto.Email))
                return BadRequest(new { message = "Email already exists." });
            User user = new User();
            user.Email = userDto.Email;
            user.Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            user.Name = userDto.Name;
            user.Role = userDto.Role;

            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok(new
            {
                msg = "Created User Successfully",
                data = new
                {
                    name = user.Name
                }
            });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // Retrieve the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            // Verify the password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                return Unauthorized("Invalid credentials");
            }

            var token = GenerateJwtToken(user);
            return Ok(new
            {
                msg = "Loggedin Successfully",
                Token = token
            });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is not configured in appsettings.json.");
            }

            var claims = new List<Claim>
            {
                new Claim("UserId", user.UserID.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // Add custom claims for Admins (for demo purposes, assume Admin users get these claims)
            if (user.Role == "Admin")
            {
                claims.Add(new Claim("CanViewOrders", "true"));
                claims.Add(new Claim("CanRefundOrders", "true"));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}

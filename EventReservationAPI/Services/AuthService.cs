using EventReservationAPI.Data;
using EventReservationAPI.Entities;
using EventReservationAPI.Settings;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventReservationAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthService> _logger;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            AppDbContext context, 
            ILogger<AuthService> logger, 
            IOptions<JwtSettings> jwtOptions)
        {
            _context = context;
            _logger = logger;
            _jwtSettings = jwtOptions.Value;
        }

        public async Task<User?> RegisterUserAsync(UserRegisterDto dto)
        {
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);

            if (emailExists)
            {
                _logger.LogWarning("User with email {Email} already exists.", dto.Email);
                return null;
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Email = dto.Email,
                Name = dto.Name,
                PasswordHash = passwordHash,
                Role = User.Roles.User
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered successfully with ID: {UserId}", user.Id);
            return user;
        }

        public async Task<string?> LoginUserAsync(UserLoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid login attempt for email: {Email}", dto.Email);
                return null;
            }

            _logger.LogInformation("User logged in successfully with ID: {UserId}", user.Id);
            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

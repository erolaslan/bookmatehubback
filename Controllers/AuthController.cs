using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookMateHub.Data;
using BookMateHub.Models;
using BookMateHub.DTOs;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly BookMateHubDbContext _context;
    private readonly string _key = "YourSecretKeyGoesHereErolAslan12345678"; // JWT gizli anahtarı

    public AuthController(BookMateHubDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(User user)
    {
        if (_context.Users.Any(u => u.Username == user.Username))
        {
            return BadRequest("Kullanıcı adı zaten mevcut.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash); // Şifreyi hashleyin
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("Kullanıcı başarıyla kaydedildi.");
    }

    [HttpPost("login")]
    public IActionResult Login(UserLoginDto loginDto)
    {
        var existingUser = _context.Users.FirstOrDefault(u => u.Username == loginDto.Username);
        if (existingUser == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, existingUser.PasswordHash))
        {
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");
        }

        // JWT token oluşturma
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, existingUser.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new { Token = tokenString });
    }
}

using Microsoft.AspNetCore.Mvc;
using FinTrackWebApi.Dtos;
using FinTrackWebApi.Models;
using FinTrackWebApi.Data;
using Microsoft.EntityFrameworkCore;
using FinTrackWebApi.Security;
using FinTrackWebApi.Services.EmailService;

namespace FinTrackWebApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MyDataContext _context;

        private readonly IConfiguration _configuration;

        public AuthController(MyDataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Kullanıcı kayıt işlemi yapılıyor.
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest("Email already exists");
            }

            // Kayıt için gerekli sınıftan örnek oluşturuluyor.
            var user = new UserModel
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                ProfilePicture = registerDto.ProfilePicture
            };

            EmailSender emailSender = new EmailSender(_configuration); // Email gönderimi için sınıf örneği oluşturuluyor.
            await emailSender.SendEmailAsync(registerDto.Email, "Welcome to FinTrack", "<p>You have successfully registered to FinTrack.</p>"); // Kullanıcıya hoş geldin emaili gönderiliyor.

            await _context.Users.AddAsync(user); // Kullanıcı veritabanına ekleniyor.
            await _context.SaveChangesAsync(); // Değişiklikler kaydediliyor.
            
            return Ok(new { user.UserId, user.Username, user.Email, user.ProfilePicture }); // Başarılı kayıt sonrası kullanıcı bilgileri döndürülüyor.
        }

        // Kullanıcı giriş işlemi yapılıyor.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            // Kullanıcı bulunamazsa veya şifre yanlışsa hata döndürülüyor.
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials");
            }

            Token token = TokenHandler.CreateToken(_configuration); // JWT token oluşturuluyor.

            // Başarılı giriş sonrası kullanıcı bilgileri döndürülüyor.
            return Ok(new { user.UserId, user.Username, user.Email, user.ProfilePicture, token});
        }
    }
}

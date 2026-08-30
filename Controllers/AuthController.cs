using System.Security.Cryptography;
using System.Text;
using LoginApi.Data;
using LoginApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoginApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "กรุณากรอกชื่อผู้ใช้และรหัสผ่าน" });
        }

        var exists = await _context.Users.AnyAsync(u => u.Username == request.Username);
        if (exists)
        {
            return Conflict(new { message = "ชื่อผู้ใช้นี้ถูกใช้งานแล้ว" });
        }

        var user = new User
        {
            Username = request.Username.Trim(),
            PasswordHash = HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "สมัครสมาชิกสำเร็จ", username = user.Username });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "กรุณากรอกชื่อผู้ใช้และรหัสผ่าน" });
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง" });
        }

        return Ok(new { message = "เข้าสู่ระบบสำเร็จ", username = user.Username });
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    private static bool VerifyPassword(string password, string expectedHash)
    {
        return string.Equals(HashPassword(password), expectedHash, StringComparison.OrdinalIgnoreCase);
    }
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
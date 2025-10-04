using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHUYENXEBACAI.Infrastructure.EF;
using BCrypt.Net;
using CHUYENXEBACAI.Auth;

namespace CHUYENXEBACAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db, IJwtService jwt) : ControllerBase
{
    public record RegisterDto(string Email, string Password, string FullName, string? Phone);
    public record LoginDto(string Email, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await db.users.AnyAsync(u => u.email == dto.Email))
            return Conflict("Email already exists.");

        var u = new Infrastructure.EF.Models.user
        {
            id = Guid.NewGuid(),
            email = dto.Email.Trim().ToLowerInvariant(),
            password_hash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            full_name = dto.FullName,
            phone = dto.Phone,
            Status = CHUYENXEBACAI.Domain.UserStatus.Active
        };
        db.users.Add(u);
        await db.SaveChangesAsync();
        return Ok(new { u.id, u.email });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var u = await db.users.Include(x => x.roles).FirstOrDefaultAsync(x => x.email == dto.Email);
        if (u is null || !BCrypt.Net.BCrypt.Verify(dto.Password, u.password_hash)) return Unauthorized();

        var roleCodes = u.roles.Select(r => r.code);
        var token = jwt.CreateToken(u.id, u.email, roleCodes);
        return Ok(new { access_token = token });
    }
}

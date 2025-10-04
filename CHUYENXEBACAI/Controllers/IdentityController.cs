using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHUYENXEBACAI.Infrastructure.EF;
using CHUYENXEBACAI.Domain;

namespace CHUYENXEBACAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdentityController(AppDbContext db) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> Users(int page = 1, int pageSize = 20, string? q = null)
    {
        var query = db.users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(u => EF.Functions.ILike(u.email, $"%{q}%") ||
                                     EF.Functions.ILike(u.full_name!, $"%{q}%"));
        var total = await query.CountAsync();
        var data = await query.OrderByDescending(u => u.created_at)
                              .Skip((page - 1) * pageSize).Take(pageSize)
                              .Select(u => new { u.id, u.email, u.full_name, u.phone, Status = u.Status, u.created_at })
                              .ToListAsync();
        return Ok(new { total, data });
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        if (await db.users.AnyAsync(x => x.email == dto.Email))
            return Conflict("Email already exists.");

        var u = new Infrastructure.EF.Models.user
        {
            id = Guid.NewGuid(),
            email = dto.Email,
            password_hash = dto.PasswordHash,
            full_name = dto.FullName,
            phone = dto.Phone,
            Status = UserStatus.Active
        };
        db.users.Add(u);
        await db.SaveChangesAsync();
        return Ok(new { u.id, u.email });
    }

    [HttpGet("roles")]
    public async Task<IActionResult> Roles() => Ok(await db.roles.AsNoTracking().OrderBy(r => r.code).ToListAsync());

    [HttpPost("roles/assign")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        var role = await db.roles.FirstOrDefaultAsync(r => r.code == dto.RoleCode);
        if (role is null) return NotFound("Role not found");

        var user = await db.users.Include(u => u.roles).FirstOrDefaultAsync(u => u.id == dto.UserId);
        if (user is null) return NotFound("User not found");

        if (!user.roles.Any(r => r.id == role.id))
            user.roles.Add(role);

        await db.SaveChangesAsync();
        return NoContent();
    }
}

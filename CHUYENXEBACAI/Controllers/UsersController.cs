using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHUYENXEBACAI.Infrastructure.EF;
using CHUYENXEBACAI.Domain;
using Models = CHUYENXEBACAI.Infrastructure.EF.Models;

namespace CHUYENXEBACAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? q = null)
    {
        var query = db.users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var k = q.Trim().ToLower();
            query = query.Where(x => x.email.ToLower().Contains(k) || x.full_name.ToLower().Contains(k));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(x => x.created_at)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { total, data });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detail(Guid id)
    {
        var u = await db.users.AsNoTracking()
            .Include(x => x.roles)
            .FirstOrDefaultAsync(x => x.id == id);
        return u is null ? NotFound() : Ok(u);
    }

    // Lưu ý bảo mật: DTO CreateUserDto của bạn có field PasswordHash.
    // Ở đây minh họa 2 cách:
    // 1) Tạo user qua AuthController/register (khuyên dùng)
    // 2) Cho phép tạo trực tiếp (dùng PasswordHash đã hash sẵn)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (await db.users.AnyAsync(x => x.email == dto.Email)) return Conflict("Email already exists.");

        var u = new Models.user
        {
            id = Guid.NewGuid(),
            email = dto.Email.Trim().ToLowerInvariant(),
            password_hash = dto.PasswordHash, // ASSUME đã hash sẵn. (Hoặc tự hash tại đây nếu muốn)
            full_name = dto.FullName,
            phone = dto.Phone,
            Status = UserStatus.Active
        };
        db.users.Add(u);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Detail), new { id = u.id }, u);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateUserDto dto)
    {
        var u = await db.users.FirstOrDefaultAsync(x => x.id == id);
        if (u is null) return NotFound();

        u.email = dto.Email.Trim().ToLowerInvariant();
        u.password_hash = dto.PasswordHash; // hoặc giữ nguyên nếu null
        u.full_name = dto.FullName;
        u.phone = dto.Phone;

        await db.SaveChangesAsync();
        return Ok(u);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UserStatus status)
    {
        var u = await db.users.FirstOrDefaultAsync(x => x.id == id);
        if (u is null) return NotFound();
        u.Status = status;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var u = await db.users.FirstOrDefaultAsync(x => x.id == id);
        if (u is null) return NotFound();
        db.users.Remove(u);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/roles")]
    public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleDto dto)
    {
        var u = await db.users.Include(x => x.roles).FirstOrDefaultAsync(x => x.id == id);
        if (u is null) return NotFound("User not found.");
        var role = await db.roles.FirstOrDefaultAsync(r => r.code == dto.RoleCode);
        if (role is null) return NotFound("Role not found.");

        if (!u.roles.Any(r => r.id == role.id))
            u.roles.Add(role);

        await db.SaveChangesAsync();
        return Ok(u.roles.Select(r => new { r.id, r.code, r.name }));
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHUYENXEBACAI.Infrastructure.EF;
using CHUYENXEBACAI.Domain;

namespace CHUYENXEBACAI.Controllers;

[ApiController]
[Route("api/volunteers/registrations")]
public class VolunteerRegistrationsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? campaignId = null, [FromQuery] Guid? sessionId = null,
                                          [FromQuery] RegistrationStatus? status = null,
                                          [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var q = db.volunteer_registrations.AsNoTracking();
        if (campaignId is not null) q = q.Where(x => x.campaign_id == campaignId);
        if (sessionId is not null) q = q.Where(x => x.session_id == sessionId);
        if (status is not null) q = q.Where(x => x.Status == status);

        var total = await q.CountAsync();
        var data = await q.OrderByDescending(x => x.applied_at)
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();
        return Ok(new { total, data });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detail(Guid id)
    {
        var r = await db.volunteer_registrations.AsNoTracking().FirstOrDefaultAsync(x => x.id == id);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var r = await db.volunteer_registrations.FirstOrDefaultAsync(x => x.id == id);
        if (r is null) return NotFound();
        db.volunteer_registrations.Remove(r);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

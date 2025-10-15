using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHUYENXEBACAI.Infrastructure.EF;
using CHUYENXEBACAI.Domain;
using Models = CHUYENXEBACAI.Infrastructure.EF.Models;

namespace CHUYENXEBACAI.Controllers;

[ApiController]
[Route("api/volunteers/applications")]
public class VolunteerApplicationsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
                                          [FromQuery] RegistrationStatus? regStatus = null,
                                          [FromQuery] AppReviewStatus? status = null)
    {
        var q = db.volunteer_applications.AsNoTracking();
        if (status is not null) q = q.Where(x => x.Status == status);
        var total = await q.CountAsync();
        var data = await q.OrderByDescending(x => x.created_at)
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();
        return Ok(new { total, data });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detail(Guid id)
    {
        var app = await db.volunteer_applications.AsNoTracking().FirstOrDefaultAsync(x => x.id == id);
        return app is null ? NotFound() : Ok(app);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SubmitVolunteerAppDto dto)
    {
        var app = new Models.volunteer_application
        {
            id = Guid.NewGuid(),
            user_id = dto.UserId,
            skills = dto.Skills,
            availability = dto.Availability,
            Status = AppReviewStatus.PendingReview
        };
        db.volunteer_applications.Add(app);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Detail), new { id = app.id }, app);
    }

    [HttpPost("{id:guid}/review")]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewVolunteerAppDto dto)
    {
        var app = await db.volunteer_applications.FirstOrDefaultAsync(x => x.id == id);
        if (app is null) return NotFound();
        app.Status = dto.Status;
        app.reject_reason = dto.RejectReason;
        app.reviewed_at = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SubmitVolunteerAppDto dto)
    {
        var app = await db.volunteer_applications.FirstOrDefaultAsync(x => x.id == id);
        if (app is null) return NotFound();
        app.skills = dto.Skills;
        app.availability = dto.Availability;
        await db.SaveChangesAsync();
        return Ok(app);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var app = await db.volunteer_applications.FirstOrDefaultAsync(x => x.id == id);
        if (app is null) return NotFound();
        db.volunteer_applications.Remove(app);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

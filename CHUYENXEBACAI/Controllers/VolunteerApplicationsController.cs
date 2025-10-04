using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHUYENXEBACAI.Infrastructure.EF;
using CHUYENXEBACAI.Domain;

namespace CHUYENXEBACAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VolunteerApplicationsController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitVolunteerAppDto dto)
    {
        var app = new Infrastructure.EF.Models.volunteer_application
        {
            id = Guid.NewGuid(),
            user_id = dto.UserId,
            skills = dto.Skills,
            availability = dto.Availability,
            Status = AppReviewStatus.PendingReview
        };
        db.volunteer_applications.Add(app);
        await db.SaveChangesAsync();
        return Ok(app);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] AppReviewStatus? status = null)
    {
        var q = db.volunteer_applications.AsNoTracking().AsQueryable();
        if (status.HasValue) q = q.Where(x => x.Status == status);
        var data = await q.OrderByDescending(x => x.created_at).Take(200).ToListAsync();
        return Ok(data);
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
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHUYENXEBACAI.Infrastructure.EF;
using CHUYENXEBACAI.Domain;
using Models = CHUYENXEBACAI.Infrastructure.EF.Models;

namespace CHUYENXEBACAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController(AppDbContext db) : ControllerBase
{
    [HttpGet("by-campaign/{campaignId:guid}")]
    public async Task<IActionResult> ByCampaign(Guid campaignId)
        => Ok(await db.sessions.AsNoTracking()
               .Where(s => s.campaign_id == campaignId)
               .OrderBy(s => s.session_date)
               .Select(s => new { s.id, s.title, s.session_date, s.Shift, s.quota, s.approved_volunteers, s.Status, s.place_name })
               .ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSessionDto dto)
    {
        var s = new Models.session
        {
            id = Guid.NewGuid(),
            campaign_id = dto.CampaignId,
            title = dto.Title,
            session_date = dto.SessionDate,
            Shift = dto.Shift,
            quota = dto.Quota,
            Status = dto.Status,
            place_name = dto.PlaceName,
            lat = dto.Lat,
            lng = dto.Lng,
            geo_radius_m = dto.GeoRadiusM
        };
        db.sessions.Add(s);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("uq_session_per_campaign") == true)
        { return Conflict("Trùng (campaign_id, session_date, shift)."); }

        return CreatedAtAction(nameof(ByCampaign), new { campaignId = dto.CampaignId }, s);
    }

    [HttpPost("registrations")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var reg = new Models.volunteer_registration
        {
            id = Guid.NewGuid(),
            user_id = dto.UserId,
            campaign_id = dto.CampaignId,
            session_id = dto.SessionId,
            Status = RegistrationStatus.Pending
        };
        db.volunteer_registrations.Add(reg);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (
            ex.InnerException?.Message.Contains("uq_reg_user_session") == true ||
            ex.InnerException?.Message.Contains("uq_reg_user_campaign_when_session_null") == true)
        { return Conflict("Đã đăng ký trước đó."); }
        return Ok(reg);
    }

    [HttpPost("registrations/{id:guid}/review")]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewRegistrationDto dto)
    {
        var reg = await db.volunteer_registrations.FirstOrDefaultAsync(x => x.id == id);
        if (reg is null) return NotFound();
        reg.Status = dto.Status;
        reg.reject_reason = dto.RejectReason;
        reg.reviewed_at = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }
}

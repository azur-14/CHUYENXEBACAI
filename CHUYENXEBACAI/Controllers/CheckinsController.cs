using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHUYENXEBACAI.Infrastructure.EF;

namespace CHUYENXEBACAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckinsController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCheckinDto dto)
    {
        var chk = new Infrastructure.EF.Models.checkin
        {
            id = Guid.NewGuid(),
            session_id = dto.SessionId,
            user_id = dto.UserId,
            Method = dto.Method, // enum property (PascalCase)
            Status = CHUYENXEBACAI.Domain.CheckinStatus.OnTime,
            lat = dto.Lat,
            lng = dto.Lng,
            evidence_note = dto.EvidenceNote
        };
        db.checkins.Add(chk);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("uq_checkin_once_per_session") == true)
        { return Conflict("User already checked-in for this session."); }
        return Ok(chk);
    }

    [HttpPost("media")]
    public async Task<IActionResult> AddMedia([FromBody] CreateMediaDto dto)
    {
        var m = new Infrastructure.EF.Models.media_asset
        {
            id = Guid.NewGuid(),
            campaign_id = dto.CampaignId,
            checkin_id = dto.CheckinId,
            url = dto.Url,
            public_id = dto.PublicId,
            thumb_url = dto.ThumbUrl,
            format = dto.Format
        };
        db.media_assets.Add(m);
        await db.SaveChangesAsync();
        return Ok(m);
    }
}

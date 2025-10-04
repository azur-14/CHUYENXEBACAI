using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHUYENXEBACAI.Infrastructure.EF;
using CHUYENXEBACAI.Domain;
using Models = CHUYENXEBACAI.Infrastructure.EF.Models;

namespace CHUYENXEBACAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CampaignsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] CampaignStatus? status, [FromQuery] string? q, [FromQuery] Pagination p)
    {
        var query = db.campaigns.AsNoTracking().AsQueryable();
        if (status.HasValue) query = query.Where(c => c.Status == status);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c => EF.Functions.ILike(c.title, $"%{q}%") || (c.location != null && EF.Functions.ILike(c.location, $"%{q}%")));
        var total = await query.CountAsync();
        var data = await query.OrderByDescending(c => c.created_at)
                              .Skip((p.Page - 1) * p.PageSize)
                              .Take(p.PageSize)
                              .Select(c => new { c.id, c.title, c.location, c.start_date, c.end_date, Status = c.Status, c.goal_amount })
                              .ToListAsync();
        return Ok(new { total, data });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
        => (await db.campaigns.AsNoTracking().FirstOrDefaultAsync(x => x.id == id)) is { } c ? Ok(c) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCampaignDto dto)
    {
        var creator = await db.users.AsNoTracking().FirstOrDefaultAsync();
        var c = new Models.campaign
        {
            id = Guid.NewGuid(),
            title = dto.Title,
            description = dto.Description,
            location = dto.Location,
            start_date = dto.StartDate,
            end_date = dto.EndDate,
            goal_amount = dto.GoalAmount,
            Status = CampaignStatus.Planning,
            created_by = creator?.id
        };
        db.campaigns.Add(c);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = c.id }, c);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateCampaignStatusDto dto)
    {
        var c = await db.campaigns.FirstOrDefaultAsync(x => x.id == id);
        if (c is null) return NotFound();
        c.Status = dto.Status;
        await db.SaveChangesAsync();
        return NoContent();
    }
}

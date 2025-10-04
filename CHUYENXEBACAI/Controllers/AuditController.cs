using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHUYENXEBACAI.Infrastructure.EF;

namespace CHUYENXEBACAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController(AppDbContext db) : ControllerBase
{
    [HttpGet("{entityType}/{entityId:guid}")]
    public async Task<IActionResult> Logs(string entityType, Guid entityId)
    {
        var logs = await db.change_logs.AsNoTracking()
            .Where(x => x.entity_type == entityType && x.entity_id == entityId)
            .OrderByDescending(x => x.changed_at)
            .Select(x => new { x.id, x.Action, x.changed_at, x.changed_by, x.before_data, x.after_data })
            .ToListAsync();
        return Ok(logs);
    }
}

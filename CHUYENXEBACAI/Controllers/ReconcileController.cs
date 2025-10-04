using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHUYENXEBACAI.Infrastructure.EF;

namespace CHUYENXEBACAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReconcileController(AppDbContext db) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] Guid? campaignId = null)
    {
        var q = db.v_reconcile_summaries.AsNoTracking().AsQueryable();
        if (campaignId.HasValue) q = q.Where(x => x.campaign_id == campaignId);
        return Ok(await q.OrderByDescending(x => x.fund_time).Take(500).ToListAsync());
    }

    [HttpPost("matches/{fundTxId:guid}/{bankStmtId:guid}/decide")]
    public async Task<IActionResult> Decide(Guid fundTxId, Guid bankStmtId, [FromBody] DecideMatchDto dto)
    {
        var m = await db.reconcile_matches.FirstOrDefaultAsync(x => x.fund_tx_id == fundTxId && x.bank_stmt_id == bankStmtId);
        if (m is null)
        {
            m = new Infrastructure.EF.Models.reconcile_match
            {
                id = Guid.NewGuid(),
                fund_tx_id = fundTxId,
                bank_stmt_id = bankStmtId,
                Decision = dto.Decision,
                decided_at = DateTime.UtcNow,
                note = dto.Note
            };
            db.reconcile_matches.Add(m);
        }
        else
        {
            m.Decision = dto.Decision;
            m.decided_at = DateTime.UtcNow;
            m.note = dto.Note;
        }
        await db.SaveChangesAsync();
        return Ok(m);
    }
}

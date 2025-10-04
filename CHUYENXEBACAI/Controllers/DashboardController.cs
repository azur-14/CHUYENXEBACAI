using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHUYENXEBACAI.Infrastructure.EF;

namespace CHUYENXEBACAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(AppDbContext db) : ControllerBase
{
    [HttpGet("campaign-progress")]
    public async Task<IActionResult> CampaignProgress()
        => Ok(await db.v_campaign_progresses.AsNoTracking().OrderBy(x => x.title).ToListAsync());

    [HttpGet("session-roster")]
    public async Task<IActionResult> SessionRoster()
        => Ok(await db.v_session_rosters.AsNoTracking().OrderBy(x => x.session_date).ToListAsync());

    [HttpGet("reconcile-summary")]
    public async Task<IActionResult> ReconcileSummary()
        => Ok(await db.v_reconcile_summaries.AsNoTracking().OrderByDescending(x => x.fund_time).Take(200).ToListAsync());
}

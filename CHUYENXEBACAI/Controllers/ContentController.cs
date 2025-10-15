using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHUYENXEBACAI.Infrastructure.EF;
using System.Linq;                     
using System.Collections.Generic;    

namespace CHUYENXEBACAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContentController(AppDbContext db) : ControllerBase
{
    [HttpGet("posts/by-campaign/{campaignId:guid}")]
    public async Task<IActionResult> PostsByCampaign(Guid campaignId)
        => Ok(await db.posts.AsNoTracking().Where(p => p.campaign_id == campaignId)
                 .OrderByDescending(p => p.published_at).ToListAsync());

    [HttpPost("posts")]
    public async Task<IActionResult> UpsertPost([FromBody] UpsertPostDto dto)
    {
        var p = new Infrastructure.EF.Models.post
        {
            id = Guid.NewGuid(),
            campaign_id = dto.CampaignId,
            title = dto.Title,
            content_md = dto.ContentMd,
            cover_url = dto.CoverUrl,
            Status = dto.Status,
            published_at = dto.Status == CHUYENXEBACAI.Domain.PostStatus.Published ? DateTime.UtcNow : null
        };
        db.posts.Add(p);
        await db.SaveChangesAsync();
        return Ok(p);
    }

    [HttpGet("faqs")]
    public async Task<IActionResult> Faqs()
        => Ok(await db.faqs.AsNoTracking().OrderBy(x => x.order_no).ToListAsync());

    [HttpPost("faqs")]
    public async Task<IActionResult> UpsertFaq([FromBody] UpsertFaqDto dto)
    {
        var f = new Infrastructure.EF.Models.faq
        {
            id = Guid.NewGuid(),
            question = dto.Question,
            answer_md = dto.AnswerMd,
            tags = dto.Tags?.ToList() ?? new List<string>(),   // <-- convert string[] -> List<string>
            order_no = dto.OrderNo
        };
        db.faqs.Add(f);
        await db.SaveChangesAsync();
        return Ok(f);
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeDto dto)
    {
        if (await db.newsletter_subscriptions.AnyAsync(x => x.email == dto.Email))
            return Conflict("Email already subscribed.");

        var s = new Infrastructure.EF.Models.newsletter_subscription
        {
            id = Guid.NewGuid(),
            email = dto.Email,
            consent = dto.Consent
        };
        db.newsletter_subscriptions.Add(s);
        await db.SaveChangesAsync();
        return Ok(s);
    }
}

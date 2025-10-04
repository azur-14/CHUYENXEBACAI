using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHUYENXEBACAI.Infrastructure.EF;

namespace CHUYENXEBACAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinanceController(AppDbContext db) : ControllerBase
{
    // Expenses
    [HttpGet("expenses/by-campaign/{campaignId:guid}")]
    public async Task<IActionResult> ExpensesByCampaign(Guid campaignId)
        => Ok(await db.expenses.AsNoTracking().Where(x => x.campaign_id == campaignId)
                 .OrderByDescending(x => x.occurred_at).ToListAsync());

    [HttpPost("expenses")]
    public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDto dto)
    {
        var e = new Infrastructure.EF.Models.expense
        {
            id = Guid.NewGuid(),
            campaign_id = dto.CampaignId,
            session_id = dto.SessionId,
            category = dto.Category,
            description = dto.Description,
            amount = dto.Amount,
            Currency = dto.Currency,
            payment_method = dto.PaymentMethod,
            payer_id = dto.PayerId,
            receipt_url = dto.ReceiptUrl,
            note = dto.Note
        };
        db.expenses.Add(e);
        await db.SaveChangesAsync();
        return Ok(e);
    }

    // Donations
    [HttpGet("donations/by-campaign/{campaignId:guid}")]
    public async Task<IActionResult> DonationsByCampaign(Guid campaignId)
        => Ok(await db.donations.AsNoTracking().Where(x => x.campaign_id == campaignId)
                 .OrderByDescending(x => x.created_at).ToListAsync());

    [HttpPost("donations")]
    public async Task<IActionResult> UpsertDonation([FromBody] UpsertDonationDto dto)
    {
        var d = new Infrastructure.EF.Models.donation
        {
            id = Guid.NewGuid(),
            campaign_id = dto.CampaignId,
            donor_name = dto.DonorName,
            donor_email = dto.DonorEmail,
            amount = dto.Amount,
            Currency = dto.Currency,
            wish_to_show_name = dto.WishToShowName,
            message = dto.Message,
            Gateway = dto.Gateway,
            order_code = dto.OrderCode,
            Status = dto.Status,
            paid_at = dto.PaidAt
        };
        db.donations.Add(d);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("donations_order_code_key") == true)
        { return Conflict("order_code already exists."); }
        return Ok(d);
    }

    // Fund transactions
    [HttpGet("funds/by-campaign/{campaignId:guid}")]
    public async Task<IActionResult> FundsByCampaign(Guid campaignId)
        => Ok(await db.fund_transactions.AsNoTracking().Where(x => x.campaign_id == campaignId)
                 .OrderByDescending(x => x.occurred_at).ToListAsync());

    [HttpPost("funds")]
    public async Task<IActionResult> UpsertFundTx([FromBody] UpsertFundTxDto dto)
    {
        var exists = await db.fund_transactions.AnyAsync(x => x.Source == dto.Source && x.ref_id == dto.RefId);
        if (exists) return Conflict("Duplicate (source, ref_id).");

        var f = new Infrastructure.EF.Models.fund_transaction
        {
            id = Guid.NewGuid(),
            campaign_id = dto.CampaignId,
            Source = dto.Source,
            ref_id = dto.RefId,
            amount = dto.Amount,
            occurred_at = dto.OccurredAt,
            donation_id = dto.DonationId
        };
        db.fund_transactions.Add(f);
        await db.SaveChangesAsync();
        return Ok(f);
    }
}

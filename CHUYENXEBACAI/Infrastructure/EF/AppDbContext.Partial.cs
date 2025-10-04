using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;   // <- cần có để dùng HasPostgresEnum
using CHUYENXEBACAI.Domain;

namespace CHUYENXEBACAI.Infrastructure.EF;

// KHÔNG override OnModelCreating lần nữa.
// Chỉ implement partial method mà AppDbContext.cs đã khai báo & gọi sẵn.
public partial class AppDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Khai báo Postgres ENUMs có sẵn trong DB
        modelBuilder.HasPostgresEnum<UserStatus>("public", "user_status_enum");
        modelBuilder.HasPostgresEnum<AppReviewStatus>("public", "app_review_status_enum");
        modelBuilder.HasPostgresEnum<RegistrationStatus>("public", "registration_status_enum");
        modelBuilder.HasPostgresEnum<CampaignStatus>("public", "campaign_status_enum");
        modelBuilder.HasPostgresEnum<SessionStatus>("public", "session_status_enum");
        modelBuilder.HasPostgresEnum<SessionShift>("public", "session_shift_enum");
        modelBuilder.HasPostgresEnum<CheckinMethod>("public", "checkin_method_enum");
        modelBuilder.HasPostgresEnum<CheckinStatus>("public", "checkin_status_enum");
        modelBuilder.HasPostgresEnum<DonationGateway>("public", "donation_gateway_enum");
        modelBuilder.HasPostgresEnum<DonationStatus>("public", "donation_status_enum");
        modelBuilder.HasPostgresEnum<FundSource>("public", "fund_source_enum");
        modelBuilder.HasPostgresEnum<FundStatus>("public", "fund_status_enum");
        modelBuilder.HasPostgresEnum<BankImportSource>("public", "bank_import_source_enum");
        modelBuilder.HasPostgresEnum<ReconcileDecision>("public", "reconcile_decision_enum");
        modelBuilder.HasPostgresEnum<Currency>("public", "currency_enum");
        modelBuilder.HasPostgresEnum<ChangeAction>("public", "change_action_enum");
        modelBuilder.HasPostgresEnum<PostStatus>("public", "post_status_enum");

        // Map 3 VIEW thành keyless entity
        modelBuilder.Entity<VCampaignProgress>().HasNoKey().ToView("v_campaign_progress", "public");
        modelBuilder.Entity<VSessionRoster>().HasNoKey().ToView("v_session_roster", "public");
        modelBuilder.Entity<VReconcileSummary>().HasNoKey().ToView("v_reconcile_summary", "public");
    }
}

// Keyless types khớp với cột trong view
public class VCampaignProgress
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public decimal? DonationsPaid { get; set; }
    public decimal? TotalExpenses { get; set; }
    public decimal? GoalAmount { get; set; }
    public decimal? NetAmount { get; set; }
    public decimal? PercentOfGoal { get; set; }
}

public class VSessionRoster
{
    public Guid SessionId { get; set; }
    public Guid CampaignId { get; set; }
    public DateOnly SessionDate { get; set; }  // nếu scaffold của bạn dùng DateTime, đổi sang DateTime
    public SessionShift Shift { get; set; }
    public int? Quota { get; set; }
    public int ApprovedVolunteers { get; set; }
    public bool IsFull { get; set; }
}

public class VReconcileSummary
{
    public Guid FundTxId { get; set; }
    public Guid CampaignId { get; set; }
    public FundSource Source { get; set; }
    public string RefId { get; set; } = default!;
    public decimal FundAmount { get; set; }
    public DateTime FundTime { get; set; }
    public Guid? BankStmtId { get; set; }
    public decimal? BankAmount { get; set; }
    public DateTime? BankTime { get; set; }
    public decimal? Score { get; set; }
    public ReconcileDecision? Decision { get; set; }
    public DateTime? DecidedAt { get; set; }
}

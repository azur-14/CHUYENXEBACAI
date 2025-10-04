using System.ComponentModel.DataAnnotations.Schema;
using CHUYENXEBACAI.Domain;

namespace CHUYENXEBACAI.Infrastructure.EF.Models
{
    public partial class user { [Column("status")] public UserStatus Status { get; set; } }

    public partial class volunteer_application { [Column("status")] public AppReviewStatus Status { get; set; } }

    public partial class campaign { [Column("status")] public CampaignStatus Status { get; set; } }

    public partial class session
    {
        [Column("shift")]  public SessionShift  Shift  { get; set; }
        [Column("status")] public SessionStatus Status { get; set; }
    }

    public partial class volunteer_registration { [Column("status")] public RegistrationStatus Status { get; set; } }

    public partial class checkin
    {
        [Column("method")] public CheckinMethod Method { get; set; }
        [Column("status")] public CheckinStatus Status { get; set; }
    }

    public partial class expense { [Column("currency")] public Currency Currency { get; set; } }

    public partial class donation
    {
        [Column("currency")] public Currency        Currency { get; set; }
        [Column("gateway")]  public DonationGateway? Gateway  { get; set; }
        [Column("status")]   public DonationStatus?  Status   { get; set; }
    }

    public partial class fund_transaction
    {
        [Column("source")] public FundSource Source { get; set; }
        [Column("status")] public FundStatus Status { get; set; }
    }

    public partial class bank_statement { [Column("source")] public BankImportSource Source { get; set; } }

    public partial class reconcile_match { [Column("decision")] public ReconcileDecision? Decision { get; set; } }

    public partial class post { [Column("status")] public PostStatus Status { get; set; } }

    public partial class change_log { [Column("action")] public ChangeAction Action { get; set; } }
}

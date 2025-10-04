using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class v_reconcile_summary
{
    public Guid? fund_tx_id { get; set; }

    public Guid? campaign_id { get; set; }

    public string? ref_id { get; set; }

    public decimal? fund_amount { get; set; }

    public DateTime? fund_time { get; set; }

    public Guid? bank_stmt_id { get; set; }

    public decimal? bank_amount { get; set; }

    public DateTime? bank_time { get; set; }

    public decimal? score { get; set; }

    public DateTime? decided_at { get; set; }
}

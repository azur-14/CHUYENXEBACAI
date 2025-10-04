using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class reconcile_match
{
    public Guid id { get; set; }

    public Guid fund_tx_id { get; set; }

    public Guid bank_stmt_id { get; set; }

    public decimal? score { get; set; }

    public Guid? decided_by { get; set; }

    public DateTime? decided_at { get; set; }

    public string? note { get; set; }

    public string? decision_history { get; set; }

    public virtual bank_statement bank_stmt { get; set; } = null!;

    public virtual user? decided_byNavigation { get; set; }

    public virtual fund_transaction fund_tx { get; set; } = null!;
}

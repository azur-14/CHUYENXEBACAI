using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class fund_transaction
{
    public Guid id { get; set; }

    public Guid campaign_id { get; set; }

    public string ref_id { get; set; } = null!;

    public decimal amount { get; set; }

    public DateTime occurred_at { get; set; }

    public Guid? donation_id { get; set; }

    public DateTime created_at { get; set; }

    public virtual campaign campaign { get; set; } = null!;

    public virtual donation? donation { get; set; }

    public virtual ICollection<reconcile_match> reconcile_matches { get; set; } = new List<reconcile_match>();
}

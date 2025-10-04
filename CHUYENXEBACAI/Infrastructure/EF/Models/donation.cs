using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class donation
{
    public Guid id { get; set; }

    public Guid campaign_id { get; set; }

    public string? donor_name { get; set; }

    public string? donor_email { get; set; }

    public decimal amount { get; set; }

    public bool wish_to_show_name { get; set; }

    public string? message { get; set; }

    public DateTime created_at { get; set; }

    public string? order_code { get; set; }

    public DateTime? paid_at { get; set; }

    public virtual campaign campaign { get; set; } = null!;

    public virtual ICollection<fund_transaction> fund_transactions { get; set; } = new List<fund_transaction>();
}

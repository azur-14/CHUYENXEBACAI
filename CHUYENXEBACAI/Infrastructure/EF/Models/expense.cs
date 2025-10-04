using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class expense
{
    public Guid id { get; set; }

    public Guid campaign_id { get; set; }

    public Guid? session_id { get; set; }

    public DateTime occurred_at { get; set; }

    public string? category { get; set; }

    public string? description { get; set; }

    public decimal amount { get; set; }

    public string? payment_method { get; set; }

    public Guid? payer_id { get; set; }

    public string? receipt_url { get; set; }

    public string? note { get; set; }

    public virtual campaign campaign { get; set; } = null!;

    public virtual user? payer { get; set; }

    public virtual session? session { get; set; }
}

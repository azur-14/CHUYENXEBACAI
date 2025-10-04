using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class volunteer_registration
{
    public Guid id { get; set; }

    public Guid user_id { get; set; }

    public Guid campaign_id { get; set; }

    public Guid? session_id { get; set; }

    public DateTime applied_at { get; set; }

    public Guid? reviewed_by { get; set; }

    public DateTime? reviewed_at { get; set; }

    public string? reject_reason { get; set; }

    public virtual campaign campaign { get; set; } = null!;

    public virtual user? reviewed_byNavigation { get; set; }

    public virtual session? session { get; set; }

    public virtual user user { get; set; } = null!;
}

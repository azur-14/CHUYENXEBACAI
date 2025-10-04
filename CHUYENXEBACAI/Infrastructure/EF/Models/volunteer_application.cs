using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class volunteer_application
{
    public Guid id { get; set; }

    public Guid user_id { get; set; }

    public string? skills { get; set; }

    public string? availability { get; set; }

    public Guid? reviewed_by { get; set; }

    public DateTime? reviewed_at { get; set; }

    public string? reject_reason { get; set; }

    public DateTime created_at { get; set; }

    public virtual user? reviewed_byNavigation { get; set; }

    public virtual user user { get; set; } = null!;
}

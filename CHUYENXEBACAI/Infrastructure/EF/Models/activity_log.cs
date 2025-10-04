using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class activity_log
{
    public Guid id { get; set; }

    public Guid session_id { get; set; }

    public string activity_type { get; set; } = null!;

    public Guid? performed_by { get; set; }

    public DateTime? started_at { get; set; }

    public DateTime? ended_at { get; set; }

    public string? note { get; set; }

    public virtual user? performed_byNavigation { get; set; }

    public virtual session session { get; set; } = null!;
}

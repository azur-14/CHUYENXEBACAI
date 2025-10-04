using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class change_log
{
    public Guid id { get; set; }

    public string entity_type { get; set; } = null!;

    public Guid entity_id { get; set; }

    public Guid? changed_by { get; set; }

    public string? before_data { get; set; }

    public string? after_data { get; set; }

    public DateTime changed_at { get; set; }

    public virtual user? changed_byNavigation { get; set; }
}

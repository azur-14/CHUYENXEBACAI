using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class checkin
{
    public Guid id { get; set; }

    public Guid session_id { get; set; }

    public Guid user_id { get; set; }

    public DateTime checkin_time { get; set; }

    public double? lat { get; set; }

    public double? lng { get; set; }

    public string? evidence_note { get; set; }

    public virtual ICollection<media_asset> media_assets { get; set; } = new List<media_asset>();

    public virtual session session { get; set; } = null!;

    public virtual user user { get; set; } = null!;
}

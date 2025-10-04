using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class media_asset
{
    public Guid id { get; set; }

    public Guid campaign_id { get; set; }

    public Guid? uploader_id { get; set; }

    public Guid checkin_id { get; set; }

    public string url { get; set; } = null!;

    public string? public_id { get; set; }

    public string? thumb_url { get; set; }

    public string? format { get; set; }

    public DateTime uploaded_at { get; set; }

    public virtual campaign campaign { get; set; } = null!;

    public virtual checkin checkin { get; set; } = null!;

    public virtual user? uploader { get; set; }
}

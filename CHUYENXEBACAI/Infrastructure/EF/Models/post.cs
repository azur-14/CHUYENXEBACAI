using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class post
{
    public Guid id { get; set; }

    public Guid campaign_id { get; set; }

    public string title { get; set; } = null!;

    public string? content_md { get; set; }

    public string? cover_url { get; set; }

    public DateTime? published_at { get; set; }

    public virtual campaign campaign { get; set; } = null!;
}

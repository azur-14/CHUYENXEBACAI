using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class newsletter_subscription
{
    public Guid id { get; set; }

    public string email { get; set; } = null!;

    public bool consent { get; set; }

    public DateTime created_at { get; set; }
}

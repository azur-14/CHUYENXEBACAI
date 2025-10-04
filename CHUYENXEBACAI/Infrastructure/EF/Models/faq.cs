using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class faq
{
    public Guid id { get; set; }

    public string question { get; set; } = null!;

    public string? answer_md { get; set; }

    public List<string>? tags { get; set; }

    public int? order_no { get; set; }
}

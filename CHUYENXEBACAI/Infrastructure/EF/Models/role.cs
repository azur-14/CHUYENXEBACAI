using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class role
{
    public Guid id { get; set; }

    public string code { get; set; } = null!;

    public string? name { get; set; }

    public virtual ICollection<user> users { get; set; } = new List<user>();
}

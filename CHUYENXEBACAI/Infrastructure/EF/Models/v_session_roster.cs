using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class v_session_roster
{
    public Guid? session_id { get; set; }

    public Guid? campaign_id { get; set; }

    public DateOnly? session_date { get; set; }

    public int? quota { get; set; }

    public int? approved_volunteers { get; set; }

    public bool? is_full { get; set; }
}

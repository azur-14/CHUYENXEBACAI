using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class v_campaign_progress
{
    public Guid? id { get; set; }

    public string? title { get; set; }

    public decimal? donations_paid { get; set; }

    public decimal? total_expenses { get; set; }

    public decimal? goal_amount { get; set; }

    public decimal? net_amount { get; set; }

    public decimal? percent_of_goal { get; set; }
}

using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class campaign
{
    public Guid id { get; set; }

    public string title { get; set; } = null!;

    public string? description { get; set; }

    public string? location { get; set; }

    public DateOnly? start_date { get; set; }

    public DateOnly? end_date { get; set; }

    public decimal? goal_amount { get; set; }

    public decimal? collected_amount { get; set; }

    public int? goal_volunteers { get; set; }

    public Guid? created_by { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual user? created_byNavigation { get; set; }

    public virtual ICollection<donation> donations { get; set; } = new List<donation>();

    public virtual ICollection<expense> expenses { get; set; } = new List<expense>();

    public virtual ICollection<fund_transaction> fund_transactions { get; set; } = new List<fund_transaction>();

    public virtual ICollection<media_asset> media_assets { get; set; } = new List<media_asset>();

    public virtual ICollection<post> posts { get; set; } = new List<post>();

    public virtual ICollection<session> sessions { get; set; } = new List<session>();

    public virtual ICollection<volunteer_registration> volunteer_registrations { get; set; } = new List<volunteer_registration>();
}

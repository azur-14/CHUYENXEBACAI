using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class session
{
    public Guid id { get; set; }

    public Guid campaign_id { get; set; }

    public string? title { get; set; }

    public DateOnly session_date { get; set; }

    public DateTime? start_time { get; set; }

    public DateTime? end_time { get; set; }

    public bool checkin_open { get; set; }

    public int? quota { get; set; }

    public int approved_volunteers { get; set; }

    public string? place_name { get; set; }

    public double? lat { get; set; }

    public double? lng { get; set; }

    public int? geo_radius_m { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<activity_log> activity_logs { get; set; } = new List<activity_log>();

    public virtual campaign campaign { get; set; } = null!;

    public virtual ICollection<checkin> checkins { get; set; } = new List<checkin>();

    public virtual ICollection<expense> expenses { get; set; } = new List<expense>();

    public virtual ICollection<volunteer_registration> volunteer_registrations { get; set; } = new List<volunteer_registration>();
}

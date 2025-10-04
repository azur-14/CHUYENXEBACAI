using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class user
{
    public Guid id { get; set; }

    public string email { get; set; } = null!;

    public string password_hash { get; set; } = null!;

    public string full_name { get; set; } = null!;

    public string? phone { get; set; }

    public string? avatar_url { get; set; }

    public string? bio { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<activity_log> activity_logs { get; set; } = new List<activity_log>();

    public virtual ICollection<bank_statement> bank_statements { get; set; } = new List<bank_statement>();

    public virtual ICollection<campaign> campaigns { get; set; } = new List<campaign>();

    public virtual ICollection<change_log> change_logs { get; set; } = new List<change_log>();

    public virtual ICollection<checkin> checkins { get; set; } = new List<checkin>();

    public virtual ICollection<expense> expenses { get; set; } = new List<expense>();

    public virtual ICollection<media_asset> media_assets { get; set; } = new List<media_asset>();

    public virtual ICollection<reconcile_match> reconcile_matches { get; set; } = new List<reconcile_match>();

    public virtual ICollection<volunteer_application> volunteer_applicationreviewed_byNavigations { get; set; } = new List<volunteer_application>();

    public virtual ICollection<volunteer_application> volunteer_applicationusers { get; set; } = new List<volunteer_application>();

    public virtual ICollection<volunteer_registration> volunteer_registrationreviewed_byNavigations { get; set; } = new List<volunteer_registration>();

    public virtual ICollection<volunteer_registration> volunteer_registrationusers { get; set; } = new List<volunteer_registration>();

    public virtual ICollection<role> roles { get; set; } = new List<role>();
}

using System;
using System.Collections.Generic;

namespace CHUYENXEBACAI.Infrastructure.EF.Models;

public partial class bank_statement
{
    public Guid id { get; set; }

    public string? bank_ref { get; set; }

    public string? description { get; set; }

    public decimal amount { get; set; }

    public DateTime txn_time { get; set; }

    public string? account_no { get; set; }

    public string? raw { get; set; }

    public string? file_name { get; set; }

    public Guid? imported_by { get; set; }

    public DateTime imported_at { get; set; }

    public string? notes { get; set; }

    public virtual user? imported_byNavigation { get; set; }

    public virtual ICollection<reconcile_match> reconcile_matches { get; set; } = new List<reconcile_match>();
}

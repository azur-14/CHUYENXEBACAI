using System;
using System.Collections.Generic;
using CHUYENXEBACAI.Infrastructure.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace CHUYENXEBACAI.Infrastructure.EF;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<activity_log> activity_logs { get; set; }

    public virtual DbSet<bank_statement> bank_statements { get; set; }

    public virtual DbSet<campaign> campaigns { get; set; }

    public virtual DbSet<change_log> change_logs { get; set; }

    public virtual DbSet<checkin> checkins { get; set; }

    public virtual DbSet<donation> donations { get; set; }

    public virtual DbSet<expense> expenses { get; set; }

    public virtual DbSet<faq> faqs { get; set; }

    public virtual DbSet<fund_transaction> fund_transactions { get; set; }

    public virtual DbSet<media_asset> media_assets { get; set; }

    public virtual DbSet<newsletter_subscription> newsletter_subscriptions { get; set; }

    public virtual DbSet<post> posts { get; set; }

    public virtual DbSet<reconcile_match> reconcile_matches { get; set; }

    public virtual DbSet<role> roles { get; set; }

    public virtual DbSet<session> sessions { get; set; }

    public virtual DbSet<user> users { get; set; }

    public virtual DbSet<v_campaign_progress> v_campaign_progresses { get; set; }

    public virtual DbSet<v_reconcile_summary> v_reconcile_summaries { get; set; }

    public virtual DbSet<v_session_roster> v_session_rosters { get; set; }

    public virtual DbSet<volunteer_application> volunteer_applications { get; set; }

    public virtual DbSet<volunteer_registration> volunteer_registrations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("app_review_status_enum", new[] { "PENDING_REVIEW", "APPROVED", "REJECTED" })
            .HasPostgresEnum("bank_import_source_enum", new[] { "CSV", "EMAIL" })
            .HasPostgresEnum("campaign_status_enum", new[] { "PLANNING", "ONGOING", "DONE", "CANCELLED" })
            .HasPostgresEnum("change_action_enum", new[] { "CREATE", "UPDATE", "DELETE" })
            .HasPostgresEnum("checkin_method_enum", new[] { "QR", "MANUAL" })
            .HasPostgresEnum("checkin_status_enum", new[] { "ON_TIME", "LATE", "INVALID" })
            .HasPostgresEnum("currency_enum", new[] { "VND", "USD", "EUR" })
            .HasPostgresEnum("donation_gateway_enum", new[] { "MOMO", "VNPAY", "STRIPE" })
            .HasPostgresEnum("donation_status_enum", new[] { "PENDING", "PAID", "FAILED", "CANCELLED" })
            .HasPostgresEnum("fund_source_enum", new[] { "WEBHOOK", "CSV", "EMAIL", "MANUAL" })
            .HasPostgresEnum("fund_status_enum", new[] { "PENDING", "MATCHED", "FLAGGED" })
            .HasPostgresEnum("post_status_enum", new[] { "DRAFT", "PUBLISHED" })
            .HasPostgresEnum("reconcile_decision_enum", new[] { "AUTO", "ACCEPT", "REJECT", "REVIEW", "UNMATCH" })
            .HasPostgresEnum("registration_status_enum", new[] { "PENDING", "APPROVED", "REJECTED", "CANCELLED" })
            .HasPostgresEnum("session_shift_enum", new[] { "MORNING", "AFTERNOON", "EVENING" })
            .HasPostgresEnum("session_status_enum", new[] { "PLANNED", "ONGOING", "DONE" })
            .HasPostgresEnum("user_status_enum", new[] { "ACTIVE", "INACTIVE" })
            .HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<activity_log>(entity =>
        {
            entity.HasKey(e => e.id).HasName("activity_logs_pkey");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.performed_byNavigation).WithMany(p => p.activity_logs)
                .HasForeignKey(d => d.performed_by)
                .HasConstraintName("activity_logs_performed_by_fkey");

            entity.HasOne(d => d.session).WithMany(p => p.activity_logs)
                .HasForeignKey(d => d.session_id)
                .HasConstraintName("activity_logs_session_id_fkey");
        });

        modelBuilder.Entity<bank_statement>(entity =>
        {
            entity.HasKey(e => e.id).HasName("bank_statements_pkey");

            entity.HasIndex(e => e.bank_ref, "idx_bank_ref");

            entity.HasIndex(e => e.txn_time, "idx_bank_txn_time");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.amount).HasPrecision(14, 2);
            entity.Property(e => e.imported_at).HasDefaultValueSql("now()");
            entity.Property(e => e.raw).HasColumnType("jsonb");

            entity.HasOne(d => d.imported_byNavigation).WithMany(p => p.bank_statements)
                .HasForeignKey(d => d.imported_by)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("bank_statements_imported_by_fkey");
        });

        modelBuilder.Entity<campaign>(entity =>
        {
            entity.HasKey(e => e.id).HasName("campaigns_pkey");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.collected_amount)
                .HasPrecision(14, 2)
                .HasDefaultValueSql("0");
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.goal_amount).HasPrecision(14, 2);
            entity.Property(e => e.updated_at).HasDefaultValueSql("now()");

            entity.HasOne(d => d.created_byNavigation).WithMany(p => p.campaigns)
                .HasForeignKey(d => d.created_by)
                .HasConstraintName("campaigns_created_by_fkey");
        });

        modelBuilder.Entity<change_log>(entity =>
        {
            entity.HasKey(e => e.id).HasName("change_logs_pkey");

            entity.HasIndex(e => new { e.entity_type, e.entity_id }, "idx_changelog_entity");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.after_data).HasColumnType("jsonb");
            entity.Property(e => e.before_data).HasColumnType("jsonb");
            entity.Property(e => e.changed_at).HasDefaultValueSql("now()");

            entity.HasOne(d => d.changed_byNavigation).WithMany(p => p.change_logs)
                .HasForeignKey(d => d.changed_by)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("change_logs_changed_by_fkey");
        });

        modelBuilder.Entity<checkin>(entity =>
        {
            entity.HasKey(e => e.id).HasName("checkins_pkey");

            entity.HasIndex(e => e.session_id, "idx_checkins_session");

            entity.HasIndex(e => new { e.session_id, e.user_id }, "uq_checkin_once_per_session").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.checkin_time).HasDefaultValueSql("now()");

            entity.HasOne(d => d.session).WithMany(p => p.checkins)
                .HasForeignKey(d => d.session_id)
                .HasConstraintName("checkins_session_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.checkins)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("checkins_user_id_fkey");
        });

        modelBuilder.Entity<donation>(entity =>
        {
            entity.HasKey(e => e.id).HasName("donations_pkey");

            entity.HasIndex(e => e.order_code, "donations_order_code_key").IsUnique();

            entity.HasIndex(e => e.campaign_id, "idx_donations_campaign");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.amount).HasPrecision(14, 2);
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.wish_to_show_name).HasDefaultValue(true);

            entity.HasOne(d => d.campaign).WithMany(p => p.donations)
                .HasForeignKey(d => d.campaign_id)
                .HasConstraintName("donations_campaign_id_fkey");
        });

        modelBuilder.Entity<expense>(entity =>
        {
            entity.HasKey(e => e.id).HasName("expenses_pkey");

            entity.HasIndex(e => e.campaign_id, "idx_expenses_campaign");

            entity.HasIndex(e => e.session_id, "idx_expenses_session");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.amount).HasPrecision(14, 2);
            entity.Property(e => e.occurred_at).HasDefaultValueSql("now()");

            entity.HasOne(d => d.campaign).WithMany(p => p.expenses)
                .HasForeignKey(d => d.campaign_id)
                .HasConstraintName("expenses_campaign_id_fkey");

            entity.HasOne(d => d.payer).WithMany(p => p.expenses)
                .HasForeignKey(d => d.payer_id)
                .HasConstraintName("expenses_payer_id_fkey");

            entity.HasOne(d => d.session).WithMany(p => p.expenses)
                .HasForeignKey(d => d.session_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("expenses_session_id_fkey");
        });

        modelBuilder.Entity<faq>(entity =>
        {
            entity.HasKey(e => e.id).HasName("faqs_pkey");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
        });

        modelBuilder.Entity<fund_transaction>(entity =>
        {
            entity.HasKey(e => e.id).HasName("fund_transactions_pkey");

            entity.HasIndex(e => e.campaign_id, "idx_fund_campaign");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.amount).HasPrecision(14, 2);
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");

            entity.HasOne(d => d.campaign).WithMany(p => p.fund_transactions)
                .HasForeignKey(d => d.campaign_id)
                .HasConstraintName("fund_transactions_campaign_id_fkey");

            entity.HasOne(d => d.donation).WithMany(p => p.fund_transactions)
                .HasForeignKey(d => d.donation_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fund_transactions_donation_id_fkey");
        });

        modelBuilder.Entity<media_asset>(entity =>
        {
            entity.HasKey(e => e.id).HasName("media_assets_pkey");

            entity.HasIndex(e => e.campaign_id, "idx_media_campaign");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.uploaded_at).HasDefaultValueSql("now()");

            entity.HasOne(d => d.campaign).WithMany(p => p.media_assets)
                .HasForeignKey(d => d.campaign_id)
                .HasConstraintName("media_assets_campaign_id_fkey");

            entity.HasOne(d => d.checkin).WithMany(p => p.media_assets)
                .HasForeignKey(d => d.checkin_id)
                .HasConstraintName("media_assets_checkin_id_fkey");

            entity.HasOne(d => d.uploader).WithMany(p => p.media_assets)
                .HasForeignKey(d => d.uploader_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("media_assets_uploader_id_fkey");
        });

        modelBuilder.Entity<newsletter_subscription>(entity =>
        {
            entity.HasKey(e => e.id).HasName("newsletter_subscriptions_pkey");

            entity.HasIndex(e => e.email, "newsletter_subscriptions_email_key").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.consent).HasDefaultValue(true);
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<post>(entity =>
        {
            entity.HasKey(e => e.id).HasName("posts_pkey");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.campaign).WithMany(p => p.posts)
                .HasForeignKey(d => d.campaign_id)
                .HasConstraintName("posts_campaign_id_fkey");
        });

        modelBuilder.Entity<reconcile_match>(entity =>
        {
            entity.HasKey(e => e.id).HasName("reconcile_matches_pkey");

            entity.HasIndex(e => new { e.fund_tx_id, e.bank_stmt_id }, "uq_match_pair").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.decision_history).HasColumnType("jsonb");
            entity.Property(e => e.score).HasPrecision(6, 3);

            entity.HasOne(d => d.bank_stmt).WithMany(p => p.reconcile_matches)
                .HasForeignKey(d => d.bank_stmt_id)
                .HasConstraintName("reconcile_matches_bank_stmt_id_fkey");

            entity.HasOne(d => d.decided_byNavigation).WithMany(p => p.reconcile_matches)
                .HasForeignKey(d => d.decided_by)
                .HasConstraintName("reconcile_matches_decided_by_fkey");

            entity.HasOne(d => d.fund_tx).WithMany(p => p.reconcile_matches)
                .HasForeignKey(d => d.fund_tx_id)
                .HasConstraintName("reconcile_matches_fund_tx_id_fkey");
        });

        modelBuilder.Entity<role>(entity =>
        {
            entity.HasKey(e => e.id).HasName("roles_pkey");

            entity.HasIndex(e => e.code, "roles_code_key").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
        });

        modelBuilder.Entity<session>(entity =>
        {
            entity.HasKey(e => e.id).HasName("sessions_pkey");

            entity.HasIndex(e => e.campaign_id, "idx_sessions_campaign");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.approved_volunteers).HasDefaultValue(0);
            entity.Property(e => e.checkin_open).HasDefaultValue(false);
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.updated_at).HasDefaultValueSql("now()");

            entity.HasOne(d => d.campaign).WithMany(p => p.sessions)
                .HasForeignKey(d => d.campaign_id)
                .HasConstraintName("sessions_campaign_id_fkey");
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.id).HasName("users_pkey");

            entity.HasIndex(e => e.email, "users_email_key").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.updated_at).HasDefaultValueSql("now()");

            entity.HasMany(d => d.roles).WithMany(p => p.users)
                .UsingEntity<Dictionary<string, object>>(
                    "user_role",
                    r => r.HasOne<role>().WithMany()
                        .HasForeignKey("role_id")
                        .HasConstraintName("user_roles_role_id_fkey"),
                    l => l.HasOne<user>().WithMany()
                        .HasForeignKey("user_id")
                        .HasConstraintName("user_roles_user_id_fkey"),
                    j =>
                    {
                        j.HasKey("user_id", "role_id").HasName("user_roles_pkey");
                        j.ToTable("user_roles");
                    });
        });

        modelBuilder.Entity<v_campaign_progress>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_campaign_progress");

            entity.Property(e => e.goal_amount).HasPrecision(14, 2);
        });

        modelBuilder.Entity<v_reconcile_summary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_reconcile_summary");

            entity.Property(e => e.bank_amount).HasPrecision(14, 2);
            entity.Property(e => e.fund_amount).HasPrecision(14, 2);
            entity.Property(e => e.score).HasPrecision(6, 3);
        });

        modelBuilder.Entity<v_session_roster>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_session_roster");
        });

        modelBuilder.Entity<volunteer_application>(entity =>
        {
            entity.HasKey(e => e.id).HasName("volunteer_applications_pkey");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");

            entity.HasOne(d => d.reviewed_byNavigation).WithMany(p => p.volunteer_applicationreviewed_byNavigations)
                .HasForeignKey(d => d.reviewed_by)
                .HasConstraintName("volunteer_applications_reviewed_by_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.volunteer_applicationusers)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("volunteer_applications_user_id_fkey");
        });

        modelBuilder.Entity<volunteer_registration>(entity =>
        {
            entity.HasKey(e => e.id).HasName("volunteer_registrations_pkey");

            entity.HasIndex(e => new { e.user_id, e.campaign_id }, "uq_reg_user_campaign_when_session_null")
                .IsUnique()
                .HasFilter("(session_id IS NULL)");

            entity.HasIndex(e => new { e.user_id, e.session_id }, "uq_reg_user_session")
                .IsUnique()
                .HasFilter("(session_id IS NOT NULL)");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.applied_at).HasDefaultValueSql("now()");

            entity.HasOne(d => d.campaign).WithMany(p => p.volunteer_registrations)
                .HasForeignKey(d => d.campaign_id)
                .HasConstraintName("volunteer_registrations_campaign_id_fkey");

            entity.HasOne(d => d.reviewed_byNavigation).WithMany(p => p.volunteer_registrationreviewed_byNavigations)
                .HasForeignKey(d => d.reviewed_by)
                .HasConstraintName("volunteer_registrations_reviewed_by_fkey");

            entity.HasOne(d => d.session).WithMany(p => p.volunteer_registrations)
                .HasForeignKey(d => d.session_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("volunteer_registrations_session_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.volunteer_registrationusers)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("volunteer_registrations_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

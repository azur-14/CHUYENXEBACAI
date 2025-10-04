-- ======================================
-- 01_triggers_and_views.sql  (run second)
-- ======================================
SET search_path TO public;
SET TIME ZONE 'Asia/Ho_Chi_Minh';

-- (A) Idempotent CHECKs for sessions
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint
    WHERE conname='ck_quota_nonneg' AND conrelid='public.sessions'::regclass
  ) THEN
    ALTER TABLE public.sessions
      ADD CONSTRAINT ck_quota_nonneg CHECK (quota IS NULL OR quota >= 0);
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint
    WHERE conname='ck_approved_le_quota' AND conrelid='public.sessions'::regclass
  ) THEN
    ALTER TABLE public.sessions
      ADD CONSTRAINT ck_approved_le_quota
      CHECK (quota IS NULL OR approved_volunteers <= quota);
  END IF;
END$$;

-- (B) updated_at triggers
CREATE OR REPLACE FUNCTION public.set_updated_at()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
  NEW.updated_at := now();
  RETURN NEW;
END$$;

DROP TRIGGER IF EXISTS trg_users_updated_at     ON public.users;
DROP TRIGGER IF EXISTS trg_campaigns_updated_at ON public.campaigns;
DROP TRIGGER IF EXISTS trg_sessions_updated_at  ON public.sessions;

CREATE TRIGGER trg_users_updated_at
BEFORE UPDATE ON public.users
FOR EACH ROW EXECUTE FUNCTION public.set_updated_at();

CREATE TRIGGER trg_campaigns_updated_at
BEFORE UPDATE ON public.campaigns
FOR EACH ROW EXECUTE FUNCTION public.set_updated_at();

CREATE TRIGGER trg_sessions_updated_at
BEFORE UPDATE ON public.sessions
FOR EACH ROW EXECUTE FUNCTION public.set_updated_at();

-- (C) sync approved_volunteers theo volunteer_registrations
CREATE OR REPLACE FUNCTION public.sync_session_approved_count()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
DECLARE
  s_id UUID;
  delta INT := 0;
BEGIN
  s_id := COALESCE(NEW.session_id, OLD.session_id);
  IF s_id IS NULL THEN RETURN NEW; END IF;

  IF (TG_OP = 'INSERT') THEN
    IF NEW.status = 'APPROVED' THEN delta := 1; END IF;
  ELSIF (TG_OP = 'UPDATE') THEN
    IF OLD.status = 'APPROVED' AND NEW.status <> 'APPROVED' THEN delta := -1; END IF;
    IF OLD.status <> 'APPROVED' AND NEW.status = 'APPROVED' THEN delta := 1;  END IF;
  ELSIF (TG_OP = 'DELETE') THEN
    IF OLD.status = 'APPROVED' THEN delta := -1; END IF;
  END IF;

  IF delta <> 0 THEN
    UPDATE public.sessions
       SET approved_volunteers = GREATEST(0, approved_volunteers + delta),
           updated_at = now()
     WHERE id = s_id;

    UPDATE public.sessions
       SET approved_volunteers = quota
     WHERE id = s_id
       AND quota IS NOT NULL
       AND approved_volunteers > quota;
  END IF;

  RETURN NEW;
END$$;

DROP TRIGGER IF EXISTS trg_reg_sync_insert ON public.volunteer_registrations;
DROP TRIGGER IF EXISTS trg_reg_sync_update ON public.volunteer_registrations;
DROP TRIGGER IF EXISTS trg_reg_sync_delete ON public.volunteer_registrations;

CREATE TRIGGER trg_reg_sync_insert
AFTER INSERT ON public.volunteer_registrations
FOR EACH ROW EXECUTE FUNCTION public.sync_session_approved_count();

CREATE TRIGGER trg_reg_sync_update
AFTER UPDATE ON public.volunteer_registrations
FOR EACH ROW EXECUTE FUNCTION public.sync_session_approved_count();

CREATE TRIGGER trg_reg_sync_delete
AFTER DELETE ON public.volunteer_registrations
FOR EACH ROW EXECUTE FUNCTION public.sync_session_approved_count();

-- (D) Views cho dashboard
CREATE OR REPLACE VIEW public.v_campaign_progress AS
SELECT
  c.id,
  c.title,
  COALESCE(SUM(d.amount) FILTER (WHERE d.status='PAID'), 0)           AS donations_paid,
  COALESCE(SUM(e.amount), 0)                                          AS total_expenses,
  c.goal_amount,
  (COALESCE(SUM(d.amount) FILTER (WHERE d.status='PAID'),0)
   - COALESCE(SUM(e.amount),0))                                       AS net_amount,
  CASE
    WHEN c.goal_amount IS NULL OR c.goal_amount = 0 THEN NULL
    ELSE LEAST(100, ROUND( (COALESCE(SUM(d.amount) FILTER (WHERE d.status='PAID'),0) / c.goal_amount) * 100, 2))
  END AS percent_of_goal
FROM public.campaigns c
LEFT JOIN public.donations d ON d.campaign_id = c.id
LEFT JOIN public.expenses  e ON e.campaign_id = c.id
GROUP BY c.id;

CREATE OR REPLACE VIEW public.v_session_roster AS
SELECT
  s.id AS session_id,
  s.campaign_id,
  s.session_date,
  s.shift,
  s.quota,
  s.approved_volunteers,
  (s.quota IS NOT NULL AND s.approved_volunteers >= s.quota) AS is_full
FROM public.sessions s;

CREATE OR REPLACE VIEW public.v_reconcile_summary AS
SELECT
  ft.id            AS fund_tx_id,
  ft.campaign_id,
  ft.source,
  ft.ref_id,
  ft.amount        AS fund_amount,
  ft.occurred_at   AS fund_time,
  bs.id            AS bank_stmt_id,
  bs.amount        AS bank_amount,
  bs.txn_time      AS bank_time,
  rm.score,
  rm.decision,
  rm.decided_at
FROM public.fund_transactions ft
LEFT JOIN public.reconcile_matches rm ON rm.fund_tx_id = ft.id
LEFT JOIN public.bank_statements  bs ON bs.id = rm.bank_stmt_id;

-- (E) (Optional) Nâng cấp ON DELETE cho FK nếu trước đó tạo chưa đúng
-- Safe to run many times: drop-if-exists then add
ALTER TABLE public.sessions              DROP CONSTRAINT IF EXISTS sessions_campaign_id_fkey;
ALTER TABLE public.sessions              ADD  CONSTRAINT sessions_campaign_id_fkey
  FOREIGN KEY (campaign_id) REFERENCES public.campaigns(id) ON DELETE CASCADE;

ALTER TABLE public.donations             DROP CONSTRAINT IF EXISTS donations_campaign_id_fkey;
ALTER TABLE public.donations             ADD  CONSTRAINT donations_campaign_id_fkey
  FOREIGN KEY (campaign_id) REFERENCES public.campaigns(id) ON DELETE CASCADE;

ALTER TABLE public.fund_transactions     DROP CONSTRAINT IF EXISTS fund_transactions_campaign_id_fkey;
ALTER TABLE public.fund_transactions     ADD  CONSTRAINT fund_transactions_campaign_id_fkey
  FOREIGN KEY (campaign_id) REFERENCES public.campaigns(id) ON DELETE CASCADE;

ALTER TABLE public.expenses              DROP CONSTRAINT IF EXISTS expenses_campaign_id_fkey;
ALTER TABLE public.expenses              ADD  CONSTRAINT expenses_campaign_id_fkey
  FOREIGN KEY (campaign_id) REFERENCES public.campaigns(id) ON DELETE CASCADE;

ALTER TABLE public.posts                 DROP CONSTRAINT IF EXISTS posts_campaign_id_fkey;
ALTER TABLE public.posts                 ADD  CONSTRAINT posts_campaign_id_fkey
  FOREIGN KEY (campaign_id) REFERENCES public.campaigns(id) ON DELETE CASCADE;

ALTER TABLE public.media_assets          DROP CONSTRAINT IF EXISTS media_assets_campaign_id_fkey;
ALTER TABLE public.media_assets          ADD  CONSTRAINT media_assets_campaign_id_fkey
  FOREIGN KEY (campaign_id) REFERENCES public.campaigns(id) ON DELETE CASCADE;

ALTER TABLE public.checkins              DROP CONSTRAINT IF EXISTS checkins_session_id_fkey;
ALTER TABLE public.checkins              ADD  CONSTRAINT checkins_session_id_fkey
  FOREIGN KEY (session_id) REFERENCES public.sessions(id) ON DELETE CASCADE;

ALTER TABLE public.activity_logs         DROP CONSTRAINT IF EXISTS activity_logs_session_id_fkey;
ALTER TABLE public.activity_logs         ADD  CONSTRAINT activity_logs_session_id_fkey
  FOREIGN KEY (session_id) REFERENCES public.sessions(id) ON DELETE CASCADE;

ALTER TABLE public.expenses              DROP CONSTRAINT IF EXISTS expenses_session_id_fkey;
ALTER TABLE public.expenses              ADD  CONSTRAINT expenses_session_id_fkey
  FOREIGN KEY (session_id) REFERENCES public.sessions(id) ON DELETE SET NULL;

ALTER TABLE public.volunteer_registrations DROP CONSTRAINT IF EXISTS volunteer_registrations_session_id_fkey;
ALTER TABLE public.volunteer_registrations ADD  CONSTRAINT volunteer_registrations_session_id_fkey
  FOREIGN KEY (session_id) REFERENCES public.sessions(id) ON DELETE CASCADE;

ALTER TABLE public.user_roles            DROP CONSTRAINT IF EXISTS user_roles_user_id_fkey;
ALTER TABLE public.user_roles            ADD  CONSTRAINT user_roles_user_id_fkey
  FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;

ALTER TABLE public.volunteer_applications DROP CONSTRAINT IF EXISTS volunteer_applications_user_id_fkey;
ALTER TABLE public.volunteer_applications ADD  CONSTRAINT volunteer_applications_user_id_fkey
  FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;

ALTER TABLE public.media_assets          DROP CONSTRAINT IF EXISTS media_assets_uploader_id_fkey;
ALTER TABLE public.media_assets          ADD  CONSTRAINT media_assets_uploader_id_fkey
  FOREIGN KEY (uploader_id) REFERENCES public.users(id) ON DELETE SET NULL;

ALTER TABLE public.volunteer_registrations DROP CONSTRAINT IF EXISTS volunteer_registrations_user_id_fkey;
ALTER TABLE public.volunteer_registrations ADD  CONSTRAINT volunteer_registrations_user_id_fkey
  FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;

ALTER TABLE public.bank_statements       DROP CONSTRAINT IF EXISTS bank_statements_imported_by_fkey;
ALTER TABLE public.bank_statements       ADD  CONSTRAINT bank_statements_imported_by_fkey
  FOREIGN KEY (imported_by) REFERENCES public.users(id) ON DELETE SET NULL;

-- Kiểm tra triggers đã tạo
SELECT tgname, relname
FROM pg_trigger t
JOIN pg_class c ON c.oid = t.tgrelid
WHERE NOT t.tgisinternal
  AND relname IN ('users','campaigns','sessions','volunteer_registrations')
ORDER BY relname, tgname;

-- Kỳ vọng thấy:
-- users:    trg_users_updated_at
-- campaigns:trg_campaigns_updated_at
-- sessions: trg_sessions_updated_at
-- volunteer_registrations: trg_reg_sync_insert / update / delete

-- Kiểm tra views
SELECT table_schema, table_name
FROM information_schema.views
WHERE table_schema='public'
  AND table_name IN ('v_campaign_progress','v_session_roster','v_reconcile_summary')
ORDER BY table_name;

-- Test nhanh view
SELECT * FROM public.v_session_roster LIMIT 5;

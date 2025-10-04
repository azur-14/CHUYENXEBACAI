-- =========================
-- 00_schema.sql  (run first)
-- =========================
SET search_path TO public;
SET TIME ZONE 'Asia/Ho_Chi_Minh';

-- Extensions
CREATE EXTENSION IF NOT EXISTS pgcrypto; -- crypt(), gen_random_uuid()

-- ENUMs (create-if-missing)
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname='user_status_enum') THEN
    CREATE TYPE public.user_status_enum          AS ENUM ('ACTIVE','INACTIVE');
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname='app_review_status_enum') THEN
    CREATE TYPE public.app_review_status_enum    AS ENUM ('PENDING_REVIEW','APPROVED','REJECTED');
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname='registration_status_enum') THEN
    CREATE TYPE public.registration_status_enum  AS ENUM ('PENDING','APPROVED','REJECTED','CANCELLED');
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname='campaign_status_enum') THEN
    CREATE TYPE public.campaign_status_enum      AS ENUM ('PLANNING','ONGOING','DONE','CANCELLED');
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname='session_status_enum') THEN
    CREATE TYPE public.session_status_enum       AS ENUM ('PLANNED','ONGOING','DONE');
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname='session_shift_enum') THEN
    CREATE TYPE public.session_shift_enum        AS ENUM ('MORNING','AFTERNOON','EVENING');
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname='checkin_method_enum') THEN
    CREATE TYPE public.checkin_method_enum       AS ENUM ('QR','MANUAL');
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname='checkin_status_enum') THEN
    CREATE TYPE public.checkin_status_enum       AS ENUM ('ON_TIME','LATE','INVALID');
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname='donation_gateway_enum') THEN
    CREATE TYPE public.donation_gateway_enum     AS ENUM ('MOMO','VNPAY','STRIPE');
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname='donation_status_enum') THEN
    CREATE TYPE public.donation_status_enum      AS ENUM ('PENDING','PAID','FAILED','CANCELLED');
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname='fund_source_enum') THEN
    CREATE TYPE public.fund_source_enum          AS ENUM ('WEBHOOK','CSV','EMAIL','MANUAL');
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname='fund_status_enum') THEN
    CREATE TYPE public.fund_status_enum          AS ENUM ('PENDING','MATCHED','FLAGGED');
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname='bank_import_source_enum') THEN
    CREATE TYPE public.bank_import_source_enum   AS ENUM ('CSV','EMAIL');
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname='reconcile_decision_enum') THEN
    CREATE TYPE public.reconcile_decision_enum   AS ENUM ('AUTO','ACCEPT','REJECT','REVIEW','UNMATCH');
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname='currency_enum') THEN
    CREATE TYPE public.currency_enum             AS ENUM ('VND','USD','EUR');
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname='change_action_enum') THEN
    CREATE TYPE public.change_action_enum        AS ENUM ('CREATE','UPDATE','DELETE');
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname='post_status_enum') THEN
    CREATE TYPE public.post_status_enum          AS ENUM ('DRAFT','PUBLISHED');
  END IF;
END$$;

-- 1) Identity & Access
CREATE TABLE IF NOT EXISTS public.roles (
  id        UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  code      TEXT UNIQUE NOT NULL,
  name      TEXT
);

CREATE TABLE IF NOT EXISTS public.users (
  id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  email          TEXT UNIQUE NOT NULL,
  password_hash  TEXT NOT NULL,
  full_name      TEXT NOT NULL,
  phone          TEXT,
  avatar_url     TEXT,
  bio            TEXT,
  status         user_status_enum NOT NULL DEFAULT 'ACTIVE',
  created_at     TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at     TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS public.user_roles (
  user_id  UUID NOT NULL REFERENCES public.users(id) ON DELETE CASCADE,
  role_id  UUID NOT NULL REFERENCES public.roles(id) ON DELETE CASCADE,
  PRIMARY KEY (user_id, role_id)
);

-- 2) Volunteers
CREATE TABLE IF NOT EXISTS public.volunteer_applications (
  id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id        UUID NOT NULL REFERENCES public.users(id) ON DELETE CASCADE,
  skills         TEXT,
  availability   TEXT,
  status         app_review_status_enum NOT NULL DEFAULT 'PENDING_REVIEW',
  reviewed_by    UUID REFERENCES public.users(id),
  reviewed_at    TIMESTAMPTZ,
  reject_reason  TEXT,
  created_at     TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- 3) Campaigns & Sessions
CREATE TABLE IF NOT EXISTS public.campaigns (
  id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  title            TEXT NOT NULL,
  description      TEXT,
  location         TEXT,
  start_date       DATE,
  end_date         DATE,
  goal_amount      NUMERIC(14,2),
  collected_amount NUMERIC(14,2) DEFAULT 0,
  goal_volunteers  INT,
  status           campaign_status_enum NOT NULL,
  created_by       UUID REFERENCES public.users(id),
  created_at       TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at       TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS public.sessions (
  id                   UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  campaign_id          UUID NOT NULL REFERENCES public.campaigns(id) ON DELETE CASCADE,
  title                TEXT,
  session_date         DATE NOT NULL,
  shift                session_shift_enum NOT NULL,
  start_time           TIMESTAMPTZ,
  end_time             TIMESTAMPTZ,
  checkin_open         BOOLEAN NOT NULL DEFAULT FALSE,
  quota                INT,
  approved_volunteers  INT NOT NULL DEFAULT 0,
  status               session_status_enum NOT NULL,
  place_name           TEXT,
  lat                  DOUBLE PRECISION,
  lng                  DOUBLE PRECISION,
  geo_radius_m         INT,
  created_at           TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at           TIMESTAMPTZ NOT NULL DEFAULT now(),
  CONSTRAINT uq_session_per_campaign UNIQUE (campaign_id, session_date, shift),
  CONSTRAINT ck_session_time CHECK (start_time IS NULL OR end_time IS NULL OR start_time < end_time)
);

CREATE TABLE IF NOT EXISTS public.volunteer_registrations (
  id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id        UUID NOT NULL REFERENCES public.users(id) ON DELETE CASCADE,
  campaign_id    UUID NOT NULL REFERENCES public.campaigns(id) ON DELETE CASCADE,
  session_id     UUID REFERENCES public.sessions(id) ON DELETE CASCADE, -- NULL = campaign-level prereg
  status         registration_status_enum NOT NULL DEFAULT 'PENDING',
  applied_at     TIMESTAMPTZ NOT NULL DEFAULT now(),
  reviewed_by    UUID REFERENCES public.users(id),
  reviewed_at    TIMESTAMPTZ,
  reject_reason  TEXT
);
CREATE UNIQUE INDEX IF NOT EXISTS uq_reg_user_session
  ON public.volunteer_registrations(user_id, session_id)
  WHERE session_id IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS uq_reg_user_campaign_when_session_null
  ON public.volunteer_registrations(user_id, campaign_id)
  WHERE session_id IS NULL;

CREATE TABLE IF NOT EXISTS public.checkins (
  id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  session_id    UUID NOT NULL REFERENCES public.sessions(id) ON DELETE CASCADE,
  user_id       UUID NOT NULL REFERENCES public.users(id) ON DELETE CASCADE,
  checkin_time  TIMESTAMPTZ NOT NULL DEFAULT now(),
  method        checkin_method_enum NOT NULL,
  status        checkin_status_enum NOT NULL,
  lat           DOUBLE PRECISION,
  lng           DOUBLE PRECISION,
  evidence_note TEXT,
  CONSTRAINT uq_checkin_once_per_session UNIQUE (session_id, user_id)
);

CREATE TABLE IF NOT EXISTS public.media_assets (
  id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  campaign_id  UUID NOT NULL REFERENCES public.campaigns(id) ON DELETE CASCADE,
  uploader_id  UUID REFERENCES public.users(id) ON DELETE SET NULL,
  checkin_id   UUID NOT NULL REFERENCES public.checkins(id) ON DELETE CASCADE,
  url          TEXT NOT NULL,
  public_id    TEXT,
  thumb_url    TEXT,
  format       TEXT,
  uploaded_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS public.activity_logs (
  id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  session_id    UUID NOT NULL REFERENCES public.sessions(id) ON DELETE CASCADE,
  activity_type TEXT NOT NULL,
  performed_by  UUID REFERENCES public.users(id),
  started_at    TIMESTAMPTZ,
  ended_at      TIMESTAMPTZ,
  note          TEXT
);

CREATE TABLE IF NOT EXISTS public.expenses (
  id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  campaign_id    UUID NOT NULL REFERENCES public.campaigns(id) ON DELETE CASCADE,
  session_id     UUID REFERENCES public.sessions(id) ON DELETE SET NULL, -- NULL = campaign-level
  occurred_at    TIMESTAMPTZ NOT NULL DEFAULT now(),
  category       TEXT,
  description    TEXT,
  amount         NUMERIC(14,2) NOT NULL,
  currency       currency_enum NOT NULL DEFAULT 'VND',
  payment_method TEXT,
  payer_id       UUID REFERENCES public.users(id),
  receipt_url    TEXT,
  note           TEXT,
  CONSTRAINT ck_expense_amount_positive CHECK (amount >= 0)
);

-- 4) Inflow & Reconciliation
CREATE TABLE IF NOT EXISTS public.donations (
  id                 UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  campaign_id        UUID NOT NULL REFERENCES public.campaigns(id) ON DELETE CASCADE,
  donor_name         TEXT,
  donor_email        TEXT,
  amount             NUMERIC(14,2) NOT NULL,
  currency           currency_enum NOT NULL DEFAULT 'VND',
  wish_to_show_name  BOOLEAN NOT NULL DEFAULT TRUE,
  message            TEXT,
  created_at         TIMESTAMPTZ NOT NULL DEFAULT now(),
  gateway            donation_gateway_enum,
  order_code         TEXT UNIQUE,
  status             donation_status_enum,
  paid_at            TIMESTAMPTZ,
  CONSTRAINT ck_donation_amount_positive CHECK (amount >= 0)
);

CREATE TABLE IF NOT EXISTS public.fund_transactions (
  id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  campaign_id  UUID NOT NULL REFERENCES public.campaigns(id) ON DELETE CASCADE,
  source       fund_source_enum NOT NULL,
  ref_id       TEXT NOT NULL,
  amount       NUMERIC(14,2) NOT NULL,
  occurred_at  TIMESTAMPTZ NOT NULL,
  status       fund_status_enum NOT NULL DEFAULT 'PENDING',
  donation_id  UUID REFERENCES public.donations(id) ON DELETE SET NULL,
  created_at   TIMESTAMPTZ NOT NULL DEFAULT now(),
  CONSTRAINT ck_fund_amount_positive CHECK (amount >= 0)
);
CREATE UNIQUE INDEX IF NOT EXISTS uq_fund_ref_per_source
  ON public.fund_transactions(source, ref_id);

CREATE TABLE IF NOT EXISTS public.bank_statements (
  id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  bank_ref      TEXT,
  description   TEXT,
  amount        NUMERIC(14,2) NOT NULL,
  txn_time      TIMESTAMPTZ NOT NULL,
  account_no    TEXT,
  raw           JSONB,
  source        bank_import_source_enum NOT NULL,
  file_name     TEXT,
  imported_by   UUID REFERENCES public.users(id) ON DELETE SET NULL,
  imported_at   TIMESTAMPTZ NOT NULL DEFAULT now(),
  notes         TEXT,
  CONSTRAINT ck_bank_amount_positive CHECK (amount >= 0)
);
CREATE INDEX IF NOT EXISTS idx_bank_ref ON public.bank_statements(bank_ref);
CREATE INDEX IF NOT EXISTS idx_bank_txn_time ON public.bank_statements(txn_time);

CREATE TABLE IF NOT EXISTS public.reconcile_matches (
  id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  fund_tx_id       UUID NOT NULL REFERENCES public.fund_transactions(id) ON DELETE CASCADE,
  bank_stmt_id     UUID NOT NULL REFERENCES public.bank_statements(id) ON DELETE CASCADE,
  score            NUMERIC(6,3),
  decision         reconcile_decision_enum,
  decided_by       UUID REFERENCES public.users(id),
  decided_at       TIMESTAMPTZ,
  note             TEXT,
  decision_history JSONB
);
CREATE UNIQUE INDEX IF NOT EXISTS uq_match_pair
  ON public.reconcile_matches(fund_tx_id, bank_stmt_id);

-- 5) Content & Comms
CREATE TABLE IF NOT EXISTS public.posts (
  id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  campaign_id   UUID NOT NULL REFERENCES public.campaigns(id) ON DELETE CASCADE,
  title         TEXT NOT NULL,
  content_md    TEXT,
  cover_url     TEXT,
  status        post_status_enum NOT NULL DEFAULT 'DRAFT',
  published_at  TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS public.faqs (
  id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  question   TEXT NOT NULL,
  answer_md  TEXT,
  tags       TEXT[],
  order_no   INT
);

CREATE TABLE IF NOT EXISTS public.newsletter_subscriptions (
  id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  email      TEXT UNIQUE NOT NULL,
  consent    BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- 6) Audit
CREATE TABLE IF NOT EXISTS public.change_logs (
  id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  entity_type  TEXT NOT NULL,
  entity_id    UUID NOT NULL,
  action       change_action_enum NOT NULL,
  changed_by   UUID REFERENCES public.users(id) ON DELETE SET NULL,
  before_data  JSONB,
  after_data   JSONB,
  changed_at   TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS idx_changelog_entity
  ON public.change_logs(entity_type, entity_id);

-- Helper indexes
CREATE INDEX IF NOT EXISTS idx_sessions_campaign   ON public.sessions(campaign_id);
CREATE INDEX IF NOT EXISTS idx_checkins_session    ON public.checkins(session_id);
CREATE INDEX IF NOT EXISTS idx_media_campaign      ON public.media_assets(campaign_id);
CREATE INDEX IF NOT EXISTS idx_expenses_campaign   ON public.expenses(campaign_id);
CREATE INDEX IF NOT EXISTS idx_expenses_session    ON public.expenses(session_id);
CREATE INDEX IF NOT EXISTS idx_donations_campaign  ON public.donations(campaign_id);
CREATE INDEX IF NOT EXISTS idx_fund_campaign       ON public.fund_transactions(campaign_id);

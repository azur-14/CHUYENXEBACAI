SET search_path TO public;
SET TIME ZONE 'Asia/Ho_Chi_Minh';

BEGIN;

-- Roles
INSERT INTO roles(code, name) VALUES
  ('ADMIN','Administrator'),
  ('VOLUNTEER','Volunteer'),
  ('ORG','Organizer')
ON CONFLICT (code) DO NOTHING;

-- Admin user (pass: admin123)
INSERT INTO users(email, password_hash, full_name)
VALUES ('admin@example.com', crypt('admin123', gen_salt('bf')), 'Admin User')
ON CONFLICT (email) DO NOTHING;

-- Gán quyền ADMIN
INSERT INTO user_roles(user_id, role_id)
SELECT u.id, r.id
FROM users u
JOIN roles r ON r.code='ADMIN'
WHERE u.email='admin@example.com'
ON CONFLICT DO NOTHING;

-- Campaign demo
WITH created_campaign AS (
  INSERT INTO campaigns(
    title, description, location, start_date, end_date, goal_amount, status, created_by
  )
  SELECT
    'Trung Thu Ấm Áp', 'Ủng hộ trẻ em vùng cao', 'Hà Giang',
    CURRENT_DATE, CURRENT_DATE + INTERVAL '15 days',
    100000000, 'ONGOING'::campaign_status_enum, u.id
  FROM users u WHERE u.email='admin@example.com'
  ON CONFLICT DO NOTHING
  RETURNING id
),
target_campaign AS (
  SELECT id FROM created_campaign
  UNION ALL
  SELECT id FROM campaigns WHERE title='Trung Thu Ấm Áp'
  LIMIT 1
)

-- Session 1
INSERT INTO sessions(
  campaign_id, title, session_date, shift,
  start_time, end_time, quota, status,
  place_name, lat, lng, geo_radius_m
)
SELECT
  tc.id, 'Phát quà đợt 1', CURRENT_DATE + INTERVAL '1 day',
  'MORNING'::session_shift_enum,
  now(), now() + interval '3 hours',
  50,
  'PLANNED'::session_status_enum,
  'Trường A', 22.83, 104.98, 200
FROM target_campaign tc
ON CONFLICT (campaign_id, session_date, shift) DO NOTHING;

-- Session 2
WITH target_campaign AS (
  SELECT id FROM campaigns WHERE title='Trung Thu Ấm Áp' LIMIT 1
)
INSERT INTO sessions(
  campaign_id, title, session_date, shift,
  start_time, end_time, quota, status,
  place_name, lat, lng, geo_radius_m
)
SELECT
  tc.id, 'Phát quà đợt 2', CURRENT_DATE + INTERVAL '2 days',
  'AFTERNOON'::session_shift_enum,
  now(), now() + interval '3 hours',
  40,
  'PLANNED'::session_status_enum,
  'Trường B', 22.85, 105.00, 200
FROM target_campaign tc
ON CONFLICT (campaign_id, session_date, shift) DO NOTHING;

COMMIT;

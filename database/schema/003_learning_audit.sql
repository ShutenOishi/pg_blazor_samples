-- CI専用監査テーブル。実運用DBへ自動適用しない。
-- アプリ権限はINSERT、監査官はSELECTを原則として適切に分離する。
CREATE TABLE IF NOT EXISTS learning_operation_audit_ci
(
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    actor_login varchar(129) NOT NULL,
    operation varchar(48) NOT NULL,
    unit_code varchar(32) NOT NULL,
    target_row_id bigint NOT NULL,
    row_count integer NOT NULL CHECK (row_count >= 0),
    outcome varchar(32) NOT NULL,
    correlation_id uuid NOT NULL,
    occurred_utc timestamptz NOT NULL
);
CREATE INDEX IF NOT EXISTS learning_operation_audit_actor_time_idx
    ON learning_operation_audit_ci (actor_login, occurred_utc DESC);

-- CIだけで監査障害を再現し、監査失敗時に業務更新がロールバックされることを確認。
CREATE OR REPLACE FUNCTION learning_audit_ci_failure() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
    IF position('AUDITFAIL' IN NEW.actor_login) > 0 THEN
        RAISE EXCEPTION 'CI専用：監査のINSERTを故意に拒否';
    END IF;
    RETURN NEW;
END;
$$;
DROP TRIGGER IF EXISTS learning_audit_ci_failure_trigger ON learning_operation_audit_ci;
CREATE TRIGGER learning_audit_ci_failure_trigger
BEFORE INSERT ON learning_operation_audit_ci
FOR EACH ROW EXECUTE FUNCTION learning_audit_ci_failure();

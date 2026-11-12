-- Creates the two database roles from ADR-0009 and gives them their rights.
--
-- Run as the database's admin (superuser), connected to the app database:
--
--   psql -v ON_ERROR_STOP=1 -v migrator_password=… -v app_password=… -f deploy/db-roles.sql
--
-- Safe to run again: it then only sets the given passwords, so rotating them means re-running it.

-- psql doesn't interpolate variables inside DO $$ … $$, so the roles are created with \gexec.
SELECT 'CREATE ROLE musialize_migrator'
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'musialize_migrator') \gexec
SELECT 'CREATE ROLE musialize_app'
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'musialize_app') \gexec

ALTER ROLE musialize_migrator WITH LOGIN PASSWORD :'migrator_password';
ALTER ROLE musialize_app WITH LOGIN PASSWORD :'app_password';

-- The migrator owns the schema, so it creates and changes every table in it.
-- The app may use the schema but not create anything in it.
ALTER SCHEMA public OWNER TO musialize_migrator;
REVOKE CREATE ON SCHEMA public FROM PUBLIC;
GRANT USAGE ON SCHEMA public TO musialize_app;

-- Every table and sequence the migrator creates, including the migrations history table,
-- lets the app read and write data, but not change the schema.
ALTER DEFAULT PRIVILEGES FOR ROLE musialize_migrator IN SCHEMA public
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO musialize_app;
ALTER DEFAULT PRIVILEGES FOR ROLE musialize_migrator IN SCHEMA public
    GRANT USAGE ON SEQUENCES TO musialize_app;

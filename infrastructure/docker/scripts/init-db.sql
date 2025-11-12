-- Initialize PostgreSQL Database for User Service
-- This script runs automatically when the PostgreSQL container starts

-- Create database if not exists (already created via POSTGRES_DB env var)
-- Additional databases can be created here if needed

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Set timezone
SET timezone = 'UTC';

-- Create schema for user service (optional, using public schema by default)
-- CREATE SCHEMA IF NOT EXISTS userservice;

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE userservice TO postgres;

-- Log initialization
DO $$
BEGIN
    RAISE NOTICE 'Database initialization completed successfully';
    RAISE NOTICE 'Database: userservice';
    RAISE NOTICE 'Extensions: uuid-ossp, pgcrypto';
    RAISE NOTICE 'Timezone: UTC';
END $$;

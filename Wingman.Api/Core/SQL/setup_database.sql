CREATE DATABASE "Wingman";

CREATE TABLE "Users" (
    "Id" UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    "Email" VARCHAR(100) UNIQUE NOT NULL,
    "PasswordHash" CHAR(84) NOT NULL,
    "RefreshToken" CHAR(44),
    "RefreshTokenExpirationDateTimeUTC" TIMESTAMPTZ
);
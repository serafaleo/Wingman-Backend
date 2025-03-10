CREATE DATABASE "Wingman";

CREATE TABLE "Users" (
    "Id" UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    "Email" VARCHAR(100) UNIQUE NOT NULL,
    "PasswordHash" CHAR(84) NOT NULL,
    "RefreshToken" CHAR(44),
    "RefreshTokenExpirationDateTimeUTC" TIMESTAMPTZ
);

CREATE TABLE "Aircrafts" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "Registration" VARCHAR(7) NOT NULL,
    "TypeICAO" VARCHAR(4) NOT NULL,
    CONSTRAINT "FK_Aircrafts_Users" FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE
);

CREATE TABLE "Flights" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "AircraftId" UUID NOT NULL,
    "Status" INT NOT NULL,
    "DepartureDateTimeUTC" TIMESTAMPTZ NOT NULL,
    "DepartureICAO" VARCHAR(4) NOT NULL,
    "ArrivalICAO" VARCHAR(4) NOT NULL,
    "AlternateICAO" VARCHAR(4) NOT NULL,
    "Duration" INTERVAL NOT NULL,
    CONSTRAINT "FK_Flights_Users" FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Flights_Aircrafts" FOREIGN KEY ("AircraftId") REFERENCES "Aircrafts"("Id") ON DELETE CASCADE
);

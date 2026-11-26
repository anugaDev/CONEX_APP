-- Esquema de la base de datos de CONEX APP.
--
-- Es la ÚNICA definición del esquema. La usan:
--   * la aplicación al arrancar (Infrastructure/Data/DatabaseInitializer.cs, como recurso incrustado)
--   * tools/rebuild_db.ps1, para crear una BD nueva antes de cargar import_data.sql
--
-- Todas las sentencias deben poder ejecutarse sobre una BD que ya existe sin cambiar nada
-- (CREATE ... IF NOT EXISTS, INSERT OR IGNORE). Si añades una columna a una tabla que ya existe,
-- añádela aquí Y en DatabaseInitializer.ColumnUpgrades para que se cree también en las BD antiguas.

CREATE TABLE IF NOT EXISTS "Users" (
    "Id"            INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
    "Name"          TEXT    NOT NULL,
    "Email"         TEXT    NOT NULL DEFAULT '',
    "CreatedAt"     TEXT    NOT NULL,
    "Address"       TEXT    NOT NULL DEFAULT '',
    "DischargedAt"  TEXT    NOT NULL DEFAULT '0001-01-01 00:00:00',
    "IdCard"        TEXT    NOT NULL DEFAULT '',
    "IsPartner"     INTEGER NOT NULL DEFAULT 0,
    "Location"      TEXT    NOT NULL DEFAULT '',
    "Phone"         TEXT    NOT NULL DEFAULT '',
    "SecondSurname" TEXT    NOT NULL DEFAULT '',
    "Surname"       TEXT    NOT NULL DEFAULT '',
    "IsTutor"       INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS "Activities" (
    "Id"          INTEGER NOT NULL CONSTRAINT "PK_Activities" PRIMARY KEY AUTOINCREMENT,
    "Name"        TEXT    NOT NULL,
    "Tutor"       TEXT    NOT NULL,
    "Classroom"   TEXT    NOT NULL,
    "Date"        TEXT    NOT NULL,
    "CreatedAt"   TEXT    NOT NULL,
    "MaxStudents" INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS "ActivityUser" (
    "ActivitiesId" INTEGER NOT NULL,
    "StudentsId"   INTEGER NOT NULL,
    CONSTRAINT "PK_ActivityUser" PRIMARY KEY ("ActivitiesId", "StudentsId"),
    CONSTRAINT "FK_ActivityUser_Activities_ActivitiesId" FOREIGN KEY ("ActivitiesId") REFERENCES "Activities" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ActivityUser_Users_StudentsId" FOREIGN KEY ("StudentsId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS "IX_ActivityUser_StudentsId" ON "ActivityUser" ("StudentsId");

CREATE TABLE IF NOT EXISTS "Renewals" (
    "Id"              INTEGER PRIMARY KEY AUTOINCREMENT,
    "UserId"          INTEGER NOT NULL,
    "RenewalDate"     TEXT    NOT NULL,
    "NextRenewalDate" TEXT    NULL,
    "RenewalAmount"   TEXT    NULL,
    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS "IX_Renewals_UserId" ON "Renewals" ("UserId");

-- Copia de las clases incluidas en cada recibo, tal como estaban al renovar,
-- para poder reimprimir el recibo idéntico aunque luego cambien las clases o los precios.
CREATE TABLE IF NOT EXISTS "RenewalClasses" (
    "Id"           INTEGER PRIMARY KEY AUTOINCREMENT,
    "RenewalId"    INTEGER NOT NULL,
    "ActivityName" TEXT    NOT NULL,
    "Tutor"        TEXT    NOT NULL DEFAULT '',
    "Schedule"     TEXT    NOT NULL DEFAULT '',
    "Price"        TEXT    NOT NULL DEFAULT '0',
    FOREIGN KEY ("RenewalId") REFERENCES "Renewals" ("Id") ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS "IX_RenewalClasses_RenewalId" ON "RenewalClasses" ("RenewalId");

CREATE TABLE IF NOT EXISTS "AppSettings" (
    "Id"                  INTEGER PRIMARY KEY,
    "RenewalPeriodMonths" INTEGER NOT NULL DEFAULT 12,
    "RenewalCost"         TEXT    NOT NULL DEFAULT '0',
    "ClassCost"           TEXT    NOT NULL DEFAULT '0'
);
INSERT OR IGNORE INTO "AppSettings" ("Id", "RenewalPeriodMonths", "RenewalCost", "ClassCost") VALUES (1, 12, '0', '0');

CREATE TABLE IF NOT EXISTS "Waitlists" (
    "Id"         INTEGER PRIMARY KEY AUTOINCREMENT,
    "UserId"     INTEGER NOT NULL,
    "ActivityId" INTEGER NOT NULL,
    "JoinedAt"   TEXT    NOT NULL,
    FOREIGN KEY ("UserId")     REFERENCES "Users" ("Id")      ON DELETE CASCADE,
    FOREIGN KEY ("ActivityId") REFERENCES "Activities" ("Id") ON DELETE CASCADE
);
-- Un usuario solo puede estar una vez en la lista de espera de cada clase.
DELETE FROM "Waitlists"
 WHERE "Id" NOT IN (SELECT MIN("Id") FROM "Waitlists" GROUP BY "UserId", "ActivityId");
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Waitlists_UserId_ActivityId" ON "Waitlists" ("UserId", "ActivityId");
CREATE INDEX IF NOT EXISTS "IX_Waitlists_ActivityId" ON "Waitlists" ("ActivityId");

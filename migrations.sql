BEGIN TRANSACTION;
CREATE TABLE "Reviews" (
    "ReviewId" INTEGER NOT NULL CONSTRAINT "PK_Reviews" PRIMARY KEY AUTOINCREMENT,
    "Title" TEXT NOT NULL,
    "Author" TEXT NOT NULL,
    "Rating" INTEGER NOT NULL,
    "Content" TEXT NOT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260228034359_Reviews1', '10.0.2');

COMMIT;

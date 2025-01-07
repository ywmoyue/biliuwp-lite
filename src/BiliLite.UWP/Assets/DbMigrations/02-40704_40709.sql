CREATE TABLE IF NOT EXISTS "PageSavedItems" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_PageSavedItems" PRIMARY KEY,
    "Url" TEXT NULL,
    "Parameters" TEXT NULL,
    "Type" TEXT NULL,
    "Title" TEXT NULL,
    "Icon" TEXT NULL
);
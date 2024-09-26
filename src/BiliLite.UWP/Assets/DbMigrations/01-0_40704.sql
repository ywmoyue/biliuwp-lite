-- 创建 DownloadedItems 表
CREATE TABLE IF NOT EXISTS DownloadedItems (
    "ID" TEXT NOT NULL CONSTRAINT "PK_DownloadedItems" PRIMARY KEY,
    "IsSeason" INTEGER NOT NULL,
    "CoverPath" TEXT NULL,
    "Title" TEXT NULL,
    "UpdateTime" TEXT NOT NULL,
    "Path" TEXT NULL
);

-- 创建 DownloadedSubItems 表
CREATE TABLE IF NOT EXISTS "DownloadedSubItems" (
    "CID" TEXT NOT NULL CONSTRAINT "PK_DownloadedSubItems" PRIMARY KEY,
    "EpisodeID" TEXT NULL,
    "Title" TEXT NULL,
    "IsDash" INTEGER NOT NULL,
    "QualityID" INTEGER NOT NULL,
    "QualityName" TEXT NULL,
    "PathList" TEXT NULL,
    "DanmakuPath" TEXT NULL,
    "Index" INTEGER NOT NULL,
    "FilePath" TEXT NULL,
    "SubtitlePaths" TEXT NULL,
    "DownloadedItemDTOID" TEXT NULL,
    CONSTRAINT "FK_DownloadedSubItems_DownloadedItems_DownloadedItemDTOID" FOREIGN KEY ("DownloadedItemDTOID") REFERENCES "DownloadedItems" ("ID") ON DELETE RESTRICT
);

CREATE INDEX "IX_DownloadedSubItems_DownloadedItemDTOID" ON "DownloadedSubItems" ("DownloadedItemDTOID");
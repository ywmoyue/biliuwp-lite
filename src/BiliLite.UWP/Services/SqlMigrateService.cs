using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BiliLite.Models.Common;
using Windows.Storage;
using BiliLite.Extensions;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BiliLite.Services
{
    public class SqlMigrateService
    {
        private readonly BiliLiteDbContext m_biliLiteDbContext;
        private StorageFolder m_migrateFolder;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public SqlMigrateService(BiliLiteDbContext biliLiteDbContext)
        {
            m_biliLiteDbContext = biliLiteDbContext;
        }

        public async Task MigrateDatabase()
        {
            try
            {
                var currentAppVersion = GetCurrentAppVersion();
                var currentDbVersion = GetCurrentDatabaseVersion();
                if (currentDbVersion == currentAppVersion) return;
                var migrationScripts = await GetMigrationScriptsToRun(currentDbVersion);
                foreach (var script in migrationScripts)
                {
                    await ExecuteMigrationScript(script);
                }
                UpdateDatabaseVersion();
            }
            catch (Exception ex)
            {
                _logger.Error("迁移数据库错误",ex);
            }
        }

        private async Task<StorageFolder> GetMigrationsFolder()
        {
            if (m_migrateFolder != null) return m_migrateFolder;
            var installedLocation = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var assetsFolder = await installedLocation.GetFolderAsync("Assets");
            m_migrateFolder = await assetsFolder.GetFolderAsync("DbMigrations");
            return m_migrateFolder;
        }

        private int GetCurrentDatabaseVersion()
        {
            var version = SettingService.GetValue(SettingConstants.Other.SQL_DB_VERSION, 0);
            return version;
        }

        private async Task<List<MigrationScriptInfo>> GetMigrationScriptsToRun(int currentVersion)
        {
            var folder = await GetMigrationsFolder();
            var files = await folder.GetFilesAsync();
            var migrationScripts = new List<MigrationScriptInfo>();

            foreach (var file in files)
            {
                var migrationInfo = ExtractMigrationInfo(file.Name);
                migrationInfo.File = file;
                if (migrationInfo.AfterVersion.ToInt32() > currentVersion)
                {
                    migrationScripts.Add(migrationInfo);
                }
            }

            return migrationScripts.OrderBy(script => script.SequenceNumber).ToList();
        }

        private async Task ExecuteMigrationScript(MigrationScriptInfo scriptInfo)
        {
            var content = await FileIO.ReadTextAsync(scriptInfo.File);

            // Execute the SQL script content against the database
            // Assuming m_biliLiteDbContext can execute raw SQL commands
            var result = await m_biliLiteDbContext.Database.ExecuteSqlRawAsync(content);
        }

        private void UpdateDatabaseVersion()
        {
            SettingService.SetValue(SettingConstants.Other.SQL_DB_VERSION, GetCurrentAppVersion());
        }

        private int GetCurrentAppVersion()
        {
            var num = ($"{SystemInformation.ApplicationVersion.Major}{SystemInformation.ApplicationVersion.Minor.ToString("00")}" +
                      $"{SystemInformation.ApplicationVersion.Build.ToString("00")}").ToInt32();
            return num.ToInt32();
        }

        private static MigrationScriptInfo ExtractMigrationInfo(string fileName)
        {
            // 正则表达式匹配文件名格式：序号-升级前版本_升级后版本.sql
            var regex = new Regex(@"^(\d+)-(\d+)_(\d+)\.sql$");

            var match = regex.Match(fileName);
            if (match.Success)
            {
                return new MigrationScriptInfo
                {
                    SequenceNumber = int.Parse(match.Groups[1].Value),
                    BeforeVersion = match.Groups[2].Value,
                    AfterVersion = match.Groups[3].Value,
                    FileName = fileName,
                };
            }
            else
            {
                throw new ArgumentException("The file name does not match the expected format.");
            }
        }

        public class MigrationScriptInfo
        {
            public int SequenceNumber { get; set; }

            public string BeforeVersion { get; set; }

            public string AfterVersion { get; set; }

            public string FileName { get; set; }

            public StorageFile File { get; set; }
        }
    }
}

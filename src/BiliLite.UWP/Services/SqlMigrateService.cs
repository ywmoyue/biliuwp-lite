using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BiliLite.Models.Common;
using Windows.Storage;
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

        /// <summary>
        /// APP 版本号（四段全部参与比较）与上次迁移的版本不一致时，执行 DbMigrations 下的全部脚本。
        /// 也就是说：正常升级、只改第四段 Revision 的热修、以及降级重装都会重新走一遍迁移，
        /// 因此 Assets/DbMigrations 下的每个脚本都必须可重复执行：
        /// 建表建索引一律带 IF NOT EXISTS，加列先判断列是否存在，数据回填要写成重复执行结果不变。
        /// 脚本按文件名序号（NN-升级前版本_升级后版本.sql）升序执行。
        /// </summary>
        public async Task MigrateDatabase()
        {
            try
            {
                var currentAppVersion = GetCurrentAppVersion();
                if (GetMigratedVersion() == currentAppVersion) return;

                var migrationScripts = await GetMigrationScriptsToRun();
                foreach (var script in migrationScripts)
                {
                    await ExecuteMigrationScript(script);
                }

                UpdateDatabaseVersion(currentAppVersion);
            }
            catch (Exception ex)
            {
                _logger.Error("迁移数据库错误", ex);
            }
        }

        /// <summary>
        /// 开发调试用：不校验版本，直接按顺序重放全部迁移脚本（脚本需可重复执行）
        /// </summary>
        public async Task ExcuteAllMigrationScripts()
        {
            try
            {
                var migrationScripts = await GetMigrationScriptsToRun();
                foreach (var script in migrationScripts)
                {
                    await ExecuteMigrationScript(script);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("迁移数据库错误", ex);
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

        private static string GetMigratedVersion()
        {
            var key = SettingConstants.Other.SQL_DB_VERSION;
            if (!SettingService.HasValue(key)) return string.Empty;
            return SettingService.TryGetValue(key, string.Empty, out var version)
                ? version
                : string.Empty;
        }

        private async Task<List<MigrationScriptInfo>> GetMigrationScriptsToRun()
        {
            var folder = await GetMigrationsFolder();
            var files = await folder.GetFilesAsync();
            var migrationScripts = new List<MigrationScriptInfo>();

            foreach (var file in files)
            {
                var migrationInfo = ExtractMigrationInfo(file.Name);
                migrationInfo.File = file;
                migrationScripts.Add(migrationInfo);
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

        private static void UpdateDatabaseVersion(string version)
        {
            SettingService.SetValue(SettingConstants.Other.SQL_DB_VERSION, version);
        }

        /// <summary>
        /// 完整版本号字符串：Major.Minor.Build.Revision。
        /// 四段都取，保证只改 Revision 的更新也能触发迁移。
        /// </summary>
        private static string GetCurrentAppVersion()
        {
            var version = SystemInformation.ApplicationVersion;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
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

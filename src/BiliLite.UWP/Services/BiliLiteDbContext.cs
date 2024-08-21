using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Windows.Storage;
using BiliLite.Models.Databases;

namespace BiliLite.Services
{
    public class BiliLiteDbContext : DbContext
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private static readonly string _dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "bililite.db3");

        public BiliLiteDbContext()
        {
            try
            {
                SQLitePCL.Batteries.Init();
                Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                _logger.Error("初始化sqlite失败", ex);
                throw ex;
            }
        }

        public DbSet<SettingItem> SettingItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite($"Filename={_dbPath}");
        }
    }
}

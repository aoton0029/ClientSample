using ClientSample.Dbs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ClientSample.Dbs
{
    public class WorkflowDbContext : DbContext
    {
        public DbSet<DbTemplateType> TemplateTypes { get; set; }
        public DbSet<DbConditionType> ConditionTypes { get; set; }
        public DbSet<DbCommandType> CommandTypes { get; set; }
        public DbSet<DbKeyMaster> KeyMasters { get; set; }

        public DbSet<DbTemplate> Templates { get; set; }
        public DbSet<DbCondition> Conditions { get; set; }
        public DbSet<DbCommand> Commands { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 実行ディレクトリのパスを取得
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = Path.Combine(basePath, "workflow.db");

            // SQLite データベースのパスを設定
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // TemplateType 初期データ
            modelBuilder.Entity<DbTemplateType>().HasData(
                new DbTemplateType { TemplateTypeID = 1, Name = "Threshold" },
                new DbTemplateType { TemplateTypeID = 2, Name = "Case" },
                new DbTemplateType { TemplateTypeID = 3, Name = "End" }
            );

            // ConditionType 初期データ
            modelBuilder.Entity<DbConditionType>().HasData(
                new DbConditionType { ConditionTypeID = 1, Name = "threshold" },
                new DbConditionType { ConditionTypeID = 2, Name = "case" }
            );

            // CommandType 初期データ
            modelBuilder.Entity<DbCommandType>().HasData(
                new DbCommandType { CommandTypeID = 1, Name = "Send" },
                new DbCommandType { CommandTypeID = 2, Name = "Receive" }
            );

            // KeyMaster 初期データ
            modelBuilder.Entity<DbKeyMaster>().HasData(
                new DbKeyMaster { KeyID = 1, Name = "threshold" },
                new DbKeyMaster { KeyID = 2, Name = "SUCCESS" },
                new DbKeyMaster { KeyID = 3, Name = "FAILURE" }
            );
        }
    }
}
using Microsoft.EntityFrameworkCore;
using SimpleETL.Models;
using System.IO;
using System;

namespace SimpleETL.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<DataFlow> DataFlows { get; set; }
        public DbSet<JobLog> JobLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "simpleetl.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}

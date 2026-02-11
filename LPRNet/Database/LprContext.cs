using LPRNet.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace LPRNet.Database
{
    public class LprContext : DbContext
    {
        public DbSet<PlateRecord> PlateRecords { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lpr_log.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        public async Task AddRecordAsync(PlateRecord record)
        {
            await PlateRecords.AddAsync(record);
            await SaveChangesAsync();
        }

        public async Task<List<PlateRecord>> SearchByPlateAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await PlateRecords.OrderByDescending(r => r.Timestamp).ToListAsync();

            return await PlateRecords
                .Where(r => r.PlateNumber.Contains(query) || r.Country.Contains(query))
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();
        }

        public async Task ClearAllRecordsAsync()
        {
            await PlateRecords.ExecuteDeleteAsync();
        }
    }
}
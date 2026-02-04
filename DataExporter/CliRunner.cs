using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using DataExporter.Core;
using DataExporter.Models;
using DataExporter.Services;
using Serilog;
using Dapper;

namespace DataExporter
{
    public static class CliRunner
    {
        public static void Run()
        {
            Log.Information("=== Mode: CLI ===");

            try
            {
                // 1. Setup Mock Database
                string dbFile = "mydb.sqlite";
                
                // Microsoft.Data.Sqlite creates file automatically if Mode=ReadWriteCreate (default)
                // But we can ensure it exists.
                if (!File.Exists(dbFile))
                {
                    // Just let the connection create it, or:
                    // File.Create(dbFile).Close();
                }

                var dbService = new SqliteDataSource($"Data Source={dbFile}");
                SeedDatabase(dbService);

                // 2. Define Jobs
                var jobs = new List<ExportJob>
                {
                    new ExportJob
                    {
                        JobName = "ExportUsersToJson",
                        Query = "SELECT * FROM Users",
                        OutputFormat = "json",
                        OutputPath = "output/users_cli.json",
                        CompressOutput = true,
                        MaskFields = new List<string> { "Email", "Salary" }
                    },
                    new ExportJob
                    {
                        JobName = "ExportProductsToCsv",
                        Query = "SELECT * FROM Products",
                        OutputFormat = "csv",
                        OutputPath = "output/products_cli.csv",
                        CompressOutput = false
                    }
                };

                // 3. Ensure Output Directory Exists
                if (!Directory.Exists("output")) Directory.CreateDirectory("output");

                // 4. Execute Jobs
                foreach (var job in jobs)
                {
                    if (string.IsNullOrEmpty(job.JobName)) continue;
                    Log.Information($"Starting Job: {job.JobName}");

                    if (string.IsNullOrEmpty(job.Query)) 
                    {
                        Log.Warning($"Job {job.JobName} has empty query.");
                        continue;
                    }

                    var data = dbService.GetData(job.Query);
                    data = MaskingService.ApplyMasking(data, job.MaskFields);

                    IExporter exporter = (job.OutputFormat ?? "").ToLower() == "csv" 
                        ? (IExporter)new CsvExporter() 
                        : (IExporter)new JsonExporter();

                    string outputPath = job.OutputPath ?? $"output/{job.JobName}.txt";
                    exporter.Export(data, outputPath);

                    if (job.CompressOutput)
                    {
                        string zipPath = outputPath + ".zip";
                        CompressionService.CompressFile(outputPath, zipPath);
                    }

                    Log.Information($"Job {job.JobName} Completed Successfully!");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CLI Error");
            }
        }

        static void SeedDatabase(IDataSource db)
        {
            // Simple seed check
            db.Execute("CREATE TABLE IF NOT EXISTS Users (Id INTEGER PRIMARY KEY, Name TEXT, Email TEXT, Salary REAL)");
            db.Execute("CREATE TABLE IF NOT EXISTS Products (Id INTEGER PRIMARY KEY, ProductName TEXT, Price REAL, Stock INTEGER)");
            
            db.Execute("DELETE FROM Users");
            db.Execute("DELETE FROM Products");

            db.Execute("INSERT INTO Users (Name, Email, Salary) VALUES ('Jacky', 'jacky@gravicode.com', 9000000)");
            db.Execute("INSERT INTO Users (Name, Email, Salary) VALUES ('Fadhil', 'fadhil@gravicode.com', 15000000)");
            db.Execute("INSERT INTO Products (ProductName, Price, Stock) VALUES ('Laptop Gaming', 25000000, 10)");
        }
    }
}

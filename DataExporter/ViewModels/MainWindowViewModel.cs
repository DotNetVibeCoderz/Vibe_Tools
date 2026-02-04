using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using DataExporter.Core;
using DataExporter.Helpers;
using DataExporter.Models;
using DataExporter.Services;
using ReactiveUI;
using Serilog;
using Microsoft.Data.SqlClient;
using Npgsql;
using MySql.Data.MySqlClient;
using Microsoft.Data.Sqlite;
using Dapper;

namespace DataExporter.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _selectedDatabaseType = "SQLite";
        private string _connectionString = "Data Source=mydb.sqlite"; 
        private string _query = "SELECT * FROM Users";
        private string _selectedFormat = "JSON";
        private bool _compress = false;
        private bool _masking = false;
        private string _maskFields = "Email, Salary";
        private bool _isBusy = false;
        private string _lastExportPath = "";

        // Database Types for UI
        public ObservableCollection<string> DatabaseTypes { get; } = new ObservableCollection<string>
        {
            "SQL Server", "PostgreSQL", "MySQL", "Oracle", "MariaDB", "IBM Db2", "Snowflake", "SQLite"
        };

        public string SelectedDatabaseType
        {
            get => _selectedDatabaseType;
            set 
            {
                this.RaiseAndSetIfChanged(ref _selectedDatabaseType, value);
                UpdateSampleConnectionString(value);
            }
        }

        public string ConnectionString
        {
            get => _connectionString;
            set => this.RaiseAndSetIfChanged(ref _connectionString, value);
        }

        public string Query
        {
            get => _query;
            set => this.RaiseAndSetIfChanged(ref _query, value);
        }

        public ObservableCollection<string> OutputFormats { get; } = new ObservableCollection<string> { "JSON", "CSV" };

        public string SelectedFormat
        {
            get => _selectedFormat;
            set => this.RaiseAndSetIfChanged(ref _selectedFormat, value);
        }

        public bool Compress
        {
            get => _compress;
            set => this.RaiseAndSetIfChanged(ref _compress, value);
        }

        public bool Masking
        {
            get => _masking;
            set => this.RaiseAndSetIfChanged(ref _masking, value);
        }

        public string MaskFields
        {
            get => _maskFields;
            set => this.RaiseAndSetIfChanged(ref _maskFields, value);
        }
        
        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }
        
        public string LastExportPath
        {
            get => _lastExportPath;
            set => this.RaiseAndSetIfChanged(ref _lastExportPath, value);
        }

        public ObservableCollection<string> Logs => ObservableSink.Logs;

        public ReactiveCommand<Unit, Unit> ExportCommand { get; }
        public ReactiveCommand<Unit, Unit> TestConnectionCommand { get; }
        public ReactiveCommand<Unit, Unit> TestQueryCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenFolderCommand { get; }

        static bool IsLoggerInitialized = false;

        public MainWindowViewModel()
        {
            if (!IsLoggerInitialized)
            {
                var loggerConfig = new LoggerConfiguration()
                         .WriteTo.Console()
                         .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day);

                // Add ObservableSink for UI
                loggerConfig.WriteTo.Sink(new ObservableSink());

                Log.Logger = loggerConfig.CreateLogger();
                IsLoggerInitialized = true;
            }

            ExportCommand = ReactiveCommand.CreateFromTask(ExecuteExport, this.WhenAnyValue(x => x.IsBusy, busy => !busy));
            TestConnectionCommand = ReactiveCommand.CreateFromTask(ExecuteTestConnection, this.WhenAnyValue(x => x.IsBusy, busy => !busy));
            TestQueryCommand = ReactiveCommand.CreateFromTask(ExecuteTestQuery, this.WhenAnyValue(x => x.IsBusy, busy => !busy));
            OpenFolderCommand = ReactiveCommand.Create(OpenOutputFolder);
            
            UpdateSampleConnectionString(_selectedDatabaseType);
        }

        private void UpdateSampleConnectionString(string dbType)
        {
            string sample = "";
            switch (dbType)
            {
                case "SQL Server": sample = "Server=localhost;Database=mydb;User Id=sa;Password=pass;TrustServerCertificate=True;"; break;
                case "PostgreSQL": sample = "Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=pass;"; break;
                case "MySQL": sample = "Server=localhost;Database=mydb;Uid=root;Pwd=pass;"; break;
                case "Oracle": sample = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=MyHost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=MyOracleSID)));User Id=myUsername;Password=myPassword;"; break;
                case "MariaDB": sample = "Server=localhost;Port=3306;Database=mydb;Uid=root;Pwd=pass;"; break;
                case "IBM Db2": sample = "Server=myServerAddress:50000;Database=mydb;UID=db2admin;PWD=pass;"; break;
                case "Snowflake": sample = "account=myaccount;user=myuser;password=mypassword;db=mydb;schema=public;warehouse=mywarehouse;role=myrole;"; break;
                case "SQLite": sample = "Data Source=mydb.sqlite"; break;
                default: sample = "Data Source=..."; break;
            }
            ConnectionString = sample;
        }

        private IDbConnection CreateConnection(string dbType, string connectionString)
        {
            switch (dbType)
            {
                case "SQL Server": return new SqlConnection(connectionString);
                case "PostgreSQL": return new NpgsqlConnection(connectionString);
                case "MySQL": 
                case "MariaDB": return new MySqlConnection(connectionString);
                case "SQLite": return new SqliteConnection(connectionString);
                default: throw new NotSupportedException($"Database type '{dbType}' is not supported yet or requires specific driver installation.");
            }
        }

        private async Task ExecuteTestConnection()
        {
            IsBusy = true;
            await Task.Run(() =>
            {
                try
                {
                    Log.Information($"Testing connection to {SelectedDatabaseType}...");
                    using (var conn = CreateConnection(SelectedDatabaseType, ConnectionString))
                    {
                        conn.Open();
                        Log.Information("Connection Successful!");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Connection Failed: {ex.Message}");
                }
            });
            IsBusy = false;
        }

        private async Task ExecuteTestQuery()
        {
            IsBusy = true;
            await Task.Run(() =>
            {
                try
                {
                    Log.Information($"Testing query execution...");
                    
                    // Modify query to limit results just for preview
                    string previewQuery = Query;
                    
                    // Simple logic to inject limit if not present. Warning: This is naive.
                    if (!previewQuery.Contains("TOP", StringComparison.OrdinalIgnoreCase) && 
                        !previewQuery.Contains("LIMIT", StringComparison.OrdinalIgnoreCase))
                    {
                        if (SelectedDatabaseType == "SQL Server")
                             previewQuery = $"SELECT TOP 5 * FROM ({Query}) as SubQuery"; 
                             // Note: This nesting works for simple SELECTs, but simplest is just "SELECT TOP 5 ..." if query starts with SELECT
                             // For robustness in this demo, let's rely on user query or Dapper's ability to just fetch some.
                        else
                             previewQuery = $"{Query} LIMIT 5";
                    }

                    using (var conn = CreateConnection(SelectedDatabaseType, ConnectionString))
                    {
                        conn.Open();
                        var result = conn.Query(previewQuery).ToList();
                        
                        Log.Information($"Query executed successfully. Rows returned: {result.Count}");
                        if (result.Count > 0)
                        {
                            Log.Information("Sample Data (First Row):");
                            var firstRow = result[0] as IDictionary<string, object>;
                            if (firstRow != null)
                            {
                                foreach (var kvp in firstRow)
                                {
                                    Log.Information($" - {kvp.Key}: {kvp.Value}");
                                }
                            }
                            else
                            {
                                Log.Information(result[0].ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Query Test Failed: {ex.Message}");
                }
            });
            IsBusy = false;
        }

        private async Task ExecuteExport()
        {
            IsBusy = true;
            LastExportPath = ""; 
            
            await Task.Run(() =>
            {
                try
                {
                    Log.Information($"Starting Export Job for {SelectedDatabaseType}...");

                    var format = SelectedFormat.ToLower();
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var outputFileName = $"export_{timestamp}.{format}";
                    var outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), outputFileName);
                    
                    var job = new ExportJob
                    {
                        JobName = "UI_Export",
                        Query = Query,
                        OutputFormat = format,
                        OutputPath = outputPath,
                        CompressOutput = Compress,
                        MaskFields = Masking 
                            ? MaskFields.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList() 
                            : new List<string>()
                    };

                    Log.Information("Extracting data...");
                    IEnumerable<dynamic> data;
                    
                    using (var conn = CreateConnection(SelectedDatabaseType, ConnectionString))
                    {
                        conn.Open();
                        // Use buffered=true (default) to load into memory, or false for large datasets streaming
                        // For this tool, we load to memory for simplicity of JSON conversion
                        data = conn.Query(job.Query);
                    }

                    // Masking
                    if (job.MaskFields.Count > 0)
                    {
                        Log.Information($"Applying masking...");
                        data = MaskingService.ApplyMasking(data, job.MaskFields);
                    }

                    // Export
                    Log.Information($"Writing to {format.ToUpper()}...");
                    IExporter exporter = format == "csv" 
                        ? (IExporter)new CsvExporter() 
                        : (IExporter)new JsonExporter();

                    exporter.Export(data, job.OutputPath);
                    Log.Information($"File saved: {job.OutputPath}");

                    // Compress
                    if (job.CompressOutput)
                    {
                        Log.Information("Compressing...");
                        var zipPath = job.OutputPath + ".zip";
                        CompressionService.CompressFile(job.OutputPath, zipPath);
                        Log.Information($"Archive ready: {zipPath}");
                        LastExportPath = zipPath;
                    }
                    else
                    {
                        LastExportPath = job.OutputPath;
                    }

                    Log.Information("Success!");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Export process failed");
                }
            });
            IsBusy = false;
        }

        private void OpenOutputFolder()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (!string.IsNullOrEmpty(LastExportPath) && File.Exists(LastExportPath))
                {
                    path = Path.GetDirectoryName(LastExportPath) ?? path;
                }

                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = path,
                        UseShellExecute = true
                    });
                }
                else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                {
                     System.Diagnostics.Process.Start("xdg-open", path);
                }
                else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                {
                     System.Diagnostics.Process.Start("open", path);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not open folder");
            }
        }
    }
}

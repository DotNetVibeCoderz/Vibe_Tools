using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using SimpleETL.Data;
using SimpleETL.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Npgsql;
using MySql.Data.MySqlClient;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Jint;
using System.Data.Odbc;
using Microsoft.Data.Sqlite;
using System.Text.Json.Nodes;

// Semantic Kernel
using Microsoft.SemanticKernel;

namespace SimpleETL.Services
{
    public class FlowEngine
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClient _httpClient;

        public FlowEngine(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public class FlowVariableDef
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = "String";
            public string InitialValue { get; set; } = string.Empty;
        }

        private class FlowDocument
        {
            public List<FlowNode> Nodes { get; set; } = new List<FlowNode>();
            public List<FlowVariableDef> Variables { get; set; } = new List<FlowVariableDef>();
        }

        public class ScriptGlobals
        {
            public IEnumerable<IDictionary<string, object>>? Input { get; set; }
            public IDictionary<string, object>? Variables { get; set; }
        }

        private string GetConfigValue(string configJson, string key)
        {
            if (string.IsNullOrEmpty(configJson)) return "";
            try {
                var doc = JsonNode.Parse(configJson);
                return doc?[key]?.ToString() ?? "";
            } catch { return configJson; } 
        }

        private string ReplaceVariables(string input, Dictionary<string, object> runtimeVars)
        {
            if (string.IsNullOrEmpty(input)) return input;
            foreach (var kvp in runtimeVars)
            {
                input = input.Replace("{{" + kvp.Key + "}}", kvp.Value?.ToString() ?? "");
            }
            return input;
        }

        private object? CastVariable(string type, string value)
        {
            try {
                if (string.IsNullOrEmpty(value)) return null;
                return type switch {
                    "Int" => int.Parse(value),
                    "Double" => double.Parse(value, CultureInfo.InvariantCulture),
                    "Boolean" => bool.Parse(value),
                    _ => value
                };
            } catch { return value; } // fallback to string
        }

        public async Task ExecuteNodesAsync(List<FlowNode> nodes, List<FlowVariableDef> variableDefs, Action<string, string> onNodeStatus, Action<string> onLog, CancellationToken ct)
        {
            IEnumerable<IDictionary<string, object>>? pipelineData = new List<IDictionary<string, object>>();
            var runtimeVars = new Dictionary<string, object>();

            // Initialize Variables
            foreach (var v in variableDefs)
            {
                runtimeVars[v.Name] = CastVariable(v.Type, v.InitialValue) ?? "";
            }

            if (runtimeVars.Count > 0)
                onLog($"[{DateTime.Now:HH:mm:ss}] Initialized {runtimeVars.Count} flow variable(s).");

            foreach (var node in nodes)
            {
                ct.ThrowIfCancellationRequested();
                onNodeStatus(node.Id, "Running");
                onLog($"\n[{DateTime.Now:HH:mm:ss}] Starting: {node.Name} ({node.Type})");

                try
                {
                    // ---- SYSTEM / VARIABLES ----
                    if (node.Type == "Set Variable Value")
                    {
                        var varName = GetConfigValue(node.Config, "VariableName");
                        var rawValue = GetConfigValue(node.Config, "VariableValue");
                        var replacedValue = ReplaceVariables(rawValue, runtimeVars);
                        
                        var def = variableDefs.FirstOrDefault(v => v.Name == varName);
                        if (def != null)
                        {
                            runtimeVars[varName] = CastVariable(def.Type, replacedValue) ?? "";
                            onLog($"[{DateTime.Now:HH:mm:ss}] Set Variable '{varName}' to '{runtimeVars[varName]}'");
                        }
                        else
                        {
                            onLog($"[{DateTime.Now:HH:mm:ss}] Warning: Variable '{varName}' not defined. Skipping.");
                        }
                    }
                    
                    // ---- SOURCES ----
                    else if (node.Type.Contains("Source") && node.Type.Contains("SQL Server"))
                    {
                        pipelineData = await ReadFromDatabaseAsync(
                            new SqlConnection(ReplaceVariables(GetConfigValue(node.Config, "Connection"), runtimeVars)), 
                            ReplaceVariables(GetConfigValue(node.Config, "QueryOrTable"), runtimeVars), onLog, ct);
                    }
                    else if (node.Type.Contains("Source") && node.Type.Contains("PostgreSQL"))
                    {
                        pipelineData = await ReadFromDatabaseAsync(
                            new NpgsqlConnection(ReplaceVariables(GetConfigValue(node.Config, "Connection"), runtimeVars)), 
                            ReplaceVariables(GetConfigValue(node.Config, "QueryOrTable"), runtimeVars), onLog, ct);
                    }
                    else if (node.Type.Contains("Source") && node.Type.Contains("MySQL"))
                    {
                        pipelineData = await ReadFromDatabaseAsync(
                            new MySqlConnection(ReplaceVariables(GetConfigValue(node.Config, "Connection"), runtimeVars)), 
                            ReplaceVariables(GetConfigValue(node.Config, "QueryOrTable"), runtimeVars), onLog, ct);
                    }
                    else if (node.Type.Contains("Source") && node.Type.Contains("SQLite"))
                    {
                        pipelineData = await ReadFromDatabaseAsync(
                            new SqliteConnection(ReplaceVariables(GetConfigValue(node.Config, "Connection"), runtimeVars)), 
                            ReplaceVariables(GetConfigValue(node.Config, "QueryOrTable"), runtimeVars), onLog, ct);
                    }
                    else if (node.Type.Contains("Source") && node.Type.Contains("Oracle"))
                    {
                        pipelineData = await ReadFromDatabaseAsync(
                            new OdbcConnection(ReplaceVariables(GetConfigValue(node.Config, "Connection"), runtimeVars)), 
                            ReplaceVariables(GetConfigValue(node.Config, "QueryOrTable"), runtimeVars), onLog, ct);
                    }
                    else if (node.Type == "CSV Source")
                    {
                        var path = ReplaceVariables(GetConfigValue(node.Config, "Connection"), runtimeVars);
                        onLog($"[{DateTime.Now:HH:mm:ss}] Reading CSV: {path}");
                        using var reader = new StreamReader(path);
                        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                        var records = new List<IDictionary<string, object>>();
                        csv.Read();
                        csv.ReadHeader();
                        while (csv.Read())
                        {
                            var dict = new Dictionary<string, object>();
                            foreach (var header in csv.HeaderRecord!)
                            {
                                dict[header] = csv.GetField(header)!;
                            }
                            records.Add(dict);
                        }
                        pipelineData = records;
                        onLog($"[{DateTime.Now:HH:mm:ss}] Loaded {records.Count} rows from CSV.");
                    }
                    else if (node.Type == "JSON Source")
                    {
                        var path = ReplaceVariables(GetConfigValue(node.Config, "Connection"), runtimeVars);
                        onLog($"[{DateTime.Now:HH:mm:ss}] Reading JSON: {path}");
                        var jsonString = await File.ReadAllTextAsync(path, ct);
                        pipelineData = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonString);
                        onLog($"[{DateTime.Now:HH:mm:ss}] Loaded {pipelineData?.Count() ?? 0} rows from JSON.");
                    }
                    else if (node.Type == "REST API Source")
                    {
                        var url = ReplaceVariables(GetConfigValue(node.Config, "Connection"), runtimeVars);
                        var methodStr = GetConfigValue(node.Config, "HttpMethod");
                        if(string.IsNullOrEmpty(methodStr)) methodStr = "GET";
                        var payload = ReplaceVariables(GetConfigValue(node.Config, "Payload"), runtimeVars);
                        
                        onLog($"[{DateTime.Now:HH:mm:ss}] Fetching data from API: {methodStr} {url}");
                        var request = new HttpRequestMessage(new HttpMethod(methodStr), url);
                        
                        if (!string.IsNullOrEmpty(payload) && (methodStr == "POST" || methodStr == "PUT" || methodStr == "DELETE"))
                        {
                            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
                        }
                        
                        var responseMsg = await _httpClient.SendAsync(request, ct);
                        responseMsg.EnsureSuccessStatusCode();
                        var response = await responseMsg.Content.ReadAsStringAsync(ct);
                        
                        try 
                        {
                            pipelineData = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(response);
                            onLog($"[{DateTime.Now:HH:mm:ss}] Loaded {pipelineData?.Count() ?? 0} objects from API.");
                        } 
                        catch 
                        {
                            onLog($"[{DateTime.Now:HH:mm:ss}] Warning: API didn't return a JSON array. Wrapping in single row.");
                            pipelineData = new List<IDictionary<string, object>> { new Dictionary<string, object> { { "RawResponse", response } } };
                        }
                    }
                    
                    // ---- TRANSFORMATIONS / SCRIPTING ----
                    else if (node.Type == "PowerShell Script")
                    {
                        var script = ReplaceVariables(GetConfigValue(node.Config, "Script"), runtimeVars);
                        onLog($"[{DateTime.Now:HH:mm:ss}] Executing PowerShell Script...");
                        
                        var tempFile = Path.GetTempFileName() + ".ps1";
                        await File.WriteAllTextAsync(tempFile, script, ct);
                        
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = $"-ExecutionPolicy Bypass -NoProfile -File \"{tempFile}\"",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        
                        using var process = Process.Start(startInfo);
                        if (process != null)
                        {
                            var output = await process.StandardOutput.ReadToEndAsync(ct);
                            var error = await process.StandardError.ReadToEndAsync(ct);
                            await process.WaitForExitAsync(ct);
                            
                            if (!string.IsNullOrEmpty(output)) onLog(output);
                            if (!string.IsNullOrEmpty(error)) onLog($"ERROR: {error}");
                        }
                        try { File.Delete(tempFile); } catch { }
                    }
                    else if (node.Type == "Command Line")
                    {
                        var command = ReplaceVariables(GetConfigValue(node.Config, "Script"), runtimeVars);
                        onLog($"[{DateTime.Now:HH:mm:ss}] Executing Command Line...");
                        
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/c {command}",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        
                        using var process = Process.Start(startInfo);
                        if (process != null)
                        {
                            var output = await process.StandardOutput.ReadToEndAsync(ct);
                            var error = await process.StandardError.ReadToEndAsync(ct);
                            await process.WaitForExitAsync(ct);
                            
                            if (!string.IsNullOrEmpty(output)) onLog(output);
                            if (!string.IsNullOrEmpty(error)) onLog($"ERROR: {error}");
                        }
                    }
                    else if (node.Type == "C# Script")
                    {
                        onLog($"[{DateTime.Now:HH:mm:ss}] Executing C# Script...");
                        var options = ScriptOptions.Default.AddImports("System", "System.Collections.Generic", "System.Linq");
                        var globals = new ScriptGlobals { Input = pipelineData, Variables = runtimeVars };
                        var script = GetConfigValue(node.Config, "Script");
                        var result = await CSharpScript.EvaluateAsync(script, options, globals, typeof(ScriptGlobals), ct);
                        if (result is IEnumerable<IDictionary<string, object>> updatedData)
                        {
                            pipelineData = updatedData.ToList();
                        }
                        onLog($"[{DateTime.Now:HH:mm:ss}] C# Script executed successfully. Output rows: {pipelineData?.Count() ?? 0}");
                    }
                    else if (node.Type == "JS Script")
                    {
                        onLog($"[{DateTime.Now:HH:mm:ss}] Executing JS Script...");
                        var engine = new Engine();
                        engine.SetValue("Input", pipelineData);
                        engine.SetValue("Variables", runtimeVars);
                        var script = GetConfigValue(node.Config, "Script");
                        engine.Execute(script + "\n transform(Input);");
                        onLog($"[{DateTime.Now:HH:mm:ss}] JS Script executed successfully.");
                    }
                    else if (node.Type == "Python Script")
                    {
                        onLog($"[{DateTime.Now:HH:mm:ss}] Executing Python Script...");
                        ScriptEngine engine = Python.CreateEngine();
                        ScriptScope scope = engine.CreateScope();
                        scope.SetVariable("data", pipelineData);
                        scope.SetVariable("Variables", runtimeVars);
                        var script = GetConfigValue(node.Config, "Script");
                        engine.Execute(script + "\noutput = transform(data)", scope);
                        var pyResult = scope.GetVariable("output");
                        if (pyResult is IEnumerable<IDictionary<string, object>> res2) {
                            pipelineData = res2;
                        }
                        onLog($"[{DateTime.Now:HH:mm:ss}] Python Script executed successfully.");
                    }
                    else if (node.Type == "Filter")
                    {
                        onLog($"[{DateTime.Now:HH:mm:ss}] Applying Filter...");
                        var condition = ReplaceVariables(GetConfigValue(node.Config, "Script"), runtimeVars);
                        var engine = new Engine();
                        var filtered = new List<IDictionary<string, object>>();
                        if (pipelineData != null)
                        {
                            foreach (var row in pipelineData)
                            {
                                engine.SetValue("row", row);
                                engine.SetValue("Variables", runtimeVars);
                                var isMatch = engine.Evaluate(condition).AsBoolean();
                                if (isMatch) filtered.Add(row);
                            }
                        }
                        pipelineData = filtered;
                        onLog($"[{DateTime.Now:HH:mm:ss}] Filter applied. Rows remaining: {pipelineData?.Count() ?? 0}");
                    }
                    else if (node.Type == "LLM Action")
                    {
                        var model = GetConfigValue(node.Config, "Model");
                        if (string.IsNullOrEmpty(model)) model = "gpt-3.5-turbo";
                        
                        var systemPrompt = ReplaceVariables(GetConfigValue(node.Config, "SystemPrompt"), runtimeVars);
                        var instruction = ReplaceVariables(GetConfigValue(node.Config, "Instruction"), runtimeVars);
                        
                        onLog($"[{DateTime.Now:HH:mm:ss}] Initializing Semantic Kernel with Model: {model}");
                        
                        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
                        if (string.IsNullOrEmpty(apiKey)) 
                        {
                            onLog($"[{DateTime.Now:HH:mm:ss}] Warning: No OPENAI_API_KEY environment variable found. Processing will be skipped/simulated.");
                            await Task.Delay(1000, ct);
                            if (pipelineData != null) {
                                foreach (var row in pipelineData) row["LLM_Processed"] = "API Key Missing";
                            }
                        } 
                        else 
                        {
                            var builder = Kernel.CreateBuilder();
                            builder.AddOpenAIChatCompletion(model, apiKey);
                            var kernel = builder.Build();

                            var skPrompt = systemPrompt + "\n\nInstruction: " + instruction + "\n\nData to process:\n{{$input_data}}";
                            var summarizeFunction = kernel.CreateFunctionFromPrompt(skPrompt);

                            if (pipelineData != null && pipelineData.Any())
                            {
                                int processedCount = 0;
                                // Simple approach: we pass each row as JSON string to the LLM
                                foreach (var row in pipelineData)
                                {
                                    ct.ThrowIfCancellationRequested();
                                    string rowJson = JsonSerializer.Serialize(row);
                                    
                                    var result = await kernel.InvokeAsync(summarizeFunction, new KernelArguments { ["input_data"] = rowJson }, ct);
                                    row["LLM_Response"] = result.GetValue<string>() ?? "";
                                    processedCount++;
                                    
                                    onLog($"[{DateTime.Now:HH:mm:ss}] LLM Processed row {processedCount}/{pipelineData.Count()}");
                                }
                            }
                            onLog($"[{DateTime.Now:HH:mm:ss}] Semantic Kernel LLM execution completed.");
                        }
                    }
                    
                    // ---- DESTINATIONS ----
                    else if (node.Type == "REST API Destination")
                    {
                        var url = ReplaceVariables(GetConfigValue(node.Config, "Connection"), runtimeVars);
                        var methodStr = GetConfigValue(node.Config, "HttpMethod");
                        if(string.IsNullOrEmpty(methodStr)) methodStr = "POST";
                        var payloadTpl = ReplaceVariables(GetConfigValue(node.Config, "Payload"), runtimeVars);
                        
                        onLog($"[{DateTime.Now:HH:mm:ss}] Sending data to API: {methodStr} {url}");
                        
                        string finalPayload = "";
                        if (!string.IsNullOrEmpty(payloadTpl) && payloadTpl.Contains("{{data}}"))
                        {
                            var dataJson = JsonSerializer.Serialize(pipelineData);
                            finalPayload = payloadTpl.Replace("{{data}}", dataJson);
                        }
                        else if (string.IsNullOrEmpty(payloadTpl))
                        {
                            finalPayload = JsonSerializer.Serialize(pipelineData);
                        }
                        else 
                        {
                            finalPayload = payloadTpl;
                        }

                        var request = new HttpRequestMessage(new HttpMethod(methodStr), url);
                        if (methodStr == "POST" || methodStr == "PUT" || methodStr == "PATCH" || methodStr == "DELETE")
                        {
                            request.Content = new StringContent(finalPayload, Encoding.UTF8, "application/json");
                        }
                        
                        var responseMsg = await _httpClient.SendAsync(request, ct);
                        responseMsg.EnsureSuccessStatusCode();
                        onLog($"[{DateTime.Now:HH:mm:ss}] API Destination responded with {responseMsg.StatusCode}");
                    }
                    else if (node.Type.Contains("Destination") && node.Type.Contains("SQL Server"))
                    {
                        await WriteToDatabaseAsync(
                            new SqlConnection(ReplaceVariables(GetConfigValue(node.Config, "Connection"), runtimeVars)), 
                            ReplaceVariables(GetConfigValue(node.Config, "QueryOrTable"), runtimeVars), pipelineData, onLog, ct);
                    }
                    else if (node.Type.Contains("Destination") && node.Type.Contains("PostgreSQL"))
                    {
                        await WriteToDatabaseAsync(
                            new NpgsqlConnection(ReplaceVariables(GetConfigValue(node.Config, "Connection"), runtimeVars)), 
                            ReplaceVariables(GetConfigValue(node.Config, "QueryOrTable"), runtimeVars), pipelineData, onLog, ct);
                    }
                    else if (node.Type.Contains("Destination") && node.Type.Contains("MySQL"))
                    {
                        await WriteToDatabaseAsync(
                            new MySqlConnection(ReplaceVariables(GetConfigValue(node.Config, "Connection"), runtimeVars)), 
                            ReplaceVariables(GetConfigValue(node.Config, "QueryOrTable"), runtimeVars), pipelineData, onLog, ct);
                    }
                    else if (node.Type == "CSV Destination")
                    {
                        var path = ReplaceVariables(GetConfigValue(node.Config, "Connection"), runtimeVars);
                        onLog($"[{DateTime.Now:HH:mm:ss}] Writing CSV: {path}");
                        using var writer = new StreamWriter(path);
                        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                        if (pipelineData != null && pipelineData.Any())
                        {
                            foreach (var key in pipelineData.First().Keys) csv.WriteField(key);
                            csv.NextRecord();
                            foreach (var row in pipelineData)
                            {
                                foreach (var val in row.Values) csv.WriteField(val);
                                csv.NextRecord();
                            }
                        }
                        onLog($"[{DateTime.Now:HH:mm:ss}] Exported rows to CSV successfully.");
                    }
                    else if (node.Type == "JSON Destination")
                    {
                        var path = ReplaceVariables(GetConfigValue(node.Config, "Connection"), runtimeVars);
                        onLog($"[{DateTime.Now:HH:mm:ss}] Writing JSON: {path}");
                        var jsonString = JsonSerializer.Serialize(pipelineData, new JsonSerializerOptions { WriteIndented = true });
                        await File.WriteAllTextAsync(path, jsonString, ct);
                        onLog($"[{DateTime.Now:HH:mm:ss}] Exported to JSON successfully.");
                    }
                    else
                    {
                        onLog($"[{DateTime.Now:HH:mm:ss}] Processing node: {node.Type}");
                        await Task.Delay(200, ct); 
                    }

                    onNodeStatus(node.Id, "Success");
                    onLog($"[{DateTime.Now:HH:mm:ss}] Completed: {node.Name}");
                }
                catch (Exception ex)
                {
                    onNodeStatus(node.Id, "Failed");
                    onLog($"[{DateTime.Now:HH:mm:ss}] ERROR in {node.Name}: {ex.Message}");
                    throw; 
                }
            }
        }

        private async Task<List<IDictionary<string, object>>> ReadFromDatabaseAsync(DbConnection conn, string query, Action<string> onLog, CancellationToken ct)
        {
            var result = new List<IDictionary<string, object>>();
            onLog($"[{DateTime.Now:HH:mm:ss}] Connecting to DB: {conn.GetType().Name}");
            await conn.OpenAsync(ct);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = query;
            onLog($"[{DateTime.Now:HH:mm:ss}] Executing Query: {query}");
            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }
                result.Add(row);
            }
            onLog($"[{DateTime.Now:HH:mm:ss}] Loaded {result.Count} rows from Database.");
            return result;
        }

        private async Task WriteToDatabaseAsync(DbConnection conn, string tableName, IEnumerable<IDictionary<string, object>>? data, Action<string> onLog, CancellationToken ct)
        {
            if (data == null || !data.Any())
            {
                onLog($"[{DateTime.Now:HH:mm:ss}] No data to insert into {tableName}.");
                return;
            }

            onLog($"[{DateTime.Now:HH:mm:ss}] Connecting to DB: {conn.GetType().Name}");
            await conn.OpenAsync(ct);
            
            var columns = string.Join(", ", data.First().Keys);
            var parameters = string.Join(", ", data.First().Keys.Select(k => "@" + k.Replace(" ", "")));
            var insertQuery = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";
            
            onLog($"[{DateTime.Now:HH:mm:ss}] Batch Inserting into {tableName}...");
            int count = 0;
            using var transaction = await conn.BeginTransactionAsync(ct);
            foreach (var row in data)
            {
                using var cmd = conn.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = insertQuery;
                foreach (var kvp in row)
                {
                    var param = cmd.CreateParameter();
                    param.ParameterName = "@" + kvp.Key.Replace(" ", "");
                    param.Value = kvp.Value ?? DBNull.Value;
                    cmd.Parameters.Add(param);
                }
                await cmd.ExecuteNonQueryAsync(ct);
                count++;
            }
            await transaction.CommitAsync(ct);
            onLog($"[{DateTime.Now:HH:mm:ss}] Successfully inserted {count} rows into {tableName}.");
        }

        public async Task ExecuteFlowAsync(int flowId)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var flow = await db.DataFlows.FindAsync(flowId);
            
            if (flow == null) return;

            var logEntry = new JobLog
            {
                DataFlowId = flow.Id,
                StartTime = DateTime.Now,
                Status = "Running"
            };
            db.JobLogs.Add(logEntry);
            await db.SaveChangesAsync();

            List<FlowNode> nodes = new List<FlowNode>();
            List<FlowVariableDef> variables = new List<FlowVariableDef>();

            if (!string.IsNullOrEmpty(flow.JsonDesign))
            {
                var json = flow.JsonDesign.TrimStart();
                if (json.StartsWith("[")) {
                    nodes = JsonSerializer.Deserialize<List<FlowNode>>(json) ?? new List<FlowNode>();
                } else {
                    var doc = JsonSerializer.Deserialize<FlowDocument>(json);
                    if (doc != null) {
                        nodes = doc.Nodes;
                        variables = doc.Variables;
                    }
                }
            }

            if (nodes.Count == 0)
            {
                logEntry.Details = "Pipeline is empty.";
                logEntry.Status = "Success";
                logEntry.EndTime = DateTime.Now;
            }
            else
            {
                var fullLog = "Pipeline execution started.\n";

                try
                {
                    using var cts = new CancellationTokenSource();
                    await ExecuteNodesAsync(
                        nodes, 
                        variables,
                        (nodeId, status) => { }, 
                        (msg) => fullLog += msg + "\n", 
                        cts.Token
                    );

                    logEntry.Details = fullLog + $"\n[{DateTime.Now:HH:mm:ss}] Pipeline executed successfully across {nodes.Count} nodes.";
                    logEntry.Status = "Success";
                    logEntry.EndTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    logEntry.Details = fullLog + $"\n\nFATAL ERROR: {ex.Message}";
                    logEntry.Status = "Failed";
                    logEntry.EndTime = DateTime.Now;
                    logEntry.ErrorMessage = ex.Message;
                }
            }

            db.JobLogs.Update(logEntry);
            await db.SaveChangesAsync();
        }
    }
}

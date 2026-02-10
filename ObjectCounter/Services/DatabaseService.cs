using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using ObjectCounter.Models;

namespace ObjectCounter.Services
{
    public class DatabaseService
    {
        private const string ConnectionString = "Data Source=object_counter.db";

        public DatabaseService()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Logs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Timestamp TEXT NOT NULL,
                        ObjectType TEXT NOT NULL,
                        Count INTEGER NOT NULL,
                        Source TEXT NOT NULL
                    );
                ";
                command.ExecuteNonQuery();
            }
        }

        public void LogCount(string objectType, int count, string source)
        {
            try
            {
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT INTO Logs (Timestamp, ObjectType, Count, Source)
                        VALUES ($timestamp, $objectType, $count, $source);
                    ";
                    command.Parameters.AddWithValue("$timestamp", DateTime.Now.ToString("o"));
                    command.Parameters.AddWithValue("$objectType", objectType);
                    command.Parameters.AddWithValue("$count", count);
                    command.Parameters.AddWithValue("$source", source);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Log Error: {ex.Message}");
            }
        }

        public List<LogEntry> GetLogs(int limit = 100)
        {
            var logs = new List<LogEntry>();
            try
            {
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT Id, Timestamp, ObjectType, Count, Source FROM Logs ORDER BY Id DESC LIMIT $limit";
                    command.Parameters.AddWithValue("$limit", limit);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new LogEntry
                            {
                                Id = reader.GetInt32(0),
                                Timestamp = DateTime.Parse(reader.GetString(1)),
                                ObjectType = reader.GetString(2),
                                Count = reader.GetInt32(3),
                                Source = reader.GetString(4)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Read Error: {ex.Message}");
            }
            return logs;
        }

        public void ClearLogs()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Logs";
                command.ExecuteNonQuery();
            }
        }
    }
}

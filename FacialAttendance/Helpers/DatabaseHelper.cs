using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;
using Microsoft.Data.Sqlite;
using FacialAttendance.Models;

namespace FacialAttendance.Helpers
{
    public static class DatabaseHelper
    {
        private static string DbFile = "Data/facial_attendance.db";
        private static string ConnectionString = $"Data Source={DbFile}";

        public static void Initialize()
        {
            if (!Directory.Exists("Data")) Directory.CreateDirectory("Data");
            if (!Directory.Exists("Faces")) Directory.CreateDirectory("Faces");

            using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();
                
                string sqlUsers = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";

                string sqlAttendance = @"
                    CREATE TABLE IF NOT EXISTS Attendance (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER,
                        Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(UserId) REFERENCES Users(Id)
                    );";

                conn.Execute(sqlUsers);
                conn.Execute(sqlAttendance);
            }
        }

        public static int AddUser(string name)
        {
            using (var conn = new SqliteConnection(ConnectionString))
            {
                string sql = "INSERT INTO Users (Name, CreatedAt) VALUES (@Name, @CreatedAt); SELECT last_insert_rowid();";
                return conn.ExecuteScalar<int>(sql, new { Name = name, CreatedAt = DateTime.Now });
            }
        }

        public static void UpdateUser(int id, string name)
        {
            using (var conn = new SqliteConnection(ConnectionString))
            {
                string sql = "UPDATE Users SET Name = @Name WHERE Id = @Id";
                conn.Execute(sql, new { Name = name, Id = id });
            }
        }

        public static List<User> GetUsers()
        {
            using (var conn = new SqliteConnection(ConnectionString))
            {
                return conn.Query<User>("SELECT * FROM Users").ToList();
            }
        }

        public static void LogAttendance(int userId)
        {
            using (var conn = new SqliteConnection(ConnectionString))
            {
                // Duplicate check (e.g., within last 1 minute)
                var lastLog = conn.QueryFirstOrDefault<DateTime?>("SELECT Timestamp FROM Attendance WHERE UserId = @UserId ORDER BY Timestamp DESC LIMIT 1", new { UserId = userId });
                
                if (lastLog.HasValue && (DateTime.Now - lastLog.Value).TotalMinutes < 1)
                {
                    return; // Already logged recently
                }

                conn.Execute("INSERT INTO Attendance (UserId, Timestamp) VALUES (@UserId, @Timestamp)", new { UserId = userId, Timestamp = DateTime.Now });
            }
        }

        public static List<AttendanceLog> GetAttendanceLogs()
        {
            using (var conn = new SqliteConnection(ConnectionString))
            {
                string sql = @"
                    SELECT a.Id, a.UserId, a.Timestamp, u.Name as UserName 
                    FROM Attendance a 
                    JOIN Users u ON a.UserId = u.Id 
                    ORDER BY a.Timestamp DESC";
                return conn.Query<AttendanceLog>(sql).ToList();
            }
        }
        
        public static void DeleteUser(int userId)
        {
             using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Execute("DELETE FROM Attendance WHERE UserId = @Id", new { Id = userId });
                conn.Execute("DELETE FROM Users WHERE Id = @Id", new { Id = userId });
            }

            // Also delete images
            string userDir = Path.Combine("Faces", userId.ToString());
            if (Directory.Exists(userDir))
            {
                Directory.Delete(userDir, true);
            }
        }
    }
}